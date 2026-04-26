using com8com1.SCFPS;
using UnityEngine;

namespace mod_menu
{
    public partial class ModMenuBehaviour
    {
        private void ForceSave()
        {
            var save = Resources.FindObjectsOfTypeAll<SaveDataManager>();
            if (save == null || save.Length == 0)
            {
                Plugin.Log.LogWarning("ForceSave: SaveDataManager not found.");
                return;
            }

            var sdm = save[0];
            Plugin.Log.LogInfo($"ForceSave: Using slot {sdm.activeSaveSlot}");

            sdm.SavePersistentData(true);
            sdm.SaveProgressComplete();
            sdm.SaveWorldDataComplete();
            sdm.SaveLevelDataComplete();
            sdm.SaveAchievementData();

            Plugin.Log.LogInfo("ForceSave: Done.");
        }
    }
}