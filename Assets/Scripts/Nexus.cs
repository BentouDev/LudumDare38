using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nexus : MonoBehaviour, IDamageable
{
    public int MaxHealth = 1000;

    [HideInInspector]
    public int CurrentHealth;

    private float LastHurtTime;

    public bool IsAlive()
    {
        return CurrentHealth > 0;
    }

    public void Init()
    {
        CurrentHealth = MaxHealth;
    }

    public void Damage(int points, float attackTime, Vector3 direction)
    {
        if (attackTime < LastHurtTime)
            return;

        LastHurtTime = Time.time;

        CurrentHealth -= points;
    }
}
