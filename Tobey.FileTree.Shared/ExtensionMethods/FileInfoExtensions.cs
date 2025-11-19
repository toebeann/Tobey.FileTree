#if !IL2CPP
using MonoMod.Utils;
using SymbolicLinkSupport;
#endif
using System.IO;

namespace Tobey.FileTree.ExtensionMethods;
internal static class FileInfoExtensions
{
    public static FileSystemInfo Resolve(this FileInfo info)
    {
#if IL2CPP
        return info.ResolveLinkTarget(true);
#else
        if (!PlatformHelper.Is(Platform.Windows))
        {
            return info;
        }

        try
        {
            while (info.IsSymbolicLink())
            {
                info = new(info.GetSymbolicLinkTarget());
            }
            return info;
        }
        catch
        {
            return info;
        }
#endif
    }

#if IL2CPP
    public static bool IsSymbolicLink(this FileSystemInfo info) => info.LinkTarget is not null;
#endif
}
