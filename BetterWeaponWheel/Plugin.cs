using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WeaponWheel), "SetSegmentCount")]
    static void OverrideWheelSegmentCount(ref int count) {
        count = GunControl.Instance.slots.Count(slot => slot.Any());
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WeaponWheel), "SetSegmentCount")]
    static void ReinitializeGunWheelWheelIcons(int count) {
        GunWheelOverlay.Instance.InitWheelIcons(count);
    }

    private static bool weaponWheelIsUpdating = false;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WeaponWheel), "Update")]
    static void WeaponWheelUpdatePrefix() {
        weaponWheelIsUpdating = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WeaponWheel), "Update")]
    static void WeaponWheelUpdatePostfix(ref int ___selectedSegment, ref Vector2 ___direction) {
        GunWheelOverlay.Instance.UpdateWheelInfo(___selectedSegment, ___direction);
        weaponWheelIsUpdating = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GunControl), "SwitchWeapon", typeof(int))]
    static void AdjustSelectedWeapon(GunControl __instance, ref int target) {
        if (!weaponWheelIsUpdating) {
            return;
        }

        var gunc = __instance;

        var nonEmptySlotIndices = new List<int>(8);
        for (var i = 0; i < gunc.slots.Count; i++) {
            if (gunc.slots[i].Any()) {
                nonEmptySlotIndices.Add(i);
            }
        }

        target = nonEmptySlotIndices[target - 1] + 1;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WeaponWheel), "OnEnable")]
    [HarmonyPatch(typeof(WeaponWheel), "OnDisable")]
    static void ReinitializeGunWheelIconScale() {
        GunWheelOverlay.Instance.ResetIconScales();
    }
}
