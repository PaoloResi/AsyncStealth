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

    public float health;

    public GameObject projectile;

    public Transform player;
    public LayerMask whatIsInteruption,whatIsPlayer;

    public float timeBetweenAttack;
    bool alreadyAttacked;

    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        enemyCapsuleCollider = GetComponentInChildren<Collider>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("PlayerCapsule").transform;
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);


        if (!playerInSightRange && !playerInAttackRange) Move();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange) AttackPlayer();
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
        int phase  = (step % period);
        return phase < count ? phase : period - phase;
    }

    public void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    public void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttack);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}
