using BepInEx;

using HarmonyLib;

using UnityEngine;

namespace StyleToasts;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin {
    public void Awake() {
        Harmony.CreateAndPatchAll(this.GetType());
    }

    private static Vector3? styleLocation = null;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Coin), "RicoshotPointsCheck")]
    static void RicoshotPointsCheck(Rigidbody ___rb) {
        styleLocation = ___rb.position;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StyleHUD), nameof(StyleHUD.AddPoints))]
    static void Foo(string pointID, EnemyIdentifier eid) {
        if (eid is null || pointID is not ("ultrakill.fireworks" or "ultrakill.instakill")) {
            return;
        }

        var rb = eid.GetComponent<Rigidbody>();
        styleLocation ??= rb.worldCenterOfMass;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StyleHUD), nameof(StyleHUD.AddPoints))]
    static void Bar(StyleHUD __instance, string pointID, int count, string prefix, string postfix) {
        var pos = styleLocation;
        styleLocation = null;
        if (pos == null) {
            return;
        }

        var text = string.Concat("+ ", prefix, __instance.GetLocalizedName(pointID), postfix);
        if (count >= 0) {
            text += $" x{count}";
        }

        SpawnText(pos.Value, text);
    }

    private static void SpawnText(Vector3 position, string text) {
        var obj = new GameObject();
        obj.AddComponent<ToastText>().Text = text;
        obj.transform.position = position;
    }
}
