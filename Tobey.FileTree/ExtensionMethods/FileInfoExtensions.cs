using SymbolicLinkSupport;
using System.IO;
using UnityEngine;

namespace Tobey.FileTree.ExtensionMethods;
internal static class FileInfoExtensions
{
    public static FileInfo Resolve(this FileInfo info)
    {
        if (Application.platform != RuntimePlatform.WindowsPlayer)
        {
            return info;
        }

        try
        {
            while (info.IsSymbolicLink())
            {
                info = new FileInfo(info.GetSymbolicLinkTarget());
            }
            return info;
        }
        catch
        {
            return info;
        }
    }
}
