using System.Reflection;
using System.Collections;

using BepInEx;

using HarmonyLib;

using UnityEngine;

namespace WrathClimax;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin {
    private static Plugin self = null!;

    public void Awake() {
        self = this;
        Harmony.CreateAndPatchAll(this.GetType());
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectActivator), nameof(ObjectActivator.Activate))]
    private static void OnActivate(ObjectActivator __instance) {
        if (SceneHelper.CurrentScene != "Level 5-4") {
            return;
        }

        if (__instance.name == "LeviathanIntro") {
            self.StartCoroutine(ShowText(false));
        } else if (__instance.name == "TeaseActivator") {
            self.StartCoroutine(ShowText(true));
        }
    }

    private static IEnumerator ShowText(bool isTease) {
        var lnp = LevelNamePopup.Instance;

        if (isTease) {
            yield return new WaitForSeconds(1.5f);
        } else {
            var field = lnp.GetType().GetField("nameString", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(lnp, "");
            yield return new WaitForSeconds(2.25f);
        }

        lnp.NameAppear();
    }
}