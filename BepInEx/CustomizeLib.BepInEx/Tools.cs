using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#pragma warning disable
namespace CustomizeLib.BepInEx
{
    public class Tools
    {
        public static Assembly GetAssembly() => Assembly.GetCallingAssembly();
        public static Assembly Assembly {
            get {
                return Assembly.GetCallingAssembly();
            }
        }
    }
}
