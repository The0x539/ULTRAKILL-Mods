using BepInEx;

using HarmonyLib;

using UnityEngine;

namespace BetterWeaponWheel;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin {
    public void Awake() {
        Harmony.CreateAndPatchAll(this.GetType());
    }

    public void Start() {
        GunWheelOverlay.SetLogger(this.Logger);
    }

    public void Update() {
        _ = GunWheelOverlay.Instance;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WeaponWheel), "SetSegmentCount")]
    static void ReinitializeGunWheelWheelIcons(int count) {
        GunWheelOverlay.Instance.InitWheelIcons(count);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WeaponWheel), "Update")]
    static void UpdateTheWheelInfo(ref int ___selectedSegment, ref Vector2 ___direction) {
        GunWheelOverlay.Instance.UpdateWheelInfo(___selectedSegment, ___direction);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WeaponWheel), "OnEnable")]
    [HarmonyPatch(typeof(WeaponWheel), "OnDisable")]
    static void ReinitializeGunWheelIconScale() {
        GunWheelOverlay.Instance.ResetIconScales();
    }
}
