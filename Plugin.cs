using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;


namespace mod_menu
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;

            // Register your MonoBehaviours with the IL2CPP domain BEFORE
            // attaching them to any GameObject — this is the critical step
            ClassInjector.RegisterTypeInIl2Cpp<ModMenuBehaviour>();

            // Add a persistent runner to the scene
            AddComponent<ModMenuBehaviour>();

            Log.LogInfo("Plugin loaded!");
        }
    }
}