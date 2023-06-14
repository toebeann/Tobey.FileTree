using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Tobey.FileTree;
[DisallowMultipleComponent]
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
        _ => Enumerable.Empty<string>()
    };

    private void Awake()
    {
        enabledConfig = Config.Bind(
            section: "General",
            key: "Enabled",
            defaultValue: true,
            configDescription: new(
                description: "When enabled, the plugin will log the file tree to the console.",
                tags: new[] { new ConfigurationManagerAttributes { IsAdvanced = true } }));

        whitelistDirsConfig = Config.Bind(
            section: "General",
            key: "Directory whitelist",
            defaultValue: "BepInEx",
            configDescription: new(
                description: "Case-insensitive list of whitelisted directories, separated by commas.",
                tags: new[] { new ConfigurationManagerAttributes { IsAdvanced = true } }));

        enabledConfig.SettingChanged += _enabled_SettingChanged;
        _enabled_SettingChanged(this, null);
    }

    private void _enabled_SettingChanged(object _, System.EventArgs __) => enabled = Enabled;

    private void OnDestroy()
    {
        if (enabledConfig is not null)
        {
            enabledConfig.SettingChanged -= _enabled_SettingChanged;
        }
    }

    private void OnEnable() => ThreadingHelper.Instance.StartAsyncInvoke(() =>
    {
        var excludeDirs = Directory.GetDirectories(Paths.GameRootPath)
            .Select(dir => Path.GetFileName(dir).ToLowerInvariant())
            .Where(dir => !WhitelistDirs.Contains(dir));

        var root = new Node(Paths.GameRootPath, excludeDirs);
        return () => root.PrettyPrint(Logger.LogMessage);
    });
}
