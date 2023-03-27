using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace DoubleDonk;

public sealed class LooseCannonBall : MonoBehaviour {
#nullable disable
    private Rigidbody rb;
    private Collider col;
    internal GameObject sourceWeapon;
#nullable enable

    internal float fuseTime = 1;

    private Vector2 lastVelocity;
    private List<EnemyIdentifier> donkVictims = new();

    public void Awake() {
        this.rb = this.GetComponent<Rigidbody>();
        this.col = this.GetComponent<Collider>();
        this.donkVictims = new();
    }

    public void FixedUpdate() {
        this.lastVelocity = this.rb.velocity;

        this.fuseTime = Mathf.MoveTowards(this.fuseTime, 0, Time.fixedDeltaTime);
        if (this.fuseTime <= 0) {
            this.Explode();
        }
    }

    public void OnCollisionEnter(Collision collision) {
        if (collision.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out var eidid)) {
            var eid = eidid.eid;

            if (!this.donkVictims.Contains(eid)) {
                var direction = (eid.GetComponent<Rigidbody>().position - this.rb.position);
                direction = direction.normalized;
                direction.y += 1.5f;
                direction = direction.normalized;

                var force = direction * 300;

                if (eid.TryGetComponent<Zombie>(out var zombie)) {
                    zombie.KnockBack(force);
                } else if (eid.TryGetComponent<Machine>(out var machine)) {
                    machine.KnockBack(force);
                } else if (eid.TryGetComponent<Statue>(out var statue)) {
                    statue.KnockBack(force);
                }

                this.donkVictims.Add(eid);
            }
        } else {
            var speed = this.lastVelocity.magnitude;
            var direction = this.lastVelocity.normalized;
            var newDirection = Vector3.Reflect(direction, collision.contacts[0].normal);
            this.rb.velocity = newDirection * speed;
        }
    }

    private void Explode() {
        Destroy(this.gameObject);

        Instantiate(GunSetter.Instance.shotgunGrenade[0].GetComponent<Shotgun>().explosion, this.rb.position, Quaternion.identity);

        var hits = Physics.OverlapSphere(this.rb.position, 3);
        foreach (var hit in hits) {
            var obj = hit.gameObject;
            var eid = obj.GetComponentInParent<EnemyIdentifier>();
            if (eid != null) {
                if (this.donkVictims.Contains(eid)) {
                    this.donkVictims.Remove(eid);
                    StyleHUD.Instance.AddPoints(100, "<color=yellow>DONK!</color>", this.sourceWeapon, eid, 2);
                }
            }
        }
    }
}
