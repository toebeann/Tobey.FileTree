using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if IL2CPP
using System.Threading.Tasks;
using BaseUnityPlugin = BepInEx.Unity.IL2CPP.BasePlugin;
using PluginInfo = Tobey.FileTree.MyPluginInfo;
#else
using UnityEngine;
#endif

namespace Tobey.FileTree;
using ExtensionMethods;

#if !IL2CPP
[DisallowMultipleComponent]
#endif
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class FileTree : BaseUnityPlugin
{
    private ConfigEntry<bool> enabledConfig;
    private ConfigEntry<string> whitelistDirsConfig;

    private bool Enabled => enabledConfig switch
    {
        { Value: true } => true,
        _ => false
    };

    private IEnumerable<string> WhitelistDirs => whitelistDirsConfig switch
    {
        { Value: string s } => s.Split(',').Select(s => s.Trim().ToLowerInvariant()),
        _ => []
    };

#if IL2CPP
    public override void Load()
#else
    private void Awake()
#endif
    {
        enabledConfig = Config.Bind(
            section: "General",
            key: "Enabled",
            defaultValue: true,
            configDescription: new(
                description: "When enabled, the plugin will log the file tree to the console.",
                tags: [new ConfigurationManagerAttributes { IsAdvanced = true }]));

        whitelistDirsConfig = Config.Bind(
            section: "General",
            key: "Directory whitelist",
            defaultValue: "BepInEx",
            configDescription: new(
                description: "Case-insensitive list of whitelisted directories, separated by commas.",
                tags: [new ConfigurationManagerAttributes { IsAdvanced = true }]));

        Enabled_SettingChanged(this, null);
        enabledConfig.SettingChanged += Enabled_SettingChanged;
    }

    private void Enabled_SettingChanged(object _, EventArgs __)
    {
#if IL2CPP
        if (Enabled) Run();
#else
        enabled = Enabled;
#endif
    }

#if IL2CPP
    public override bool Unload()
#else
    private void OnDestroy()
#endif
    {
        if (enabledConfig is not null)
        {
            enabledConfig.SettingChanged -= Enabled_SettingChanged;
        }

#if IL2CPP
        return true;
#endif
    }

#if IL2CPP
    private void Run() => Task.Run(() =>
#else
    private void OnEnable() => ThreadingHelper.Instance.StartAsyncInvoke(() =>
#endif
    {
        var rootPath = new FileInfo(Paths.GameRootPath).Resolve()?.FullName ?? Paths.GameRootPath;

        var excludeDirs = Directory.GetDirectories(rootPath)
            .Select(dir => Path.GetFileName(dir).ToLowerInvariant())
            .Where(dir => !WhitelistDirs.Contains(dir));

#if IL2CPP
        return new Node(rootPath, excludeDirs);
    }).ContinueWith(root => root.Result.PrettyPrint(Log.LogInfo));
#else
    var root = new Node(rootPath, excludeDirs);
        return () => root.PrettyPrint(Logger.LogInfo);
    });
#endif
}
