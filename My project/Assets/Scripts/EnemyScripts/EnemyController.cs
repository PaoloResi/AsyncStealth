using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{

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

    public RaycastHit sightHit;

    public RaycastHit attackHit;




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
        //playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        //playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        Debug.DrawRay(transform.position, transform.forward * sightRange, Color.red);
        Physics.Raycast(transform.position, transform.forward, out sightHit, sightRange);
        if (sightHit.collider == null) playerInSightRange = false;
        else if (sightHit.transform.parent.gameObject == player.gameObject) playerInSightRange = true;
        else
        {
            print(sightHit.transform.parent.gameObject.name);
            playerInSightRange = false;
        }

        Physics.Raycast(transform.position, transform.forward, out attackHit, attackRange);
        if (attackHit.collider == null) playerInAttackRange = false;    
        else if (attackHit.transform.parent.gameObject == player.gameObject) playerInAttackRange = true;
        else playerInAttackRange = false;

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
            //Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            //print("attacked player");
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
