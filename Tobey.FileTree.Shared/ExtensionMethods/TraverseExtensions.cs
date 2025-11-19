using HarmonyLib;
using System;
using Tobey.FileTree.Utilties;

namespace Tobey.FileTree.ExtensionMethods;

internal static class TraverseExtensions
{
    extension(Traverse traverse)
    {
        public Traverse OptionalMethod(string name, params object[] arguments) =>
            TraverseHelper.SuppressHarmonyWarnings(() => traverse.Method(name, arguments));

        public Traverse OptionalMethod(string name, Type[] paramTypes, params object[] arguments) =>
            TraverseHelper.SuppressHarmonyWarnings(() => traverse.Method(name, paramTypes, arguments));
    }
}
