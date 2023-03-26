using UnityEngine;
using UnityEngine.SceneManagement;

using BepInEx;
using HarmonyLib;

namespace IntroSkip;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin {
    public Plugin() {
        if (SceneManager.GetActiveScene().name != "Bootstrap") {
            return;
        }

        var targetScene = this.Config.Bind("IntroSkip", "TargetScene", "Main Menu");
        SceneHelper.LoadScene(targetScene.Value);
        Harmony.CreateAndPatchAll(this.GetType());
    }

    [HarmonyPatch(typeof(IntroViolenceScreen), "Start")]
    [HarmonyPrefix]
    public static bool KillIntro(IntroViolenceScreen __instance) {
        __instance.gameObject.SetActive(false);
        Cursor.visible = false;
        return false;
    }
}
