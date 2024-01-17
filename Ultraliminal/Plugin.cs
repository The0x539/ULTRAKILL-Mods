using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Ultraliminal;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin {
    public void Awake() {
        Harmony.CreateAndPatchAll(this.GetType());
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EnemyIdentifier), "Awake")]
    static void AddForcedPerspective(EnemyIdentifier __instance) {
        __instance.gameObject.AddComponent<ForcedPerspective>();
    }
}

sealed class ForcedPerspective : MonoBehaviour {
    private Vector3 baseScale;
    private float baseDistance;

    public void Awake() {
        this.baseScale = this.gameObject.transform.localScale;
        this.baseDistance = this.ComputeDistance();
    }

    public void Update() {
        var scaleFactor = this.ComputeDistance() / this.baseDistance;
        scaleFactor = Mathf.Clamp(scaleFactor, 0.5f, 100f);
        this.transform.localScale = scaleFactor * this.baseScale;
    }

    private float ComputeDistance() {
        var camPos = Camera.main.transform.position;
        var ownPos = this.transform.position;
        return (ownPos - camPos).magnitude;
    }
}