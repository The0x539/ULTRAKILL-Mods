using System;
using System.Collections.Generic;

using BepInEx;

using HarmonyLib;

using UnityEngine;

namespace RcHud;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin {
    public void Awake() {
        RcHud.Config.Init(this.Config);
        Harmony.CreateAndPatchAll(this.GetType());
    }

    public void Start() {
        RacecarHud.SetLogger(this.Logger);
        GunWheelOverlay.SetLogger(this.Logger);
    }

    public void Update() {
        _ = RacecarHud.Instance;
        _ = GunWheelOverlay.Instance;

        if (RcHud.Config.RefreshIconsOnBattleMusic) {
            var musicManager = MusicManager.Instance;
            if (musicManager != null) {
                if (musicManager.battleTheme.volume > 0 || musicManager.bossTheme.volume > 0) {
                    RefreshBothIcons();
                }
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FistControl), "UpdateFistIcon")]
    static void RefreshFistIconOnSwitch() {
        if (RcHud.Config.RefreshFistOnSwitch) {
            RacecarHud.Instance.RefreshFist();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FistControl), "UpdateFistIcon")]
    [HarmonyPatch(typeof(Punch), "PunchStart")]
    static void RefreshFistIconOnPunch() {
        if (RcHud.Config.RefreshFistOnPunch) {
            RacecarHud.Instance.RefreshFist();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GunControl), "SwitchWeapon", new Type[] { typeof(int) })]
    [HarmonyPatch(typeof(GunControl), "SwitchWeapon", new Type[] { typeof(int), typeof(List<GameObject>), typeof(bool), typeof(bool) })]
    static void RefreshGunIcon() {
        if (RcHud.Config.RefreshGunOnSwitch) {
            RacecarHud.Instance.RefreshGun();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BossHealthBar), "Update")]
    static void RefreshBothIcons() {
        if (RcHud.Config.RefreshIconsOnBossHealthBar) {
            RacecarHud.Instance.RefreshFist();
            RacecarHud.Instance.RefreshGun();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HUDOptions), "HudFade")]
    static void SetIconFade(bool stuff) {
        RacecarHud.Instance.fadeIcons = stuff;
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
