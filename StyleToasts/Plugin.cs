﻿using System.Collections.Generic;

using BepInEx;

using HarmonyLib;

using UnityEngine;

namespace StyleToasts;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin {
    public void Awake() {
        Harmony.CreateAndPatchAll(this.GetType());
    }

    private static readonly HashSet<string> ignoredBonuses = [
        "ultrakill.projectileboost",
        "ultrakill.disrespect",
        "ultrakill.quickdraw",
        "ultrakill.parry",
        "ultrakill.kill",
    ];

    private static Vector3? styleLocation = null;

    private static Vector3? TakeStyleLocation() {
        var pos = styleLocation;
        styleLocation = null;
        return pos;
    }

    private static float scatterAmount = 0;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Coin), "RicoshotPointsCheck")]
    static void OnRicoshot(Rigidbody ___rb) {
        styleLocation = ___rb.position;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Gutterman), nameof(Gutterman.ShieldBreak))]
    static void OnShieldBreak(bool player, Rigidbody ___rb) {
        if (player) {
            styleLocation = ___rb.position;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StyleHUD), nameof(StyleHUD.AddPoints))]
    static void OnAddPoints(string pointID, EnemyIdentifier eid) {
        if (eid is null || ignoredBonuses.Contains(pointID)) {
            return;
        }

        if (eid.TryGetComponent<Rigidbody>(out var rb)) {
            styleLocation ??= rb.worldCenterOfMass;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StyleHUD), nameof(StyleHUD.AddPoints))]
    static void AfterAddPoints(StyleHUD __instance, string pointID, int count, string prefix, string postfix) {
        if (TakeStyleLocation() is not Vector3 pos) return;

        var name = __instance.GetLocalizedName(pointID);
        if (name == "") return;

        var offset = Random.insideUnitSphere * scatterAmount;
        offset.x = 0;
        pos += offset;

        var text = string.Concat("+ ", prefix, name, postfix);

        var scale = pointID switch {
            "ultrakill.bigkill" => 0.04f,
            _ => 0.02f,
        };

        if (count >= 0) {
            text += $" x{count}";
            scale *= count;
        }

        SpawnText(pos, scale, text);

        // scatter amount increases with every spawn, up to a limit of 10 units...
        scatterAmount = Mathf.MoveTowards(scatterAmount, 10, 3);
    }

    public void Update() {
        // ...and decays back to zero over a couple seconds
        scatterAmount = Mathf.MoveTowards(scatterAmount, 0, 3 * Time.deltaTime);
    }

    private static void SpawnText(Vector3 position, float scale, string text) {
        var obj = new GameObject();
        var toast = obj.AddComponent<ToastText>();
        toast.Text = text;
        toast.Size = Vector3.one * scale;
        toast.transform.position = position;
    }
}