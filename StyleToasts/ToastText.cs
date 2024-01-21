using UnityEngine;
using UnityEngine.UI;

namespace StyleToasts;

public sealed class ToastText : MonoBehaviour {
    private Text textComponent;
    private RectTransform rectComponent;
    private float age = 0;

    public string Text {
        get => this.textComponent.text;
        set => this.textComponent.text = value;
    }

    public Vector3 Size {
        get => this.rectComponent.localScale;
        set => this.rectComponent.localScale = value;
    }

    public void Awake() {
        var obj = this.gameObject;

        var rt = obj.AddComponent<RectTransform>();
        obj.AddComponent<CanvasRenderer>();
        rt.pivot = rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.parent = ToastSingleton.Instance.Overlay.transform;
        rt.localScale = Vector3.one * 0.02f;
        rt.sizeDelta = Vector2.one * 500;
        this.rectComponent = rt;

        var text = obj.AddComponent<Text>();
        text.font = ToastSingleton.Instance.Font;
        text.fontSize = 50;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        // TODO: it still gets antialiased kinda
        text.mainTexture.filterMode = FilterMode.Point;
        this.textComponent = text;
    }

    public void Update() {
        this.age += Time.deltaTime;
        if (this.age >= 2) {
            Destroy(this.gameObject);
            return;
        } else if (this.age >= 1) {
            var a = 2 - this.age;
            a *= a;
            this.textComponent.canvasRenderer.SetAlpha(a);
        }

        var myPos = this.transform.position;
        var camPos = Camera.main.transform.position;
        var delta = camPos - myPos;

        var distanceScale = Mathf.Log10(delta.magnitude);

        this.transform.Translate(Vector3.up * Time.deltaTime * distanceScale);
        //this.transform.localScale = Vector3.one * 0.02f * distanceScale;
        this.transform.LookAt(myPos - delta);
    }
}