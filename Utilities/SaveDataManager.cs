using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireOverhaul.Utilities
{
    internal class SaveDataManager
    {

        ModDataManager dm = new ModDataManager("Fire Overhaul", false);

        public void Save(string data, string suffix)
        {
            dm.Save(data, suffix);
        }

        public string LoadSceneData(string suffix)
        {
            string? sceneName = dm.Load(suffix);

            return sceneName;
        }

    }
}
