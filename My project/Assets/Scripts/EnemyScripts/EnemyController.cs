using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{

    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;
    public float oldMoveSpeed;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;
    public float oldSprintSpeed;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    private CharacterController _controller;
    private Collider enemyCapsuleCollider;
    private NavMeshAgent agent;
    private List<PatrolIdentity> patrolPoints = new List<PatrolIdentity>();
    private float arriveDistance = 0.5f;
    private int patrolStep;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        enemyCapsuleCollider = GetComponentInChildren<Collider>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        Move();
    }

    public void SetPatrol(PatrolIdentity startPoint, Dictionary<string, PatrolIdentity> patrolPointDictionary)
    {   
        patrolPoints.Clear();
        if (startPoint == null) return;

        patrolPoints.Add(startPoint);
        PatrolIdentity currentPoint = startPoint;

        while (!string.IsNullOrEmpty(currentPoint.nextPoint))
        {
            if (!patrolPointDictionary.TryGetValue(currentPoint.nextPoint, out PatrolIdentity next))
            {
                Debug.LogWarning($"Patrol point '{currentPoint.nextPoint}' not found in dictionary.");
                break;
            }
            patrolPoints.Add(next);
            currentPoint = next;
        }
    }

    public void Move()
    {
        if (patrolPoints.Count == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= arriveDistance)
        {
            patrolStep++;
            int index = PingPongIndex(patrolStep, patrolPoints.Count);
            agent.destination = patrolPoints[index].transform.position;
        }
    }

    private static int PingPongIndex(int step, int count)
    {
        if (count <= 1) return 0;

        int period = (count - 1) * 2;
        int phase  = ((step % period) +  period) % period;
        return phase < count ? phase : period - phase;
    }
}
