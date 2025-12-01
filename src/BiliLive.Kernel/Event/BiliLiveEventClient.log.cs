using System.Text.Json;

using BiliLive.Kernel.Event.Models;

using Microsoft.Extensions.Logging;

namespace BiliLive.Kernel.Danmaku;

public abstract partial class BiliLiveEventClient
{
    [LoggerMessage(LogLevel.Information, "人气值: {hot}")]
    private partial void LogHot(int hot);

    [LoggerMessage(LogLevel.Information, "接收到通知包 操作类型: {operation}")]
    private partial void LogReceivedPack(BiliLiveEventOperation operation);

    [LoggerMessage(LogLevel.Information, "接收到通知包 数据类型: {type}")]
    private partial void LogNotificationType(BiliLiveEventPackBodyType type);

    [LoggerMessage(LogLevel.Information, "进入房间成功: {code}")]
    private partial void LogEnterRoomSucceed(int code);

    [LoggerMessage(LogLevel.Information, "进入房间失败: {json}")]
    private partial void LogEnterRoomFailed(JsonElement json);

    [LoggerMessage(LogLevel.Information, "收到弹幕: {user}: {content}")]
    private partial void LogDanmaku(string? user, string? content);

    [LoggerMessage(LogLevel.Trace, "接收到命令: {cmd}")]
    private partial void LogCommand(string? cmd);

    [LoggerMessage(LogLevel.Debug, "接收到命令:\r\n\r\n{json}")]
    private partial void LogCommandDebug(string json);

    [LoggerMessage(LogLevel.Information, "{type} ({cmd})")]
    private partial void LogCommandType(string type, string cmd);

    [LoggerMessage(LogLevel.Information, "欢迎进入直播间 ({cmd})")]
    private partial void LogWelcomeCommand(string cmd);

    [LoggerMessage(LogLevel.Warning, "接收到未知的命令: {cmd}")]
    private partial void LogUnknownCommandType(string? cmd);

    [LoggerMessage(LogLevel.Information, "进入房间: {roomId}; 当前用户: {userId}")]
    protected partial void LogEnterRoom(int roomId, long userId);

}
