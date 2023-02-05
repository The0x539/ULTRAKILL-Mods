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
    static void Foo(Rigidbody ___rb) {
        styleLocation = ___rb.position;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StyleHUD), nameof(StyleHUD.AddPoints))]
    static void SpawnText(StyleHUD __instance, int points, string pointID, GameObject sourceWeapon, EnemyIdentifier eid, int count, string prefix, string postfix) {
        var maybePos = styleLocation;
        styleLocation = null;
        if (maybePos == null) {
            return;
        }

        var pos = maybePos.Value;

        var obj = new GameObject();
        var toast = obj.AddComponent<ToastText>();
        obj.transform.position = pos;
        toast.Text = prefix + __instance.GetLocalizedName(pointID) + postfix; ;
    }
}
