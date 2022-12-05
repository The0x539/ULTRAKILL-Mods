using System.Collections;
using System.IO;

using BepInEx;

using UKUIHelper;

using UnityEngine;
using UnityEngine.UI;

namespace LeatHud;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("zed.uk.uihelper")]
public sealed class Plugin : BaseUnityPlugin {
    private GameObject? wheel;

    public void Awake() {
        this.Logger.LogInfo("Plugin loaded!");
    }

    private GameObject MakeWheel(Crosshair crosshair) {
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
        //image.sprite = MonoSingleton<PowerUpMeter>.Instance.GetComponent<Image>().sprite;

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

    public void Update() {
        if (this.wheel == null) {
            var crosshair = FindObjectOfType<Crosshair>();
            if (crosshair == null) {
                return;
            }

            this.wheel = this.MakeWheel(crosshair);
        }

        this.wheel.SetActive(FindObjectOfType<Crosshair>() != null);

        var wc = MonoSingleton<WeaponCharges>.Instance;
        var cbs = MonoSingleton<ColorBlindSettings>.Instance;
        if (wc != null) {
            this.wheel.GetComponent<Slider>().value = wc.raicharge;
            this.wheel.GetComponent<Image>().color = wc.raicharge < 4 ? cbs.railcannonChargingColor : cbs.railcannonFullColor;
        }
    }
}