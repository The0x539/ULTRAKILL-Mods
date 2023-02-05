﻿using System;

using BepInEx;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

namespace StyleToasts;

public sealed class ToastText : MonoBehaviour {
    private static readonly GameObject overlay = CreateOverlay();
    private static readonly Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

    private Text textComponent;

    public string Text {
        get => this.textComponent.text;
        set => this.textComponent.text = value;
    }

    public void Awake() {
        var obj = this.gameObject;

        var rt = obj.AddComponent<RectTransform>();
        obj.AddComponent<CanvasRenderer>();
        rt.sizeDelta = new Vector2(200, 50);
        rt.pivot = rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.parent = overlay.transform;

        var text = obj.AddComponent<Text>();
        text.text = "Text";
        text.font = font;
        text.fontSize = 32;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        this.textComponent = text;
    }

    private static GameObject CreateOverlay() {
        var overlay = new GameObject("Overlay");

        var canvas = overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 1000;

        overlay.AddComponent<GraphicRaycaster>();

        return overlay;
    }
}