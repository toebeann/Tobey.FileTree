using HarmonyLib;
using System;

namespace Tobey.FileTree.ExtensionMethods;

internal static class EnumExtensions
{
    extension<T>(T @enum) where T : unmanaged, Enum
    {
        public bool HasFlag(T flag)
        {
            var hasFlagInternal = Traverse.Create(@enum).OptionalMethod("HasFlag", flag);
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
}
