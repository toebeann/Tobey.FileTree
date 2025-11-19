using BepInEx;
using ByteSizeLib;
#if !IL2CPP
using MonoMod.Utils;
using SymbolicLinkSupport;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tobey.FileTree.ExtensionMethods;

namespace Tobey.FileTree;

internal sealed class Node
{
    private readonly string path;
    private readonly string name;
    private readonly Node parent;
    private readonly List<Node> children;
    private readonly bool isDirectory;
    private readonly bool isFile;

    public Node(string path, IEnumerable<string> exclude)
    {
        if (path is not null)
        {
            this.path = path;
            isDirectory = File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            isFile = !isDirectory;
        }

        name = Path.GetFileName(this.path);

        children = GetChildren(exclude);
    }

    private Node(string path, Node parent, IEnumerable<string> exclude)
    {
        this.parent = parent;

        if (path is not null)
        {
            this.path = path;
            isDirectory = File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            isFile = !isDirectory;
        }

        name = Path.GetFileName(this.path);

        children = GetChildren(exclude);
    }

    private Node(string name, Node parent)
    {
        this.parent = parent;
        this.name = name;
        children = [];
    }

    private List<Node> GetChildren(IEnumerable<string> exclude)
    {
        if (isFile) return [];

        if (exclude.Contains(name.ToLowerInvariant()))
            return [new("(contents not shown)", this)];

        return [..Directory.GetDirectories(path)
            .Concat(Directory.GetFiles(path)
                .Where(file => !exclude.Contains(System.IO.Path.GetFileName(file).ToLowerInvariant())))
            .Select(p => new Node(p, this, exclude))];
    }

    public void PrettyPrint(Action<string> printer, string indent = null, bool last = false)
    {
        indent ??= string.Empty;
        string output = indent;

        if (parent is not null)
        {
            output += last
                ? @"\-- "
                : "|-- ";
        }

        FileInfo info;
        bool isSymbolicLink;
        ByteSize? size;

        if (path is not null)
        {
            try { info = new(path); }
            catch { info = null; }

            if (info is not null)
            {
                try
                {
                    isSymbolicLink =
#if !IL2CPP
                        PlatformHelper.Is(Platform.Windows) &&
#endif
                        info.IsSymbolicLink();
                }
                catch { isSymbolicLink = false; }
            }
            else isSymbolicLink = false;

            if (isFile)
            {
                try
                {
                    size = isSymbolicLink switch
                    {
                        true => ByteSize.FromBytes(new FileInfo(info.Resolve().FullName).Length),
                        false => ByteSize.FromBytes(info.Length),
                    };
                }
                catch
                {
                    size = null;
                }
            }
            else size = null;
        }
        else
        {
            info = null;
            isSymbolicLink = false;
            size = null;
        }

        var suffix = string.Join(" ", [.. new[] {
            size is ByteSize ? $"[{size:0.##}]" : null,
            isSymbolicLink ? "[symlink]" : null,
        }.Where(s => s is not null)]);

        printer.Invoke($"{output}{(parent is null ? path : name)}{(suffix.IsNullOrWhiteSpace() ? string.Empty : $" {suffix}")}");

        if (parent is not null)
        {
            indent += last
                ? "    "
                : "|   ";
        }

        int? firstFileIndex = children.FirstOrDefault(child => child.isFile) switch
        {
            Node file => children.IndexOf(file),
            _ => null
        };

        for (int i = 0; i < children.Count; i++)
        {
            if (i == 0 || children[i].isDirectory || firstFileIndex is null || i == firstFileIndex)
            {
                printer.Invoke($"{indent}|");
            }

            children[i].PrettyPrint(printer, indent, i == children.Count - 1);
        }
    }
}
