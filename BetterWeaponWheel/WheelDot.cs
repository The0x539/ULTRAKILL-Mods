using UnityEngine;
using UnityEngine.UI;

namespace BetterWeaponWheel;

public sealed class WheelDot : MonoSingleton<WheelDot> {
    private bool initialized = false;
    private Vector2 position;

    private GameObject overlay = new();
    private GameObject wheelDot = new();

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

        this.wheelDot.GetComponent<RectTransform>().anchoredPosition = this.position * 71;
    }

    private bool TryInit() {
        if (this.initialized) {
            return true;
        }

        this.InitOverlay();
        this.InitWheelDot();
        this.initialized = true;
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

        var crosshairColorPref = PrefsManager.Instance.GetInt("crossHairColor");

        img.color = crosshairColorPref switch {
            0 or 1 => Color.white,
            2 => Color.gray,
            3 => Color.black,
            4 => Color.red,
            5 => Color.green,
            6 => Color.blue,
            7 => Color.cyan,
            8 => Color.yellow,
            9 => Color.magenta,
            _ => Color.white,
        };

        if (crosshairColorPref == 0) {
            var crosshair = StatsManager.Instance.crosshair.GetComponent<Crosshair>();
            img.material = crosshair.invertMaterial;
        }

        var rt = this.wheelDot.GetComponent<RectTransform>();
        rt.SetParent(this.overlay.GetComponent<RectTransform>());
        rt.pivot = rt.anchorMin = rt.anchorMax = new(0.5f, 0.5f);
        rt.sizeDelta = new(3, 3);
        rt.Rotate(0, 0, 45);
    }

    public void UpdateCursor(Vector2 direction) {
        this.position = direction;
    }
}
