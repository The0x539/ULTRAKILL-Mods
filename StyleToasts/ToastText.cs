using System;
using System.Linq;

using BepInEx;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

namespace StyleToasts;

public sealed class ToastText : MonoBehaviour {
    private static readonly GameObject overlay;
    private static readonly Font font;

    static ToastText() {
        // this seems extremely dumb, but I have thus far failed to find a better solution
        font = Resources.FindObjectsOfTypeAll<Font>().First(f => f.name == "VCR_OSD_MONO_1.001");

        overlay = overlay = new GameObject("Overlay");

        var canvas = overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 1000;
        canvas.referencePixelsPerUnit = 20;

        overlay.AddComponent<GraphicRaycaster>();
    }

    private Text textComponent;

    public string Text {
        get => this.textComponent.text;
        set => this.textComponent.text = value;
    }

    public void Awake() {
        var obj = this.gameObject;

        var rt = obj.AddComponent<RectTransform>();
        obj.AddComponent<CanvasRenderer>();
        rt.pivot = rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.parent = overlay.transform;
        //rt.localScale = new Vector3(0.125f, 0.125f, 0.125f);

        var text = obj.AddComponent<Text>();
        text.text = "Text";
        text.font = font;
        text.fontSize = 60;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        this.textComponent = text;
    }

    private static GameObject CreateOverlay() {
        var overlay = new GameObject("Overlay");

        var canvas = overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 1000;
        canvas.referencePixelsPerUnit = 20;

        overlay.AddComponent<GraphicRaycaster>();

        return overlay;
    }
}