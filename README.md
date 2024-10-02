# Tobey's File Tree Logger for BepInEx

A configurable file tree logger for troubleshooting BepInEx installations.

## Usage

Generally speaking, you shouldn't need to do much beyond plopping the contents of the downloaded .zip from [the releases page](https://github.com/toebeann/Tobey.FileTree/releases) into your game folder (after installing [BepInEx](https://github.com/BepInEx/BepInEx), of course).

However, if manual configuration is desired, there are several configuration options which can be edited in-game with [Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager) (requires enabling "Advanced settings"), or by editing the file `BepInEx` > `config` > `Tobey.FileTree.cfg`:

```cfg
## Settings file was created by plugin File Tree v1.0.0
## Plugin GUID: Tobey.FileTree

[General]

## When enabled, the plugin will log the file tree to the console.
# Setting type: Boolean
# Default value: true
Enabled = true

## Case-insensitive list of whitelisted directories, separated by commas.
# Setting type: String
# Default value: BepInEx
Directory whitelist = BepInEx
```
