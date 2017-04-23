using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ActionCamera : MonoBehaviour
{
    [Header("Target")]
    public string TargetTag = "Player";
    private Transform Target;
    private Transform LockOn;

    [Header("Input")]
    public string LookX = "Look X";
    public string LookY = "Look Y";

    [Header("Offset")]
    public Vector3 Offset;

    [Header("Speed")]
    public float SpeedX;
    public float SpeedY;

    [Header("Angles")]
    public float MaxAngleY =  60;
    public float MinAngleY = -60;
    
    private float AngleX;
    private float AngleY;

    public void Init()
    {
        var go = GameObject.FindGameObjectWithTag(TargetTag);
        Target = go ? go.transform : null;
    }

    void LateUpdate()
    {
        if (!Game.Instance.IsPlaying())
            return;

        OnUpdate();
    }

    public void SetLockOn(Transform lockOn)
    {
        LockOn = lockOn;
    }

    public void OnUpdate()
    {
        var rot = Quaternion.identity;

        if (LockOn)
        {
            var distance = transform.position - LockOn.position;
            var rawRot = Quaternion.LookRotation(-distance.normalized).eulerAngles;
                rot = Quaternion.Euler(rawRot.x, rawRot.y, rawRot.z);

            AngleY = rawRot.x;
            AngleX = rawRot.y;
        }
        else
        {
            AngleX += Input.GetAxis(LookX) * SpeedX;
            AngleY -= Input.GetAxis(LookY) * SpeedY;

            AngleY = Mathf.Min(Mathf.Max(MinAngleY, AngleY), MaxAngleY);

            rot = Quaternion.Euler(AngleY, AngleX, 0);
        }

        var pos = rot * Offset + (
            Target ? Target.position : Vector3.zero
        );

        transform.position = pos;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 360 * Time.deltaTime);
    }
}
