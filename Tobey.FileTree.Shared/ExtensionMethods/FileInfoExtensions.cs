#if !IL2CPP
using MonoMod.Utils;
using SymbolicLinkSupport;
#endif
using System.IO;

namespace Tobey.FileTree.ExtensionMethods;

internal static class FileInfoExtensions
{
    extension(FileInfo info)
    {
        public FileSystemInfo Resolve()
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
    }

#if IL2CPP
    extension(FileSystemInfo info)
    {
        public bool IsSymbolicLink() => info.LinkTarget is not null;
    }
#endif
}
