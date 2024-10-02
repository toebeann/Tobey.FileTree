using BepInEx;
using ByteSizeLib;
using SymbolicLinkSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Tobey.FileTree;
using ExtensionMethods;
public class Node
{
    public readonly string Path;
    public readonly string Name;
    public readonly List<Node> Children;
    public readonly Node Parent;
    public bool IsRoot => Parent is null;
    public bool IsDirectory => Path is not null && File.GetAttributes(Path).HasFlag(FileAttributes.Directory);
    public bool IsFile => Path is not null && !File.GetAttributes(Path).HasFlag(FileAttributes.Directory);
    public bool IsSymbolicLink
    {
        get
        {
            try
            {
                return
                    Path is not null &&
                    Application.platform == RuntimePlatform.WindowsPlayer &&
                    new FileInfo(Path).IsSymbolicLink();
            }
            catch
            {
                return false;
            }
        }
    }

    public Node(string path, IEnumerable<string> exclude = null)
    {
        Path = path;
        Name = System.IO.Path.GetFileName(Path);
        Parent = null;

        exclude ??= [];
        exclude = exclude.Select(x => x.ToLowerInvariant());

        Children = GetChildren(exclude);
    }

    private Node(string path, Node parent, IEnumerable<string> exclude = null)
    {
        Path = path;
        Name = System.IO.Path.GetFileName(Path);
        Parent = parent;

        exclude ??= [];
        exclude = exclude.Select(x => x.ToLowerInvariant());

        Children = GetChildren(exclude);
    }

    private Node(string name, Node parent)
    {
        Path = null;
        Name = name;
        Parent = parent;
        Children = [];
    }

    private List<Node> GetChildren(IEnumerable<string> exclude)
    {
        if (IsFile) return [];

        if (exclude.Contains(Name.ToLowerInvariant()))
            return [new("(contents not shown)", this)];

        return Directory.GetDirectories(Path)
            .Concat(Directory.GetFiles(Path)
                .Where(file => !exclude.Contains(System.IO.Path.GetFileName(file).ToLowerInvariant())))
            .Select(p => new Node(p, this, exclude)).ToList();
    }

    public long? GetSize()
    {
        if (IsDirectory) return null;

        try
        {
            return new FileInfo(Path).Resolve().Length;
        }
        catch
        {
            return null;
        }
    }

    public void PrettyPrint(Action<string> printer, string indent = null, bool last = false)
    {
        indent ??= string.Empty;
        string output = indent;
        if (!IsRoot)
        {
            output += last
                ? @"\-- "
                : "|-- ";
        }

        var suffix = string.Join(" ", new[] {
            GetSize() is long size ? $"[{ByteSize.FromBytes(size):0.##}]" : null,
            IsSymbolicLink ? "[symlink]" : null,
        }.Where(s => s is not null).ToArray());

        printer.Invoke($"{output}{(IsRoot ? Path : Name)}{(suffix.IsNullOrWhiteSpace() ? string.Empty : $" {suffix}")}");

        if (!IsRoot)
        {
            indent += last
                ? "    "
                : "|   ";
        }

        for (int i = 0; i < Children.Count; i++)
        {
            if (i == 0 || Children[i].IsDirectory || i == Children.IndexOf(Children.FirstOrDefault(node => node.IsFile) ?? this))
            {
                printer.Invoke($"{indent}|");
            }

            Children[i].PrettyPrint(printer, indent, i == Children.Count - 1);
        }
    }
}
