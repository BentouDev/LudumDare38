using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerPawn : Pawn
{
    [Header("LockOn")]
    public LockOnUI LockUI;
    public float LockOnRadius = 15;

    [Header("Input")]
    public string MoveX = "Move X";
    public string MoveY = "Move Y";

    private Vector2 CurrentInput = Vector2.zero;
    
    protected void LockOnPawn(Pawn pawn)
    {
        LockOn(pawn ? pawn.transform : null);
    }

    protected void LockOn(Transform target)
    {
        CurrentTarget = target;

        Game.Instance.ActionCamera.SetLockOn(CurrentTarget);

        if (!LockUI)
            return;

        LockUI.SetTarget(CurrentTarget);
    }

    protected void ProcessTargets(int offset = 0)
    {
        if (offset == 0 && CurrentTarget)
        {
            // Unset lock
            LockOn(null);
            return;
        }

        var pawns = FindObjectsOfType<Pawn>().ToList();
            pawns.Remove(this);

        // If offset ==  0 => take middle one (default)
        // If offset ==  1 => take first from right
        // If offset == -1 => take first from left

        var byDistance = pawns.Where(p =>
        {
            var diff = p.transform.position - transform.position;
            if (diff.magnitude > LockOnRadius)
                return false;

            if (!p.GetComponentInChildren<Renderer>().isVisible)
                return false;

            bool isOnGoodSide = offset == 0;

            if (offset != 0)
            {
                var localOffset = transform.InverseTransformPoint(p.transform.position);
                isOnGoodSide = localOffset.x * offset > 0;
            }

            return isOnGoodSide;

        }).OrderBy(p =>
        {
            var diff = p.transform.position - transform.position;
            return diff.magnitude;
        });

        var target = offset == 0 ? byDistance.FirstOrDefault() : byDistance.Skip(1).FirstOrDefault();
        if (target != null)
        {
            LockOnPawn(target);
        }
    }

    /*protected Transform FindLockTarget()
    {
        var pawns = FindObjectsOfType<Pawn>().ToList();
            pawns.Remove(this);

        Transform closest = null;
        float     dotProduct = -2;
        foreach (var target in pawns)
        {
            var rendr = target.GetComponentInChildren<Renderer>();
            if (!rendr || !rendr.isVisible)
                continue;
            
            var localPoint = Camera.main.transform.InverseTransformPoint(target.transform.position).normalized;
            var forward    = Vector3.forward;
            var test       = Vector3.Dot(localPoint, forward);
            if (test > dotProduct)
            {
                dotProduct = test;
                closest = target.transform;
            }
        }

        return closest;
    }*/

    protected override void DoUpdate()
    {
        CurrentInput.x = Input.GetAxis(MoveX);
        CurrentInput.y = Input.GetAxis(MoveY);
        
        if (Input.GetButtonDown("Attack"))
            Attack();

        if (Input.GetButton("Block"))
            Block();

        if (Input.GetButtonDown("LockOn"))
            ProcessTargets();

        if (Input.GetButtonDown("NextLockOn"))
            ProcessTargets(1);

        if (Input.GetButtonDown("PreviousLockOn"))
            ProcessTargets(-1);

        if (CurrentTarget)
        {
            var rawDistance = transform.position - CurrentTarget.position;
            DesiredForward = -Vector3.Normalize(rawDistance);
        }

        var flatVelocity = new Vector3(CurrentInput.x, 0, CurrentInput.y);
        var direction    = Quaternion.LookRotation(Vector3.Normalize(DesiredForward)) * flatVelocity;

        ProcessMovement(direction.normalized);
    }

    new void OnGUI()
    {
        if (!DrawDebug)
            return;

        base.OnGUI();

        GUI.Label(new Rect(10, 150, 200, 30), "LockedOn : " + CurrentTarget);
    }
}
