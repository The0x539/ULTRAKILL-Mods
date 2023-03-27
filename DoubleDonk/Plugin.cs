using System;
using System.Collections.Generic;

using BepInEx;

using HarmonyLib;

namespace DoubleDonk;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    public void Awake() {
        Harmony.CreateAndPatchAll(this.GetType());
    }

    [HarmonyPatch(typeof(GunSetter), nameof(GunSetter.ResetWeapons))]
    [HarmonyPostfix]
    private static void AddLooseCannon() {
        var gunSetter = GunSetter.Instance;
        var shotgunPrefab = gunSetter.shotgunGrenade[0];

        var looseCannon = Instantiate(shotgunPrefab, gunSetter.transform);
        looseCannon.AddComponent<LooseCannon>();

        var gunControl = GunControl.Instance;
        gunControl.slot6.Add(looseCannon);
        gunControl.UpdateWeaponList();
    }
}
