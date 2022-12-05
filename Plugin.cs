using System;

using BepInEx;

using UKUIHelper;

using UnityEngine;
using UnityEngine.UI;

namespace LeatHud;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("zed.uk.uihelper")]
public sealed class Plugin : BaseUnityPlugin {
    private GameObject? wheel;
    private GameObject? gun;
    private GameObject? fist;

    private readonly float gunScale = 0.1f;
    private readonly float fistScale = 1.25f;

    private float iconTimer = 0f;

    public void Awake() {
        this.Logger.LogInfo("Plugin loaded!");
    }

    private static GameObject MakeWheel(Crosshair crosshair) {
        var wheel = UIHelper.CreateImage();

        wheel.AddComponent<Slider>();
        var slider = wheel.GetComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 4;

        var canvas = UIHelper.CreateOverlay(persistent: true);
        canvas.GetComponent<Canvas>().sortingOrder = 10;

        var rt = wheel.GetComponent<RectTransform>();
        rt.SetParent(canvas.GetComponent<RectTransform>());
        rt.SetAnchor(AnchorPresets.MiddleCenter);
        rt.SetPivot(PivotPresets.MiddleCenter);
        rt.Rotate(0, 0, 180);

        var pt = MonoSingleton<PowerUpMeter>.Instance.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(64, 64);
        pt.sizeDelta = new Vector2(69, 69);

        wheel.AddComponent<CanvasGroup>();
        wheel.GetComponent<CanvasGroup>().blocksRaycasts = false;

        var image = wheel.GetComponent<Image>();
        image.sprite = crosshair.circles[0];
        image.fillMethod = Image.FillMethod.Radial360;
        image.type = Image.Type.Filled;
        image.fillAmount = 0.0f;

        wheel.AddComponent<SliderToFillAmount>();
        var stfa = wheel.GetComponent<SliderToFillAmount>();
        stfa.targetSlider = slider;
        stfa.maxFill = 1;

        wheel.AddComponent<FadeOutBars>();
        var fob = wheel.GetComponent<FadeOutBars>();
        fob.fadeOutTime = 4;

        wheel.SetActive(true);
        return wheel;
    }

    private static GameObject MakeGun() {
        var gun = UIHelper.CreateImage();

        var canvas = UIHelper.CreateOverlay(persistent: true);
        canvas.GetComponent<Canvas>().sortingOrder = 10;

        var rt = gun.GetComponent<RectTransform>();
        rt.SetParent(canvas.GetComponent<RectTransform>());
        rt.SetPivot(PivotPresets.MiddleLeft);
        rt.anchoredPosition = new Vector2(45, 0);

        gun.SetActive(true);

        return gun;
    }

    private static float BattleVolume() {
        var musicManager = MonoSingleton<MusicManager>.Instance;
        return Math.Max(musicManager.battleTheme.volume, musicManager.bossTheme.volume);
    }

    private static GameObject MakeFist() {
        var fist = UIHelper.CreateImage();

        var canvas = UIHelper.CreateOverlay(persistent: true);
        canvas.GetComponent<Canvas>().sortingOrder = 10;

        var rt = fist.GetComponent<RectTransform>();
        rt.SetParent(canvas.GetComponent<RectTransform>());
        rt.SetPivot(PivotPresets.MiddleRight);
        rt.anchoredPosition = new Vector2(-50, 0);

        fist.SetActive(true);

        return fist;
    }

    public void Update() {
        this.UpdateWheel();
        this.UpdateIconTimer();
        this.UpdateGun();
        this.UpdateFist();
    }

    private void UpdateIconTimer() {
        var musicManager = MonoSingleton<MusicManager>.Instance;
        var battleVolume = Math.Max(musicManager.battleTheme.volume, musicManager.bossTheme.volume);

        if (battleVolume > 0) {
            this.iconTimer = 5;
        } else {
            this.iconTimer = Mathf.MoveTowards(this.iconTimer, 0, Time.deltaTime);
        }
    }

    private void UpdateWheel() {
        var crosshair = MonoSingleton<HUDOptions>.Instance?.GetComponentInChildren<Crosshair>();
        if (this.wheel == null) {
            if (crosshair == null) {
                return;
            }

            this.wheel = MakeWheel(crosshair);
        }

        this.wheel.SetActive(crosshair != null);

        var wc = MonoSingleton<WeaponCharges>.Instance;
        if (wc != null) {
            var cbs = MonoSingleton<ColorBlindSettings>.Instance;
            this.wheel.GetComponent<Slider>().value = wc.raicharge;
            this.wheel.GetComponent<Image>().color = wc.raicharge < 4 ? cbs.railcannonChargingColor : cbs.railcannonFullColor;
        }
    }

    private void UpdateGun() {
        if (this.gun == null) {
            this.gun = MakeGun();
        }

        var weaponHud = MonoSingleton<WeaponHUD>.Instance;

        this.gun.SetActive(weaponHud != null);

        if (weaponHud != null) {
            var img = this.gun.GetComponent<Image>();
            var rt = this.gun.GetComponent<RectTransform>();
            var icon = weaponHud.GetComponentInChildren<Image>();

            img.sprite = icon.sprite;
            img.color = icon.color with { a = Math.Min(1, this.iconTimer) };
            rt.sizeDelta = new Vector2(img.sprite.texture.width, img.sprite.texture.height) * this.gunScale;
        }
    }

    private void UpdateFist() {
        if (this.fist == null) {
            this.fist = MakeFist();
        }

        var fistControl = MonoSingleton<FistControl>.Instance;
        var fistIcon = fistControl?.fistIcon;

        this.fist.SetActive(fistIcon != null && fistControl!.activated);

        if (fistIcon != null) {
            var img = this.fist.GetComponent<Image>();
            var rt = this.fist.GetComponent<RectTransform>();

            img.sprite = fistIcon.sprite;
            img.color = fistIcon.color with { a = Math.Min(1, this.iconTimer) };
            rt.sizeDelta = new Vector2(img.sprite.texture.width, img.sprite.texture.height) * this.fistScale;
        }
    }
}