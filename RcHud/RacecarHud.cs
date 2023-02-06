using BepInEx.Logging;

using UnityEngine;
using UnityEngine.UI;

namespace RcHud;

public sealed class RacecarHud : MonoSingleton<RacecarHud> {
    private static ManualLogSource log = new("");
    internal static void SetLogger(ManualLogSource logger) => log = logger;

    private bool initialized = false;

    private GameObject fist = new();
    private GameObject gun = new();
    private GameObject wheel = new();

    private Crosshair crosshairReference = new();

    private float fistFade = Config.IconFadeTime;
    private float gunFade = Config.IconFadeTime;

    internal bool fadeIcons;
    private bool southpaw;
    private bool newHandedness = true;

    public bool LeftHanded {
        get => this.southpaw;
        set {
            this.southpaw = value;
            this.newHandedness = true;
        }
    }

    public void RefreshFist() => this.fistFade = Config.IconFadeTime;
    public void RefreshGun() => this.gunFade = Config.IconFadeTime;

    public void Update() {
        if (!this.TryInit()) {
            return;
        }

        this.UpdateHandedness();
        this.UpdateFade(ref this.fistFade, this.fist);
        this.UpdateFade(ref this.gunFade, this.gun);

        var gunIcon = this.gun.GetComponent<Image>();
        var gunTransform = this.gun.GetComponent<RectTransform>();
        gunTransform.sizeDelta = GetTexSize(gunIcon) * Config.GunIconScale;

        var weaponWheelVisible = WeaponWheel.Instance.isActiveAndEnabled;
        this.gun.SetActive(!weaponWheelVisible);
        this.fist.SetActive(!weaponWheelVisible);

        var weaponCharges = WeaponCharges.Instance;
        var cbs = ColorBlindSettings.Instance;

        var wheelSlider = this.wheel.GetComponent<Slider>();
        wheelSlider.value = weaponCharges.raicharge;

        var wheelImg = this.wheel.GetComponent<Image>();
        // TODO: flash white?
        wheelImg.color = wheelSlider.normalizedValue < 1 ? cbs.railcannonChargingColor : cbs.railcannonFullColor;
    }

    private void UpdateFade(ref float fade, GameObject icon) {
        fade = this.fadeIcons ? Mathf.MoveTowards(fade, 0, Time.deltaTime) : Config.IconFadeTime;
        icon.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Min(1, fade));
    }

    private void UpdateHandedness() {
        if (!this.newHandedness) {
            return;
        }
        this.newHandedness = false;

        var fistTransform = this.fist.GetComponent<RectTransform>();
        var gunTransform = this.gun.GetComponent<RectTransform>();

        if (this.southpaw) {
            fistTransform.pivot = new(0, 0.5f);
            fistTransform.anchoredPosition = new(Config.FistIconOffset, 0);

            gunTransform.pivot = new(1, 0.5f);
            gunTransform.anchoredPosition = new(-Config.GunIconOffset, 0);
        } else {
            fistTransform.pivot = new(1, 0.5f);
            fistTransform.anchoredPosition = new(-Config.FistIconOffset, 0);

            gunTransform.pivot = new(0, 0.5f);
            gunTransform.anchoredPosition = new(Config.GunIconOffset, 0);
        }
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

        this.InitFist(fistControl);
        this.InitGun(weaponHud);
        this.InitWheel(powerUpMeter);

        log.LogInfo("hud initialized");
        this.initialized = true;
        return true;
    }

    private void InitFist(FistControl fistControl) {
        var fist = new GameObject("Fist");

        fist.AddComponent<Image>();

        var icon = fistControl.fistIcon;

        var copyImg = fist.AddComponent<CopyImage>();
        copyImg.imgToCopy = icon;
        copyImg.copyColor = true;

        var rt = fist.GetComponent<RectTransform>();
        rt.parent = this.crosshairReference.transform;
        rt.anchorMin = rt.anchorMax = new(0.5f, 0.5f);
        rt.pivot = new(1, 0.5f);
        rt.sizeDelta = GetTexSize(icon) * Config.FistIconScale;
        rt.anchoredPosition = new(-Config.FistIconOffset, 0);

        this.fist = fist;
    }

    private void InitGun(WeaponHUD weaponHud) {
        var gun = new GameObject("Gun");

        gun.AddComponent<Image>();

        var copyImg = gun.AddComponent<CopyImage>();
        copyImg.imgToCopy = weaponHud.GetComponentInChildren<Image>();
        copyImg.copyColor = true;

        var rt = gun.GetComponent<RectTransform>();
        rt.parent = this.crosshairReference.transform;
        rt.anchorMin = rt.anchorMax = new(0.5f, 0.5f);
        rt.pivot = new(0, 0.5f);
        rt.anchoredPosition = new(Config.GunIconOffset, 0);

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
        rt.parent = this.crosshairReference.transform;
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
        fob.fadeOutTime = Config.WheelFadeTime;

        this.wheel = wheel;
    }

    private static Vector2 GetTexSize(Image icon) => new(icon.mainTexture.width, icon.mainTexture.height);
}
