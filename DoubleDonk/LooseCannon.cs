using System;

using UnityEngine;
using UnityEngine.UI;

namespace DoubleDonk;

public sealed class LooseCannon : MonoBehaviour {
    private static void PrintComponents(GameObject o) {
        foreach (var c in o.GetComponents<Component>()) {
            Console.WriteLine(c.GetType().Name + " from " + c.GetType().Assembly.FullName);
        }
    }

#nullable disable
    private Image sliderFill;
    private Slider chargeSlider;
    private Animator anim;
    private GameObject cannonballPrefab;
    private GameObject chargeSoundPrefab;
    private AudioSource chargeSound;
#nullable enable

    private bool charging = false;
    private float chargeAmount = 0;
    private bool reloading = false;

    private static GameObject BuildCannonballPrefab() {
        var srsCannon = GunSetter.Instance.rocketGreen[0].ToAsset().GetComponent<RocketLauncher>();
        var originalPrefab = srsCannon.cannonBall.gameObject;

        var obj = Instantiate(originalPrefab);
        obj.SetActive(false);

        Destroy(obj.GetComponent<Cannonball>());

        var collider = obj.AddComponent<SphereCollider>();
        collider.radius = 0.5f;

        var rb = obj.GetComponent<Rigidbody>();
        rb.mass = 1;
        rb.SetDensity(1);

        obj.transform.localScale *= 0.5f;

        obj.AddComponent<LooseCannonBall>();

        return obj;
    }

    public void Awake() {
        var shotgun = this.gameObject.GetComponent<Shotgun>();

        this.chargeSlider = shotgun.GetComponentInChildren<Slider>();
        this.chargeSlider.minValue = 0;
        this.chargeSlider.maxValue = 1;

        this.chargeSoundPrefab = shotgun.chargeSoundBubble;

        this.sliderFill = this.chargeSlider.GetComponentInChildren<Image>();

        this.anim = shotgun.GetComponentInChildren<Animator>();

        this.cannonballPrefab = BuildCannonballPrefab();

        Destroy(shotgun);
    }

    public void OnEnable() {
    }

    public void OnDisable() {
    }

    public void Update() {
        var inman = InputManager.Instance;
        var gunc = GunControl.Instance;
        var gsm = GameStateManager.Instance;

        var ignoreInput = !gunc.activated || inman.PerformingCheatMenuCombo() || gsm.PlayerInputLocked;

        if (!ignoreInput && inman.InputSource.Fire1.IsPressed && !this.reloading) {
            this.charging = true;
        }

        if (this.charging) {
            this.chargeAmount = Mathf.MoveTowards(this.chargeAmount, 1, Time.deltaTime);

            if (this.chargeAmount >= 1) {
                this.Shoot();
            }
        }

        if (!ignoreInput && this.charging && inman.InputSource.Fire1.WasCanceledThisFrame) {
            this.Shoot();
        }

        this.UpdateMeter();
        this.UpdateChargeSound();

        // I can't figure out how to get "animation receivers" to work the way the normal shotgun does.
        this.reloading = this.CheckIfReloading();
    }

    private void Shoot() {
        var camTransform = CameraController.Instance.transform;

        var cannonball = Instantiate(this.cannonballPrefab, camTransform.position + camTransform.forward * 1.5f, camTransform.rotation);
        cannonball.SetActive(true);

        cannonball.GetComponent<Rigidbody>().AddForce(camTransform.forward * 50f, ForceMode.VelocityChange);

        var cb = cannonball.GetComponent<LooseCannonBall>();
        cb.sourceWeapon = this.gameObject;
        cb.fuseTime -= this.chargeAmount;

        this.charging = false;
        this.chargeAmount = 0;

        this.anim.SetTrigger("Fire");
    }

    private void UpdateMeter() {
        float value;
        Color color;

        if (this.charging) {
            value = this.chargeAmount;
            color = Color.Lerp(Color.cyan, Color.red, this.chargeAmount);
        } else if (!this.reloading) {
            value = this.chargeSlider.maxValue;
            color = Color.blue;
        } else {
            value = this.chargeSlider.minValue;
            color = Color.black;
        }

        this.chargeSlider.value = value;
        this.sliderFill.color = color;
    }

    private void UpdateChargeSound() {
        if (this.charging) {
            this.chargeSound ??= Instantiate(this.chargeSoundPrefab).GetComponent<AudioSource>();
            this.chargeSound.pitch = 0.25f + this.chargeAmount;
            this.chargeSound.volume = 0.75f;
        } else if (this.chargeSound != null) {
            this.chargeSound.volume = 0;
            Destroy(this.chargeSound.gameObject);
            this.chargeSound = null;
        }
    }

    private bool CheckIfReloading() {
        if (this.anim.GetBool("Fire")) {
            return true;
        }

        for (var layer = 0; layer < this.anim.layerCount; layer++) {
            foreach (var info in this.anim.GetCurrentAnimatorClipInfo(layer)) {
                if (info.clip.name == "FireWithReload") {
                    return true;
                }
            }
        }

        return false;
    }
}
