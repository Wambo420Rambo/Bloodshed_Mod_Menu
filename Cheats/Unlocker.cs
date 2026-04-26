using com8com1.SCFPS;
using UnityEngine;

namespace mod_menu
{
    public partial class ModMenuBehaviour
    {
        private void SetAllAchievement()
        {
            var all = Resources.FindObjectsOfTypeAll<AchievementManager>();
            if (all == null || all.Length == 0)
            {
                Plugin.Log.LogWarning("achievementManager not found anywhere in scene.");
                return;
            }

            var devOptions = all[0];
            var go = devOptions.gameObject;
            bool wasActive = go.activeSelf;

            foreach (var achievement in all[0].achievements)
            {
                Plugin.Log.LogInfo($"Processing achievement: {achievement.strTitle}");
                if (achievement == null) continue;

                all[0].UnlockAchievement(achievement);
                Plugin.Log.LogInfo($"Achievement unlocked: {achievement.strTitle}");
            }

            ForceSave();
        }

        private void UnlockAll()
        {
            // FindObjectsOfTypeAll finds inactive/disabled components too
            var all = Resources.FindObjectsOfTypeAll<DevOptionsManager>();
            if (all == null || all.Length == 0)
            {
                Plugin.Log.LogWarning("DevOptionsManager not found anywhere in scene.");
                return;
            }

            var devOptions = all[0];
            var go = devOptions.gameObject;
            bool wasActive = go.activeSelf;

            // Temporarily enable so internal Unity references resolve
            go.SetActive(true);

            devOptions.UnlockActives();
            devOptions.UnlockFeatures();
            devOptions.UnlockGlobalUpgrades();
            devOptions.UnlockPassives();
            devOptions.UnlockPlayers();
            devOptions.UnlockWeapons();

            // Restore original state
            go.SetActive(wasActive);

            ForceSave();

            Plugin.Log.LogInfo("UnlockAll complete.");

        }
    }
}
