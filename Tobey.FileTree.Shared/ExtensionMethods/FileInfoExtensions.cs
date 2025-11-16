#if !NET6_0_OR_GREATER
using MonoMod.Utils;
using SymbolicLinkSupport;
#endif
using System.IO;

namespace Tobey.FileTree.ExtensionMethods;
internal static class FileInfoExtensions
{
    public static FileInfo Resolve(this FileInfo info)
    {
#if NET6_0_OR_GREATER
        return new(info.ResolveLinkTarget(true).FullName);
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

#if NET6_0_OR_GREATER
    public static bool IsSymbolicLink(this FileInfo info) => info.LinkTarget is not null;
#endif
}
