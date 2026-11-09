using ImprovedFires.Component;

namespace ImprovedFires.Utilities
{
    
    public static class SaveSystem
    {
        
        public static Dictionary<string, string> m_SavedFires = new Dictionary<string, string>();

        public static Il2CppSystem.Collections.Generic.Dictionary<string, string> SaveFires()
        {
            
            Il2CppSystem.Collections.Generic.Dictionary<string, string> list = new Il2CppSystem.Collections.Generic.Dictionary<string, string>();

            foreach (var item in m_SavedFires)
            {
                list.Add(item.Key, item.Value);
            }
            
            return list;
        }

        public static void LoadFires(Il2CppSystem.Collections.Generic.Dictionary<string, string> Dict)
        {
            m_SavedFires.Clear();
            if (Dict != null)
            {
                foreach (var item in Dict)
                {
                    m_SavedFires.Add(item.Key, item.Value);
                }
            }
        }

        public static bool IsFireAlreadyLoaded(string GUID)
        {
            return m_SavedFires.ContainsKey(GUID);
        }

        public static string LoadFire(string GUID)
        {
            
            string iFire;

            if(m_SavedFires.TryGetValue(GUID, out iFire)) return iFire;

            return iFire;
        }

        public static void AddFire(string GUID, string iFire)
        {

            if (m_SavedFires.ContainsKey(GUID))
            {
                m_SavedFires.Remove(GUID);
            }
            m_SavedFires.Add(GUID, iFire);

        }

        [HarmonyLib.HarmonyPatch(typeof(SaveGameSystem), "SaveSceneData")]
        private static class SaveGameSystem_SaveSceneData
        {
            private static void Postfix(SlotData slot)
            {
                slot.TrySetData("improved_fires", SaveFires());
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(SaveGameSystem), "RestoreGlobalData")]
        private static class SaveGameSystem_RestoreGlobalData
        {
            private static void Postfix(string name)
            {
                
                SlotData slot = SaveGameSlots.GetSaveSlotFromName(name);

                if (slot != null)
                {
                   Main.Logger.Log("[CustomSaves] Keys save have:", ComplexLogger.FlaggedLoggingLevel.Debug);
                    Il2CppSystem.Collections.Generic.List<string> Keys = slot.GetAllDataKeys();
                    foreach (string key in Keys)
                    {
                        Main.Logger.Log(key, ComplexLogger.FlaggedLoggingLevel.Debug);
                    }

                    m_SavedFires.Clear();
                    Il2CppSystem.Collections.Generic.Dictionary<string, string> FiresData = null;
                    if (slot.TryGetData("improved_fires", out FiresData))
                    {
                        LoadFires(FiresData);
                    }
                }
            }
        }




















    }


}