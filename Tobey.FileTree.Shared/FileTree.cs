using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tobey.FileTree.ExtensionMethods;
#if IL2CPP
using System.Threading.Tasks;
using BaseUnityPlugin = BepInEx.Unity.IL2CPP.BasePlugin;
using PluginInfo = Tobey.FileTree.MyPluginInfo;
#else
using UnityEngine;
#endif

namespace Tobey.FileTree;

#if !IL2CPP
[DisallowMultipleComponent]
#endif
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public sealed class FileTree : BaseUnityPlugin
{
    private ManualLogSource LogSource;
    private ConfigEntry<bool> enabledConfig;
    private ConfigEntry<string> whitelistDirsConfig;

#if IL2CPP
    public override void Load()
#else
    private void Awake()
#endif
    {
#if IL2CPP
        LogSource = Log;
#else
        LogSource = Logger;
#endif

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
        var isEnabled = enabledConfig switch
        {
            { Value: true } => true,
            _ => false
        };

        LogSource.LogInfo($"File Tree logging is {(isEnabled ? "enabled" : "disabled")}.");

#if IL2CPP
        if (isEnabled) OnEnable();
#else
        enabled = isEnabled;
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

    private void OnEnable()
    {
#if IL2CPP
        Task.Run(() =>
#else
        ThreadingHelper.Instance.StartAsyncInvoke(() =>
#endif
        {
            var rootPath = new FileInfo(Paths.GameRootPath).Resolve()?.FullName ?? Paths.GameRootPath;

            var whitelistDirs = whitelistDirsConfig switch
            {
                { Value: string s } => s.Split(',').Select(s => s.Trim().ToLowerInvariant()),
                _ => []
            };

            var excludeDirs = Directory.GetDirectories(rootPath)
                .Select(dir => Path.GetFileName(dir).ToLowerInvariant())
                .Where(dir => !whitelistDirs.Contains(dir));

            List<string> lines = [];

            var root = new Node(rootPath, excludeDirs);
            root.PrettyPrint(lines.Add);
#if IL2CPP
            lines
#else
            return () => lines
#endif
                .ForEach(LogSource.LogInfo);
        });
    }
}
