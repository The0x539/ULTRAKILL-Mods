using System.Reflection;
using System.Collections.Generic;

using BepInEx.Logging;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace RcHud;

public sealed class RacecarHud : MonoSingleton<RacecarHud> {
    private static ManualLogSource log = new("");
    internal static void SetLogger(ManualLogSource logger) => log = logger;

    private bool initialized = false;

    private GameObject fist = new();
    private GameObject gun = new();
    private GameObject wheel = new();

    private Crosshair crosshairReference = new();
    private List<GameObject> fistList = new();

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

        var gunc = GunControl.Instance;
        var weaponWheelVisible = WeaponWheel.Instance.isActiveAndEnabled;
        // the information that this HUD shows is never relevant in these secret levels
        var inSecret = SceneManager.GetActiveScene().name is "Level 0-S" or "Level 1-S" or "Level 4-S";

        var showGun = true;
        showGun &= !weaponWheelVisible;
        showGun &= gunc.allWeapons.Count > 1;
        showGun &= !inSecret;

        var showFist = true;
        showFist &= !weaponWheelVisible;
        showFist &= this.fistList.Count > 1;
        showFist &= !inSecret;

        var showWheel = true;
        showWheel &= gunc.slot4.Count > 0;
        showWheel &= !inSecret;

        this.gun.SetActive(showGun);
        this.fist.SetActive(showFist);
        this.wheel.SetActive(showWheel);

        var weaponCharges = WeaponCharges.Instance;
        var cbs = ColorBlindSettings.Instance;

        var wheelSlider = this.wheel.GetComponent<Slider>();
        wheelSlider.value = weaponCharges.raicharge;

        var wheelImg = this.wheel.GetComponent<Image>();
        // TODO: flash white?
        wheelImg.color = wheelSlider.normalizedValue < 1 ? cbs.railcannonChargingColor : cbs.railcannonFullColor;

        this.UpdateStaminaColors();
        this.UpdatePersistentHp();
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

    private void UpdateStaminaColors() {
        var cbs = ColorBlindSettings.Instance;
        var blue = cbs.staminaColor;
        var dark = cbs.staminaChargingColor * 0.6f; // the normal hud uses opacity, which doesn't work here for some reason
        var red = cbs.staminaEmptyColor;

        for (var i = 2; i < 5; i++) {
            var chud = this.crosshairReference.chuds[i];
            var stfa = chud.GetComponent<SliderToFillAmount>();
            stfa.copyColor = false;
            var full = chud.fillAmount == stfa.maxFill;
            var first = i == 2;

            chud.color = Config.StaminaColorMode switch {
                0 => full ? blue : (first ? red : blue),
                1 => blue,
                2 => full ? blue : red,
                3 => full ? blue : dark,
                4 => full ? blue : (first ? red : dark),
                _ => Color.magenta, // invalid
            };
        }
    }

    private void UpdatePersistentHp() {
        if (!Config.PersistentHp) {
            return;
        }

        var chuds = this.crosshairReference.chuds;
        var hp = chuds[1];
        var overheal = chuds[7];
        var stfa = hp.GetComponent<SliderToFillAmount>();
        if (hp.fillAmount < stfa.maxFill || overheal.fillAmount > 0) {
            // this value is shared between both HP sliders
            // it would take much more hackery to only persist overheal (it is possible, though)
            stfa.mama.fadeOutTime = 2f;
        }
    }

    public void ApplyHiVisOverhealSettings() {
        var chuds = this.crosshairReference.chuds;
        var circles = this.crosshairReference.circles;
        var overheal = chuds[7];
        var overhealDmg = chuds[6];

        if (!Config.HiVisOverheal) {
            overheal.color = new Color(0.703f, 1, 0.704f); // not sure where this is originally defined
            // everything else already got updated by Crosshair.CheckCrossHair
            return;
        }

        Sprite circle;
        switch (PrefsManager.Instance.GetInt("crossHairHud")) {
            case 1: circle = circles[2]; break; // thick covering thin
            case 2: circle = circles[3]; break; // extra thicc covering medium
            case 3: circle = circles[0]; break; // thin stripe on thick
            case 4: circle = circles[1]; break; // medium stripe on extra thicc
            default: return; // invalid value, or pref is zero (crosshair HUD disabled)
        }

        overhealDmg.sprite = overheal.sprite = circle;
        overheal.color = Color.green * 0.65f;
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

        var rt = this.InitTransform(fist);
        rt.sizeDelta = GetTexSize(icon) * Config.FistIconScale;

        var fieldInfo = fistControl.GetType().GetField("spawnedArms", BindingFlags.Instance | BindingFlags.NonPublic);
        this.fistList = (List<GameObject>)fieldInfo.GetValue(fistControl);

        this.fist = fist;
    }

    private void InitGun(WeaponHUD weaponHud) {
        var gun = new GameObject("Gun");

        gun.AddComponent<Image>();

        var copyImg = gun.AddComponent<CopyImage>();
        copyImg.imgToCopy = weaponHud.GetComponentInChildren<Image>();
        copyImg.copyColor = true;

        this.InitTransform(gun);

        this.gun = gun;
    }

    private void InitWheel(PowerUpMeter powerUpMeter) {
        var wheel = new GameObject("Wheel");

        var img = wheel.AddComponent<Image>();
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillAmount = 0.5f;
        img.sprite = powerUpMeter.GetComponentInChildren<Image>().sprite;

        var rt = this.InitTransform(wheel);
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

    private RectTransform InitTransform(GameObject obj) {
        var rt = obj.GetComponent<RectTransform>();
        rt.parent = this.crosshairReference.transform;
        rt.localScale = Vector3.one * 2 / 3;
        rt.anchorMin = rt.anchorMax = new(0.5f, 0.5f);
        return rt;
    }

    private static Vector2 GetTexSize(Image icon) => new(icon.mainTexture.width, icon.mainTexture.height);
}
