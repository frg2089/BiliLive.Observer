-- 全局对象
obs = obslua
local is_windows = package.config:sub(1, 1) == "\\"
local asmPath = script_path() .. '/BiliLive.Service.dll'

local ctx = {
    settings = nil,
}

local function get_execute_path()
    if obs.obs_data_get_bool(ctx.settings, 'use_apphost') then
        ---@type string
        local apphost = obs.obs_data_get_string(ctx.settings, 'apphost')
        if apphost == '' then
            print('应用程序主机路径不能为空')
            obs.obs_data_set_bool(ctx.settings, 'use_apphost', false)
            return get_execute_path()
        end
        if string.find(apphost, ' ') then
            -- 如果有空格，用双引号包围
            apphost = '"' .. apphost .. '"'
        end
        ---@type string
        local cmd = apphost
        cmd = cmd .. ' '
        cmd = cmd .. '--apphost=' .. apphost
        return cmd
    else
        ---@type string
        local dotnet = obs.obs_data_get_string(ctx.settings, 'dotnet')
        if string.find(dotnet, ' ') then
            -- 如果有空格，用双引号包围
            dotnet = '"' .. dotnet .. '"'
        end

        ---@type string
        local cmd = dotnet
        cmd = cmd .. ' '
        cmd = cmd .. '"' .. obs.obs_data_get_string(ctx.settings, 'assembly') .. '"'
        cmd = cmd .. ' '
        cmd = cmd .. '--dotnet=' .. dotnet
        return cmd
    end
end

-- 脚本描述信息
function script_description()
    return 'OBS Bilibili 开播工具'
end

-- 脚本加载时调用
function script_load(settings)
    ctx.settings = settings

    ---@type string
    local cmd = get_execute_path()
    cmd = cmd .. ' '
    cmd = cmd .. '--urls "http://localhost:' .. obs.obs_data_get_int(ctx.settings, 'port') .. '"'
    cmd = cmd .. ' '
    cmd = cmd .. '--ALLOW-CLOSE-BY-API'
    print(cmd)
    os.execute(cmd)
end

-- 脚本卸载时调用
function script_unload()
    ---@type string
    local dotnet = obs.obs_data_get_string(ctx.settings, 'dotnet')
    if string.find(dotnet, ' ') then
        -- 如果有空格，用双引号包围
        dotnet = '"' .. dotnet .. '"'
    end

    ---@type string
    local cmd = get_execute_path()
    cmd = cmd .. ' '
    cmd = cmd .. '--EXIT'
    print(cmd)
    os.execute(cmd)
end

-- 脚本默认设置（用于注册可配置参数）
function script_defaults(settings)
    local dotnetRuntime = 'dotnet'
    if is_windows then
        dotnetRuntime = 'dotnet.exe'
    end

    obs.obs_data_set_default_string(settings, 'dotnet', dotnetRuntime)
    obs.obs_data_set_default_int(settings, 'port', 5000)
    obs.obs_data_set_default_string(settings, 'assembly', asmPath)
    obs.obs_data_set_default_bool(settings, 'use_apphost', false)
    obs.obs_data_set_default_string(settings, 'apphost', '')
end

-- 脚本属性（在 OBS 脚本设置界面显示）
function script_properties()
    local dotnetRuntime = 'dotnet'
    local apphostFilter = '*'
    if is_windows then
        dotnetRuntime = 'dotnet.exe'
        apphostFilter = '*.exe'
    end

    local props = obs.obs_properties_create()
    obs.obs_properties_add_bool(props, 'use_apphost', '使用应用程序主机')
    obs.obs_properties_add_path(props, 'apphost', '应用程序主机路径', obs.OBS_PATH_FILE, apphostFilter, script_path())

    obs.obs_properties_add_path(props, 'dotnet', '.Net 运行时', obs.OBS_PATH_FILE, dotnetRuntime, dotnetRuntime)
    obs.obs_properties_add_path(props, 'assembly', '程序集', obs.OBS_PATH_FILE, '*.dll', asmPath)
    obs.obs_properties_add_int(props, 'port', '端口号', 1, 65535, 1)
    return props
end
