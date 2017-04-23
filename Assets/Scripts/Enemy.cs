using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Pawn
{
    [Header("AI")]
    public bool DrawPath;

    public float ThinkLag = 0.5f;
    public float CornerProximity = 0.25f;
    public float MaxPlayerProximity = 10;
    public float MinPlayerProximity = 1;

    [Header("AttackZone")]
    public Vector3 Offset;
    public float Radius;

    private NavMeshPath NexusPath;
    private NavMeshPath PlayerPath;
    private NavMeshPath TargetPath;

    private float LastThinkTime;
    private Transform Target;
    private int PathProgress;
    private bool HasPath;
    private float DistanceToPlayer;
    private bool InAttackZone;

    void OnDrawGizmosSelected()
    {
        if (InAttackZone)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(transform.position + transform.rotation * Offset, Radius);
    }

    protected override void DoInit()
    {
        NexusPath  = new NavMeshPath();
        PlayerPath = new NavMeshPath();
    }

    bool CheckAttackZone()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + transform.rotation * Offset, Radius);
        InAttackZone = colliders.Any(c => c.GetComponentInParent<Pawn>() == Game.Instance.Pawn || c.GetComponentInParent<Nexus>());

        return InAttackZone;
    }

    protected override void DoUpdate()
	{
	    if (!IsAlive())
	        return;

	    if (Time.time - LastThinkTime > ThinkLag)
	    {
	        LastThinkTime = Time.time;
            
	        if (ObtainNewtarget())
	        {
	            PathProgress = 0;
	        }
	    }

	    if (CheckAttackZone())
	        Attack();

        if (Target == null 
        ||  PathProgress + 1 >= TargetPath.corners.Length 
        || DistanceToPlayer < MinPlayerProximity)
	    {
	        ProcessMovement(Vector3.zero);
	        return;
	    }

	    if (DrawPath)
	    {
	        Debug.DrawLine(transform.position, TargetPath.corners[PathProgress], Color.yellow);

            for (int i = PathProgress; i < TargetPath.corners.Length - 1; i++)
	        {
	            Debug.DrawLine(TargetPath.corners[i], TargetPath.corners[i + 1], Color.white);
	        }
	    }

        var diff = transform.position - TargetPath.corners[PathProgress + 1];

	    if (diff.magnitude < CornerProximity)
	        PathProgress++;
        
        Debug.DrawRay(transform.position, -diff.normalized, Color.red);

        ProcessMovement(-diff.normalized);
	}

    private bool ObtainNewtarget()
    {
        Transform oldTarget = Target;

        DistanceToPlayer = (transform.position - Game.Instance.Pawn.transform.position).magnitude;
        
        if (DistanceToPlayer <= MaxPlayerProximity && CalcPath(Game.Instance.Pawn.transform.position, PlayerPath))
        {
            Target = Game.Instance.Pawn.transform;
            TargetPath = PlayerPath;
        }
        else if (CalcPath(Game.Instance.Nexus.transform.position, NexusPath))
        {
            Target = Game.Instance.Nexus.transform;
            TargetPath = NexusPath;
        }

        return Target != oldTarget;
    }

    private bool CalcPath(Vector3 position, NavMeshPath path)
    {
        return NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, path);
    }

    new void OnGUI()
    {
        if (!DrawDebug)
            return;

        base.OnGUI();

        GUI.Label(new Rect(10, 150, 200, 30), "PathProgess : " + PathProgress);
    }
}
