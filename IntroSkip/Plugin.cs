using BepInEx;

using HarmonyLib;

namespace IntroSkip;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    public void Awake() {
        Harmony.CreateAndPatchAll(this.GetType());
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(IntroViolenceScreen), "Start")]
    static void TerminateIntro(ref float ___fadeAmount, ref float ___targetAlpha, ref bool ___fade) {
        ___fadeAmount = 0;
        ___targetAlpha = 0;
        ___fade = true;
    }
}
