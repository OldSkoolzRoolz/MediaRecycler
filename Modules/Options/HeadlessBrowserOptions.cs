// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers

#region Header

// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers

#endregion

using System.Configuration;

namespace MediaRecycler.Modules.Options;

public class HeadlessBrowserOptions : ApplicationSettingsBase
{
    private static HeadlessBrowserOptions? _defaultInstance;
    public static HeadlessBrowserOptions Default
    {
        get
        {
            _defaultInstance ??= (HeadlessBrowserOptions)Synchronized(new HeadlessBrowserOptions());
            return _defaultInstance;
        }
    }

    public void SaveSettings() => Save();
    public void ReloadSettings() => Reload();

    [UserScopedSetting]
    [DefaultSettingValue("true")]
    public bool Headless
    {
        get => this[nameof(Headless)] is not bool v || v;
        set => this[nameof(Headless)] = value;
    }

    [UserScopedSetting]
    public string? ExecutablePath
    {
        get => this[nameof(ExecutablePath)] as string;
        set => this[nameof(ExecutablePath)] = value;
    }

    [UserScopedSetting]
    public string? Args // Store as comma-separated string for compatibility
    {
        get => this[nameof(Args)] as string;
        set => this[nameof(Args)] = value;
    }

    [UserScopedSetting]
    public string? UserDataDir
    {
        get => this[nameof(UserDataDir)] as string;
        set => this[nameof(UserDataDir)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("false")]
    public bool Devtools
    {
        get => this[nameof(Devtools)] is bool v && v;
        set => this[nameof(Devtools)] = value;
    }

    [UserScopedSetting]
    public string? DefaultViewport
    {
        get => this[nameof(DefaultViewport)] as string;
        set => this[nameof(DefaultViewport)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("false")]
    public bool IgnoreHTTPSErrors
    {
        get => this[nameof(IgnoreHTTPSErrors)] is bool v && v;
        set => this[nameof(IgnoreHTTPSErrors)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("30000")]
    public int Timeout
    {
        get => this[nameof(Timeout)] is int v ? v : 30000;
        set => this[nameof(Timeout)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("false")]
    public bool DumpIO
    {
        get => this[nameof(DumpIO)] is bool v && v;
        set => this[nameof(DumpIO)] = value;
    }

    [UserScopedSetting]
    public string? ProxyServer
    {
        get => this[nameof(ProxyServer)] as string;
        set => this[nameof(ProxyServer)] = value;
    }

    [UserScopedSetting]
    public string? ProxyBypassList
    {
        get => this[nameof(ProxyBypassList)] as string;
        set => this[nameof(ProxyBypassList)] = value;
    }

    [UserScopedSetting]
    public string? BrowserURL
    {
        get => this[nameof(BrowserURL)] as string;
        set => this[nameof(BrowserURL)] = value;
    }

    [UserScopedSetting]
    public string? Product
    {
        get => this[nameof(Product)] as string;
        set => this[nameof(Product)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("false")]
    public bool IgnoreDefaultArgs
    {
        get => this[nameof(IgnoreDefaultArgs)] is bool v && v;
        set => this[nameof(IgnoreDefaultArgs)] = value;
    }

    [UserScopedSetting]
    public string? IgnoredDefaultArgs // Store as comma-separated string
    {
        get => this[nameof(IgnoredDefaultArgs)] as string;
        set => this[nameof(IgnoredDefaultArgs)] = value;
    }

    [UserScopedSetting]
    public string? Channel
    {
        get => this[nameof(Channel)] as string;
        set => this[nameof(Channel)] = value;
    }

    [UserScopedSetting]
    public string? Protocol
    {
        get => this[nameof(Protocol)] as string;
        set => this[nameof(Protocol)] = value;
    }

    [UserScopedSetting]
    public string? Path
    {
        get => this[nameof(Path)] as string;
        set => this[nameof(Path)] = value;
    }

    [UserScopedSetting]
    public string? RemoteDebuggingPort
    {
        get => this[nameof(RemoteDebuggingPort)] as string;
        set => this[nameof(RemoteDebuggingPort)] = value;
    }

    [UserScopedSetting]
    public string? RemoteDebuggingAddress
    {
        get => this[nameof(RemoteDebuggingAddress)] as string;
        set => this[nameof(RemoteDebuggingAddress)] = value;
    }

    [UserScopedSetting]
    public string? RemoteDebuggingPipe
    {
        get => this[nameof(RemoteDebuggingPipe)] as string;
        set => this[nameof(RemoteDebuggingPipe)] = value;
    }

    [UserScopedSetting]
    public string? WebSocketEndpoint
    {
        get => this[nameof(WebSocketEndpoint)] as string;
        set => this[nameof(WebSocketEndpoint)] = value;
    }

    [UserScopedSetting]
    public string? UserAgent
    {
        get => this[nameof(UserAgent)] as string;
        set => this[nameof(UserAgent)] = value;
    }

    [UserScopedSetting]
    public string? Language
    {
        get => this[nameof(Language)] as string;
        set => this[nameof(Language)] = value;
    }

    [UserScopedSetting]
    public string? WindowSize
    {
        get => this[nameof(WindowSize)] as string;
        set => this[nameof(WindowSize)] = value;
    }

    [UserScopedSetting]
    public string? WindowPosition
    {
        get => this[nameof(WindowPosition)] as string;
        set => this[nameof(WindowPosition)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("false")]
    public bool NoSandbox
    {
        get => this[nameof(NoSandbox)] is bool v && v;
        set => this[nameof(NoSandbox)] = value;
    }

    // ...repeat for all other properties, using [UserScopedSetting] and [DefaultSettingValue] as needed...
}
