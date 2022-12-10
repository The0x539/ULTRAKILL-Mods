using System;
using System.Collections.Generic;

using BepInEx;

using HarmonyLib;

using UnityEngine;

namespace LeatHud;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin {
    public void Awake() {
        Harmony.CreateAndPatchAll(typeof(Plugin));
    }

    public void Start() {
        RacecarHud.SetLogger(this.Logger);
    }

    public void Update() {
        _ = RacecarHud.Instance;

        var musicManager = MusicManager.Instance;
        if (musicManager != null) {
            if (musicManager.battleTheme.volume > 0 || musicManager.bossTheme.volume > 0) {
                RefreshBothIcons();
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FistControl), "UpdateFistIcon")]
    [HarmonyPatch(typeof(Punch), "PunchStart")]
    static void RefreshFistIcon() {
        RacecarHud.Instance.RefreshFist();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GunControl), "SwitchWeapon", new Type[] { typeof(int) })]
    [HarmonyPatch(typeof(GunControl), "SwitchWeapon", new Type[] { typeof(int), typeof(List<GameObject>), typeof(bool), typeof(bool) })]
    static void RefreshGunIcon() {
        RacecarHud.Instance.RefreshGun();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BossHealthBar), "Update")]
    static void RefreshBothIcons() {
        RacecarHud.Instance.RefreshFist();
        RacecarHud.Instance.RefreshGun();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HUDOptions), "HudFade")]
    static void SetIconFade(bool stuff) {
        RacecarHud.Instance.fadeIcons = stuff;
    }
}
