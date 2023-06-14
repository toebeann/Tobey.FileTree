using HarmonyLib;
using System;

namespace Tobey.FileTree.ExtensionMethods;
internal static class EnumExtensions
{
    public static bool HasFlag<T>(this T @enum, T flag) where T : unmanaged, Enum
    {
        var hasFlagInternal = Traverse.Create(@enum).Method("HasFlag", flag);
        if (hasFlagInternal.MethodExists())
        {   // use internal .NET HasFlag method if it exists
            return hasFlagInternal.GetValue<bool>();
        }

        if (@enum.GetType() != flag.GetType())
        {
            throw new ArgumentException($"Enum type mismatch: Type of {nameof(flag)} must match type of {nameof(@enum)}.");
        }

        unsafe
        {
            return sizeof(T) switch
            {
                1 => (*(byte*)&@enum & *(byte*)&flag) > 0,
                2 => (*(ushort*)&@enum & *(ushort*)&flag) > 0,
                4 => (*(uint*)&@enum & *(uint*)&flag) > 0,
                8 => (*(ulong*)&@enum & *(ulong*)&flag) > 0,
                _ => throw new Exception("Size does not match a known Enum backing type.")
            };
        }
    }
}
