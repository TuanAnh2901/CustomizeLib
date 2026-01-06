using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.MelonLoader
{
    public class Tools
    {
        public static Assembly GetAssembly() => Assembly.GetCallingAssembly();
    }
}
