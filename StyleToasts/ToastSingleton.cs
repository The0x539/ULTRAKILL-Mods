using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace StyleToasts;

// this is probably meant to be accomplished with some Object.instantiate / "template" thing
public sealed class ToastSingleton : MonoSingleton<ToastSingleton> {
    public GameObject Overlay { get; private set; }
    public Font Font { get; private set; }

    protected override void Awake() {
        // this seems extremely dumb, but I have thus far failed to find a better solution
        this.Font = Resources.FindObjectsOfTypeAll<Font>().First(f => f.name == "VCR_OSD_MONO_1.001");

        this.Overlay = new GameObject("Overlay");

        var canvas = this.Overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 1000;
        canvas.scaleFactor = 5f;

        this.Overlay.AddComponent<GraphicRaycaster>();
    }
}