using BepInEx.Logging;

using UnityEngine;
using UnityEngine.UI;

namespace LeatHud;

public sealed class RacecarHud : MonoSingleton<RacecarHud> {
    private static ManualLogSource log = new("");
    internal static void SetLogger(ManualLogSource logger) => log = logger;

    // configuration
    private const float gunScale = 0.1f;
    private const float fistScale = 1.25f;
    private const float iconFade = 4;
    private const float wheelFade = 5;

    private bool initialized = false;

    private GameObject overlay = new();
    private GameObject fist = new();
    private GameObject gun = new();
    private GameObject wheel = new();

    private Crosshair crosshairReference = new();

    private float fistFade = iconFade;
    private float gunFade = iconFade;

    internal bool fadeIcons;

    public void RefreshFist() => this.fistFade = iconFade;
    public void RefreshGun() => this.gunFade = iconFade;

    public void Update() {
        if (!this.TryInit()) {
            return;
        }

        this.overlay.SetActive(this.crosshairReference.isActiveAndEnabled);

        this.UpdateFade(ref this.fistFade, this.fist);
        this.UpdateFade(ref this.gunFade, this.gun);

        var gunIcon = this.gun.GetComponent<Image>();
        var gunTransform = this.gun.GetComponent<RectTransform>();
        gunTransform.sizeDelta = new Vector2(gunIcon.mainTexture.width, gunIcon.mainTexture.height) * gunScale;

        var weaponCharges = WeaponCharges.Instance;
        var cbs = ColorBlindSettings.Instance;

        var wheelSlider = this.wheel.GetComponent<Slider>();
        wheelSlider.value = weaponCharges.raicharge;

        var wheelImg = this.wheel.GetComponent<Image>();
        // TODO: flash white?
        wheelImg.color = wheelSlider.normalizedValue < 1 ? cbs.railcannonChargingColor : cbs.railcannonFullColor;
    }

    private void UpdateFade(ref float fade, GameObject icon) {
        fade = this.fadeIcons ? Mathf.MoveTowards(fade, 0, Time.deltaTime) : iconFade;
        icon.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Min(1, fade));
    }

    private bool TryInit() {
        if (this.initialized) {
            return true;
        }

        var fistControl = FistControl.Instance;
        var weaponHud = WeaponHUD.Instance;
        var powerUpMeter = PowerUpMeter.Instance;
        var hudOptions = HUDOptions.Instance;

        if (fistControl == null || weaponHud == null || powerUpMeter == null || hudOptions == null) {
            return false;
        }

        this.crosshairReference = hudOptions.GetComponentInChildren<Crosshair>();
        this.fadeIcons = PrefsManager.Instance.GetBool("crossHairHudFade");

        this.InitOverlay();
        this.InitFist(fistControl);
        this.InitGun(weaponHud);
        this.InitWheel(powerUpMeter);

        log.LogInfo("hud initialized");
        this.initialized = true;
        return true;
    }

    private void InitOverlay() {
        var overlay = new GameObject("Overlay");

        var canvas = overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = overlay.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0;
        scaler.referenceResolution = new(1920, 1080);

        var group = overlay.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        this.overlay = overlay;
    }

    private void InitFist(FistControl fistControl) {
        var fist = new GameObject("Fist");

        fist.AddComponent<Image>();

        var icon = fistControl.fistIcon;

        var copyImg = fist.AddComponent<CopyImage>();
        copyImg.imgToCopy = icon;
        copyImg.copyColor = true;

        var rt = fist.GetComponent<RectTransform>();
        rt.SetParent(this.overlay.GetComponent<RectTransform>());
        rt.anchorMin = rt.anchorMax = new(0.5f, 0.5f);
        rt.pivot = new(1, 0.5f);
        rt.sizeDelta = new Vector2(icon.mainTexture.width, icon.mainTexture.height) * fistScale;
        rt.anchoredPosition = new(-50, 0);

        this.fist = fist;
    }

    private void InitGun(WeaponHUD weaponHud) {
        var gun = new GameObject("Gun");

        gun.AddComponent<Image>();

        var copyImg = gun.AddComponent<CopyImage>();
        copyImg.imgToCopy = weaponHud.GetComponentInChildren<Image>();
        copyImg.copyColor = true;

        var rt = gun.GetComponent<RectTransform>();
        rt.SetParent(this.overlay.GetComponent<RectTransform>());
        rt.anchorMin = rt.anchorMax = new(0.5f, 0.5f);
        rt.pivot = new(0, 0.5f);
        rt.anchoredPosition = new(45, 0);

        this.gun = gun;
    }

    private void InitWheel(PowerUpMeter powerUpMeter) {
        var wheel = new GameObject("Wheel");

        var img = wheel.AddComponent<Image>();
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillAmount = 0.5f;
        img.sprite = powerUpMeter.GetComponentInChildren<Image>().sprite;

        var rt = wheel.GetComponent<RectTransform>();
        rt.SetParent(this.overlay.GetComponent<RectTransform>());
        rt.anchorMin = rt.anchorMax = new(0.5f, 0.5f);
        rt.pivot = new(0.5f, 0.5f);
        rt.sizeDelta = new(64, 64);
        rt.anchoredPosition = new(0, 0);
        rt.Rotate(0, 0, 180);

        // move the powerup meter so the two rings aren't fighting for space
        powerUpMeter.GetComponent<RectTransform>().sizeDelta = new(69, 69);

        var slider = wheel.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 4;

        var stfa = wheel.AddComponent<SliderToFillAmount>();
        stfa.targetSlider = slider;
        stfa.maxFill = 1;

        var fob = wheel.AddComponent<FadeOutBars>();
        fob.fadeOutTime = wheelFade;

        this.wheel = wheel;
    }
}
