using BepInEx;

using HarmonyLib;

using UnityEngine;

namespace StyleToasts;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin {
    public void Awake() {
        Harmony.CreateAndPatchAll(this.GetType());
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Coin), "RicoshotPointsCheck")]
    static void Foo(Rigidbody ___rb) {
        NewMovement.Instance.rb.position = ___rb.position;

        var obj = new GameObject();
        obj.AddComponent<ToastText>();
        //obj.transform.position = ___rb.position;
    }
}
