using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireOverhaul
{
    internal class CustomSettings : JsonModSettings
    {

      
    }
    static class Settings
    {
        internal static CustomSettings settings;
        internal static void OnLoad()
        {
            settings = new CustomSettings();
            settings.AddToModSettings("Fire Overhaul");
        }
    }

}
