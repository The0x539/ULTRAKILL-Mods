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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WeaponWheel), "Update")]
    static void UpdateWheelCursor(ref Vector2 ___direction) {
        WheelDot.Instance.UpdateCursor(___direction);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WheelSegment), nameof(WheelSegment.SetActive))]
    static void OverrideSegmentColors(WheelSegment __instance, ref bool active) {
        if (!active) {
            return;
        }

        var variantIdx = (int)__instance.descriptor.variationColor;
        var color = ColorBlindSettings.Instance.variationColors[variantIdx];
        __instance.icon.color = color;
        __instance.iconGlow.color = color;
    }

    private static readonly FieldInfo segmentsField = typeof(WeaponWheel).GetField("segments", BindingFlags.Instance | BindingFlags.NonPublic);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GunControl), nameof(GunControl.SwitchWeapon), typeof(int), typeof(List<GameObject>), typeof(bool), typeof(bool), typeof(bool))]
    [HarmonyPatch(typeof(WeaponWheel), nameof(WeaponWheel.SetSegments))]
    private static void OverrideWheelIcons() {
        var gunc = GunControl.Instance;
        var segments = (List<WheelSegment>)segmentsField.GetValue(WeaponWheel.Instance);

        var segmentIdx = 0;
        for (var slotIdx = 0; slotIdx < gunc.slots.Count; slotIdx++) {
            var slot = gunc.slots[slotIdx];
            if (!slot.Any()) {
                continue;
            }

            int variantIdx;
            if (slotIdx + 1 == gunc.currentSlot) {
                variantIdx = (gunc.currentVariation + 1) % slot.Count;
            } else if (gunc.variationMemory) {
                variantIdx = PlayerPrefs.GetInt($"Slot{slotIdx + 1}Var", 0);
            } else {
                variantIdx = 0;
            }

            var segment = segments[segmentIdx++];
            var descriptor = slot[variantIdx].GetComponent<WeaponIcon>().weaponDescriptor;
            if (segment.descriptor == descriptor) {
                // this segment is already up to date
                continue;
            }

            segment.descriptor = descriptor;
            segment.icon.sprite = descriptor.icon;
            segment.iconGlow.sprite = descriptor.glowIcon;
        }
    }

    // TODO: allow switching other slots' weapons while the wheel is up?
}
