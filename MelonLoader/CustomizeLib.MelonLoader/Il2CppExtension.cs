using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Injection;
using System.IO;
using Il2CppInterop.Common;
using Il2CppInterop.Runtime.Startup;
using System.Collections;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#pragma warning disable
namespace CustomizeLib.MelonLoader
{
    public static class Il2CppExtension
    {
        public static Il2CppSystem.Collections.Generic.List<T> ToIl2CppList<T>(this List<T> list)
        {
            var result = new Il2CppSystem.Collections.Generic.List<T>();
            foreach (var item in list) result.Add(item);
            return result;
        }

        public static List<T> ToSystemList<T>(this Il2CppSystem.Collections.Generic.List<T> list)
        {
            var result = new List<T>();
            foreach (var item in list) result.Add(item);
            return result;
        }

        public static Il2CppSystem.Type ToIl2CppType(this Type type)
        {
            if (type == null) return null;
            return Il2CppType.From(type);
        }

        public static Il2CppSystem.Array ToArray<T>(this Il2CppReferenceArray<T> array) where T : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase 
            => array.Cast<Il2CppSystem.Array>();
    }
}