
using System.Diagnostics;
using System.IO.Pipes;

namespace BiliLive.Service;

public sealed class SingleInstanceService : IHostedService, IDisposable
{
    private const string Name = "BiliLive.Service";
    private NamedPipeServerStream? _stream;

    public static async Task BeforeInitAsync(string[] args)
    {
        if (args.Any(i => i.Equals("--EXIT")))
        {
            await KillOldProcess(CancellationToken.None);
            Environment.Exit(0);
        }
        else if (args.Any(i => i.StartsWith("--dotnet=")))
        {
            var dotnet = args.First(i => i.StartsWith("--dotnet="))["--dotnet=".Length..];
            var arguments = Environment.GetCommandLineArgs()
                .Where(i => !i.StartsWith("--dotnet="));
            Process.Start(startInfo: new(dotnet, arguments)
            {
                CreateNoWindow = true,
            });
            Environment.Exit(0);
        }
        else if (args.Any(i => i.StartsWith("--apphost=")))
        {
            var apphost = args.First(i => i.StartsWith("--apphost="))["--apphost=".Length..];
            var arguments = Environment.GetCommandLineArgs()
                .Skip(1)
                .Where(i => !i.StartsWith("--apphost="));
            Process.Start(startInfo: new(apphost, arguments)
            {
                CreateNoWindow = true,
            });
            Environment.Exit(0);
        }
    }

    public static async Task KillOldProcess(CancellationToken cancellationToken)
    {
        try
        {
            await using NamedPipeClientStream client = new(Name);
            await client.ConnectAsync(TimeSpan.FromSeconds(1), cancellationToken);
            byte[] buffer = GC.AllocateUninitializedArray<byte>(sizeof(int));
            await client.ReadExactlyAsync(buffer, cancellationToken);
            var proc = Process.GetProcessById(BitConverter.ToInt32(buffer));
            proc.Kill();
        }
        catch (TimeoutException)
        {
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await KillOldProcess(cancellationToken);

        await Task.Delay(1, cancellationToken);
        _stream = new(Name, PipeDirection.InOut, 2);
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await _stream.WaitForConnectionAsync(cancellationToken);
                await _stream.WriteAsync(BitConverter.GetBytes(Environment.ProcessId), cancellationToken);
                await _stream.FlushAsync(cancellationToken);
                _stream.Disconnect();
            }
        }, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_stream is null)
            return;

        _stream.Disconnect();
        await _stream.DisposeAsync();
    }

    public void Dispose() => _stream?.Dispose();
}
