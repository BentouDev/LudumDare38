using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pawn : MonoBehaviour, IDamageable
{
    [Header("Debug")]
    public bool DrawDebug;

    [Header("Physics")]
    public float Gravity = -9.81f;
    public float ForceThreshold = 0.01f;
    public Rigidbody Rigidbody;
    public Transform RaycastOrigin;
    public LayerMask RaycastMask;
    public float RaycastLength;

    public bool IsGrounded { get; private set; }

    [HideInInspector]
    public RaycastHit LastGroundHit;

    [Header("Animation")]
    public float RotateSpeed = 25;
    public Animator Anim;
    public float AttackDelay;
    public AnimationCurve AttackMovement;
    public float AttackStart;
    public float AttackEnd;

    [System.Serializable]
    public struct AttackInfo
    {
        [SerializeField]
        public float AttackDelay;

        [SerializeField]
        public AnimationCurve AttackMovement;

        [SerializeField]
        public float AttackStart;

        [SerializeField]
        public float AttackEnd;
    }

    [Header("Attacks")]
    [Range(-1,1)]
    public float BlockRange = 0;
    public float StunTime = 1.5f;
    public float PushStrength = 4;
    public List<AttackInfo> Attacks;

    [Header("Movement")]
    public float MaxSpeed = 6;
    public float Acceleration = 1;
    public float Friction = 1;

    [HideInInspector]
    private float CurrentSpeed;

    [Header("Health")]
    public int MaximumHealth = 100;
    public int StartingHealth = 100;

    [HideInInspector]
    public int CurrentHealth;

    protected Transform CurrentTarget;

    protected Vector3 CurrentDirection = Vector3.zero;

    protected Vector3 DesiredForward;

    protected Vector3 Velocity;

    protected Vector3 ForceSum;

    private bool IsBlocking;
    private bool IsAttacking;

    private float LastHurtTime;

    private bool IsStunned { get { return Time.time - LastHurtTime < StunTime; } }

    public void Init()
    {
        CurrentDirection = Vector3.zero;
        CurrentHealth    = Mathf.Min(MaximumHealth, StartingHealth);

        if (!Rigidbody)
            Rigidbody = GetComponentInChildren<Rigidbody>();

        if (!Anim)
            Anim = GetComponentInChildren<Animator>();

        foreach (var hitbox in GetComponentsInChildren<HitBox>())
        {
            hitbox.Owner = this;
        }

        DoInit();
    }

    protected virtual void DoInit()
    { }

    protected virtual void DoUpdate()
    { }

    public void Heal(int hp)
    {
        CurrentHealth += hp;
        CurrentHealth  = Mathf.Min(CurrentHealth, MaximumHealth);
    }

    public void TakeDamage(int hp)
    {
        CurrentHealth -= hp;
        CurrentHealth  = Mathf.Min(CurrentHealth, MaximumHealth);
    }

    public void Damage(int hp, float attackTime, Vector3 direction)
    {
        if (attackTime < LastHurtTime)
            return;

        if (IsBlocking)
        {
            var dot = Vector3.Dot(transform.forward, direction);
            if (dot < BlockRange)
            {
                Debug.Log("Blocked! " + Time.time);
                return;
            }
        }

        LastHurtTime = Time.time;

        ForceSum += direction * PushStrength;

        Debug.DrawRay(transform.position, direction * hp, Color.magenta, 5.0f);
    
        TakeDamage(hp);
    }
    
    public bool IsAlive()
    {
        return CurrentHealth > 0;
    }

    public void CheckGrounded()
    {
        IsGrounded = Physics.Raycast(RaycastOrigin.position, Vector3.down, out LastGroundHit, RaycastLength, RaycastMask);

        if (IsGrounded)
        {
            Debug.DrawLine(LastGroundHit.point, LastGroundHit.point + LastGroundHit.normal, Color.cyan);
        }
    }

    public float GetMaxSpeed()
    {
        if (IsBlocking)
        {
            return MaxSpeed * 0.5f;
        }

        return MaxSpeed;
    }

    public void Block()
    {
        if (IsStunned)
            return;

        IsBlocking = true;
    }

    public void Attack()
    {
        if (IsStunned)
            return;

        if (IsAttacking)
            return;

        if (!Anim)
            return;

        StartCoroutine(PerformAttack(AttackDelay, "Attack", AttackMovement,
            AttackStart, AttackEnd, GetComponentsInChildren<HitBox>().ToList()));
    }

    IEnumerator PerformAttack(float duration, string attackName, AnimationCurve moveCurve, float hurtStart, float hurtEnd, List<HitBox> hitboxes)
    {
        IsAttacking = true;
        Anim.SetTrigger(attackName);

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            if (IsStunned)
                break;

            elapsed += Time.deltaTime;
            
            foreach (HitBox hitbox in hitboxes)
            {
                hitbox.SetEnabled(elapsed > hurtStart && elapsed < hurtEnd);
            }

            float t        =  elapsed / duration;
            float oldSpeed = (CurrentSpeed / MaxSpeed) * Mathf.Clamp(0.5f - t, 0, 0.5f);

            Vector3 direction = CurrentTarget ? DesiredForward : CurrentDirection.normalized;

            SimulateMovement(direction * Mathf.Max(moveCurve.Evaluate(t), oldSpeed));

            yield return null;
        }

        foreach (HitBox hitbox in hitboxes)
        {
            hitbox.SetEnabled(false);
        }

        IsAttacking = false;
    }

    public void OnUpdate()
    {
        IsBlocking = false;

        DoUpdate();

        ProcessAnimation();
    }

    protected void ProcessAnimation()
    {
        if (!Anim)
            return;

        Anim.SetFloat("Forward", CurrentSpeed / MaxSpeed);
        Anim.SetBool ("Block",  !IsStunned && !IsAttacking && IsGrounded && IsBlocking);
        Anim.SetBool ("Air",    !IsGrounded);
        Anim.SetBool ("Hit",     IsStunned);
    }

    private void SimulateMovement(Vector3 direction)
    {
        if (!Game.Instance.IsPlaying())
            return;

        if (IsStunned)
            return;

        if (direction.magnitude > ForceThreshold)
        {
            CurrentDirection = direction;
            CurrentSpeed += Acceleration * Time.fixedDeltaTime;
        }
        else
        {
            CurrentSpeed -= Friction * Time.fixedDeltaTime;
        }

        CurrentSpeed = Mathf.Clamp(CurrentSpeed, 0, GetMaxSpeed());

        var flatVelocity = new Vector3(CurrentDirection.x, 0, CurrentDirection.z);
        var appliedVelocity = flatVelocity * CurrentSpeed;

        if (IsGrounded)
        {
            Quaternion slope = Quaternion.FromToRotation(Vector3.up, LastGroundHit.normal);
            appliedVelocity = slope * appliedVelocity;

            ForceSum.y = 0;
        }
        else
        {
            ForceSum.y += Gravity * Time.fixedDeltaTime;
        }

        Velocity = appliedVelocity;
    }

    protected void ProcessMovement(Vector3 direction)
    {
        if (IsAttacking)
            return;

        SimulateMovement(direction);
    }

    void FixedUpdate()
    {
        if(!IsAlive())
            Velocity = Vector3.zero;

        CheckGrounded();

        Rigidbody.velocity = Velocity + ForceSum;

        ForceSum = Vector3.Lerp(ForceSum, Vector3.zero, Time.fixedDeltaTime);
        if (Mathf.Abs(ForceSum.magnitude) < ForceThreshold)
        {
            ForceSum = Vector3.zero;
        }

        if (Velocity.magnitude > 0)
        {
            transform.forward = Vector3.Lerp (
                transform.forward, 
                new Vector3(Velocity.x, 0, Velocity.z).normalized,
                Time.fixedDeltaTime * RotateSpeed
            );
        }
    }

    void LateUpdate()
    {
        if (!Game.Instance.IsPlaying() || IsAttacking)
            return;

        if (Camera.main.transform.forward.magnitude > 0)
        {
            DesiredForward = Vector3.Slerp(DesiredForward, new Vector3(
                Camera.main.transform.forward.x,
                0,
                Camera.main.transform.forward.z
            ), Time.deltaTime * 10);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!RaycastOrigin)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(RaycastOrigin.position, RaycastOrigin.position + Vector3.down * RaycastLength);
    }

    protected void OnGUI()
    {
        if (!DrawDebug)
            return;
        
        GUI.Label(new Rect(10, 10, 200, 30), "Grounded : " + IsGrounded);
        GUI.Label(new Rect(10, 30, 200, 30), "Velocity : " + Velocity);
        GUI.Label(new Rect(10, 50, 200, 30), "ForceSum : " + ForceSum);
        GUI.Label(new Rect(10, 70, 200, 30), "CurInput : " + CurrentDirection);
        GUI.Label(new Rect(10, 90, 200, 30), "Forward : " + CurrentSpeed / MaxSpeed);
        GUI.Label(new Rect(10, 110, 200, 30), "Attacking : " + IsAttacking);
        GUI.Label(new Rect(10, 130, 200, 30), "Blocking : " + IsBlocking);
    }
}
