using UnityEngine;
using UnityEngine.SceneManagement;

using BepInEx;
using HarmonyLib;

namespace IntroSkip;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin {
    private static string targetScene = "Main Menu";

    public Plugin() {
        if (SceneManager.GetActiveScene().name != "Bootstrap") {
            return;
        }

        var configEntry = this.Config.Bind("IntroSkip", "TargetScene", "Main Menu");
        targetScene = configEntry.Value;
        Harmony.CreateAndPatchAll(this.GetType());
    }

    [HarmonyPatch(typeof(IntroViolenceScreen), "Start")]
    [HarmonyPrefix]
    public static bool KillIntro(IntroViolenceScreen __instance, ref GameObject ___loadingScreen) {
        SceneHelper.LoadScene(targetScene);
        __instance.gameObject.SetActive(false);
        ___loadingScreen.SetActive(true);
        Cursor.visible = false;
        return false;
    }
}
