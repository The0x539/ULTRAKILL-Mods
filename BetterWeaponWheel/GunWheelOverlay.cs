using System.Linq;
using System.Collections.Generic;

using BepInEx.Logging;

using UnityEngine;
using UnityEngine.UI;

namespace BetterWeaponWheel;

public sealed class GunWheelOverlay : MonoSingleton<GunWheelOverlay> {
    private static ManualLogSource log = new("");
    internal static void SetLogger(ManualLogSource logger) => log = logger;

    private bool initialized = false;
    private int segmentCount = -1;
    private int selectedSegment = -1;
    private float[] iconScales = new float[0];
    private Vector2 dotDirection;

    private GameObject overlay = new();
    private GameObject wheelDot = new();
    private readonly List<GameObject> wheelIcons = new();

    private const float minIconScale = 0.085f;
    private const float maxIconScale = 0.105f;

    public void Update() {
        if (!this.TryInit()) {
            return;
        }

        var active = WeaponWheel.Instance.isActiveAndEnabled;
        this.overlay.SetActive(active);
        this.wheelDot.SetActive(active);
        if (!active) {
            return;
        }

        this.wheelDot.GetComponent<RectTransform>().anchoredPosition = this.dotDirection * 71;

        for (var i = 0; i < this.wheelIcons.Count; i++) {
            var wheelIcon = this.wheelIcons[i];
            if (wheelIcon == null || !wheelIcon.activeSelf) {
                continue;
            }
            this.UpdateSingleIcon(this.wheelIcons[i], i);
        }
    }

    private void UpdateSingleIcon(GameObject wheelIcon, int i) {
        var gunc = GunControl.Instance;
        var cbs = ColorBlindSettings.Instance;

        var isSelected = i == this.selectedSegment;
        var isActive = i == gunc.currentSlot - 1;

        var lerpTarget = isSelected ? maxIconScale : minIconScale;
        ref var scale = ref this.iconScales[i];
        scale = Mathf.MoveTowards(scale, lerpTarget, Time.deltaTime * 2);

        var img = wheelIcon.GetComponent<Image>();

        var slot = gunc.slots[i];
        if (!slot.Any()) {
            img.sprite = null;
            img.color = Color.clear;
            return;
        }

        int vIdx;
        if (isActive) {
            vIdx = (gunc.currentVariation + 1) % slot.Count;
        } else if (gunc.variationMemory) {
            vIdx = PlayerPrefs.GetInt($"Slot{i + 1}Var", 0);
        } else {
            vIdx = 0;
        }

        var targetIcon = slot[vIdx].GetComponent<WeaponIcon>();
        var rt = wheelIcon.GetComponent<RectTransform>();

        img.sprite = isSelected ? targetIcon.glowIcon : targetIcon.weaponIcon;

        img.color = cbs.variationColors[targetIcon.variationColor];

        rt.sizeDelta = GetTexSize(img) * scale;
    }

    private bool TryInit() {
        if (this.initialized) {
            return true;
        }

        this.InitOverlay();
        this.InitWheelDot();
        this.initialized = true;

        log.LogInfo("weapon wheel icon overlay initialized");
        return true;
    }

    private void InitOverlay() {
        var overlay = new GameObject("Overlay");

        var canvas = overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        var scaler = overlay.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0;
        scaler.referenceResolution = new(1920, 1080);

        var group = overlay.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        this.overlay = overlay;
    }

    private void InitWheelDot() {
        this.wheelDot = new GameObject();

        this.wheelDot.SetActive(false);

        var img = this.wheelDot.AddComponent<Image>();
        img.sprite = null;
        img.color = Color.white;

        var rt = this.wheelDot.GetComponent<RectTransform>();
        rt.SetParent(this.overlay.GetComponent<RectTransform>());
        rt.pivot = rt.anchorMin = rt.anchorMax = new(0.5f, 0.5f);
        rt.sizeDelta = new(3, 3);
        rt.Rotate(0, 0, 45);
    }

    public void InitWheelIcons(int count) {
        if (!this.initialized || count == this.segmentCount) {
            return;
        }
        this.segmentCount = count;
        this.iconScales = new float[count];
        this.ResetIconScales();

        while (count > this.wheelIcons.Count) {
            var icon = new GameObject();
            icon.AddComponent<Image>();

            var rt = icon.GetComponent<RectTransform>();
            rt.SetParent(this.overlay.GetComponent<RectTransform>());
            rt.anchorMin = rt.anchorMax = new(0.5f, 0.5f);

            this.wheelIcons.Add(icon);
        }

        var r = 85;

        for (var i = 0; i < this.wheelIcons.Count; i++) {
            var icon = this.wheelIcons[i];

            if (i >= count) {
                icon.SetActive(false);
                continue;
            }

            icon.SetActive(true);
            var theta = Mathf.PI * 2 * (i + 0.5f) / count;
            var rt = icon.GetComponent<RectTransform>();

            var v = new Vector2(-Mathf.Cos(theta), Mathf.Sin(theta));

            var taxicabMagnitude = Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y));
            var projectedTo1 = v / taxicabMagnitude;
            rt.pivot = new Vector2(0.5f, 0.5f) - projectedTo1 / 2;
            rt.anchoredPosition = r * v;
        }
    }

    public void UpdateWheelInfo(int selectedSegment, Vector2 direction) {
        this.selectedSegment = selectedSegment;
        this.dotDirection = direction;
    }

    public void ResetIconScales() {
        for (var i = 0; i < this.iconScales.Length; i++) {
            this.iconScales[i] = minIconScale;
        }
    }

    private static Vector2 GetTexSize(Image icon) => new(icon.mainTexture.width, icon.mainTexture.height);
}
