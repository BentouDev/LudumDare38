using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HitBox : MonoBehaviour
{
    public bool OnEnter;
    public bool OnStay;
    public Pawn Owner;
    public int Damage;

    public bool Enabled;

    private float LastAttackTime;

    public void SetEnabled(bool enabled)
    {
        if (Enabled && enabled)
            return;

        if (enabled)
            LastAttackTime = Time.time;

        Enabled = enabled;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (!Enabled || !OnEnter)
            return;

        ProcessHit(collider);
    }

    void OnTriggerStay(Collider collider)
    {
        if (!Enabled || !OnStay)
            return;

        ProcessHit(collider);
    }

    void ProcessHit(Collider collider)
    {
        var pawn = collider.GetComponentInParent<Pawn>() ?? collider.GetComponentInChildren<Pawn>();
        if (pawn == null || pawn == Owner)
            return;

        var direction = Owner.transform.position - pawn.transform.position;
        var dot = Vector3.Dot(direction, Owner.transform.forward);
        
        pawn.Damage(Damage, LastAttackTime, -direction.normalized);
    }

    void OnDrawGizmosSelected()
    {
        if (Enabled)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 2);
        }
    }
}
