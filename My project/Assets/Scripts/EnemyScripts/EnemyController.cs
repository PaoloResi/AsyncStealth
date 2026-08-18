using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{

    private enum State {Patrol, Chase, Attack, Search}
    
    private NavMeshAgent agent;
    private List<PatrolIdentity> patrolPoints = new List<PatrolIdentity>();
    private float arriveDistance = 0.5f;
    private int patrolStep;

    public float health;

    public GameObject projectile;

    public Transform player;
    public LayerMask whatIsPlayer;

    public float timeBetweenAttack;
    bool alreadyAttacked;

    public float sightRange, attackRange;
    [Range(0f, 180f)] public float sightAngle = 60f;
    public float eyeHeight = 1f;
    public bool playerInSightRange, playerInAttackRange;

    private State state = State.Patrol;
    private Vector3 lastKnownPosition;
    private float searchTimer;
    private float searchDuration = 6f;
    private float searchRadius = 3f;
    private float searchPointWait = 1.5f;
    private float pointWaitTimer;

    public RaycastHit sightHit;

    public RaycastHit attackHit;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("PlayerCapsule").transform;
    }

    private void Update()
    {

        bool canSee = CanSeePlayer(out float distance);

        if (canSee)
        {
            lastKnownPosition = player.position;
            state = distance <= attackRange ? State.Attack : State.Chase;
        }
        else if (state == State.Chase || state == State.Attack)
        {
            state = State.Search;
            searchTimer = searchDuration;
            pointWaitTimer = 0f;
            agent.SetDestination(lastKnownPosition);
        }

        switch (state)
        {
            case State.Patrol:
                Move();
                break;
            case State.Chase:
                ChasePlayer(); 
                break;
            case State.Attack:
                AttackPlayer();
                break;
            case State.Search:
                Search();
                break;
        }

        print(state);

        //Debug.DrawRay(transform.position, transform.forward * sightRange, Color.red);
        //Physics.Raycast(transform.position, transform.forward, out sightHit, sightRange);
        //if (sightHit.collider == null) playerInSightRange = false;
        //else if (sightHit.transform.parent.gameObject == player.gameObject) playerInSightRange = true;
        //else
        //{
        //    playerInSightRange = false;
        //}

        //Physics.Raycast(transform.position, transform.forward, out attackHit, attackRange);
        //if (attackHit.collider == null) playerInAttackRange = false;    
        //else if (attackHit.transform.parent.gameObject == player.gameObject) playerInAttackRange = true;
        //else playerInAttackRange = false;

        //if (!playerInSightRange && !playerInAttackRange) Move();
        //else if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        //else if (playerInSightRange && playerInAttackRange) AttackPlayer();
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


    private bool CanSeePlayer (out float distance)
    {
        Vector3 eye = transform.position;
        Vector3 target = player.position;
        Vector3 toPlayer = target - eye;
        distance = toPlayer.magnitude;

        if (distance > sightRange) return false;

        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        if (Vector3.Angle(transform.forward, flat) > sightAngle * 0.5f) return false;

        if (Physics.Raycast(eye, toPlayer.normalized, out RaycastHit hit, distance))
        {
            return hit.transform.root == player.root;
        }

        return false;
    }

    public void ChasePlayer()
    {
        agent.SetDestination(player.position);

        Vector3 lookat = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);

        transform.LookAt(lookat);
    }

    public void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        Vector3 lookat = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);

        transform.LookAt(lookat);

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

    private void Search()
    {
        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            state = State.Patrol;
            agent.SetDestination(transform.position);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= arriveDistance)
        {
            pointWaitTimer -= Time.deltaTime;
            if (pointWaitTimer <= 0f)
            {
                agent.SetDestination(RandomPointNear(lastKnownPosition));
                pointWaitTimer = searchPointWait;
            }
            else
            {
                transform.Rotate(0f, 90f * Time.deltaTime, 0f);
            }
        }
    }

    private Vector3 RandomPointNear(Vector3 center)
    {
        for (int i = 0; i < 10;  i++)
        {
            Vector3 point = center + Random.insideUnitSphere * searchRadius;
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return center;
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

    private void OnDrawGizmos()
    {
        Vector3 eye = transform.position + Vector3.up;
        Gizmos.color = Color.yellow;
        Vector3 left = Quaternion.Euler(0, -sightAngle * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, sightAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(eye, left * sightRange);
        Gizmos.DrawRay(eye, right * sightRange);

        if (Application.isPlaying && state == State.Search)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(lastKnownPosition, searchRadius);
        }
    }
}
