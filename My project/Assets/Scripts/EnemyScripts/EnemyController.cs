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


    public float detectionMeter;
    public float baseFillRate = 0.35f;
    public float closeFillMultiplier = 4f;
    public float decayRate = 0.25f;
    public float decayDelay = 0.5f;
    public bool isAlerted;

    private float decayDelayTimer;

    public RaycastHit sightHit;

    public RaycastHit attackHit;
    private GunLogic gunLogic;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("PlayerCapsule").transform;
        gunLogic = GetComponentInChildren<GunLogic>();

    }

    private void Update()
    {

        bool canSee = CanSeePlayer(out float distance);

        if (canSee)
        {
            lastKnownPosition = player.position;
            decayDelayTimer = decayDelay;

            if (!isAlerted)
            {
                detectionMeter += DetectionRate(distance) * Time.deltaTime;
                
                if (detectionMeter >= 1f)
                {
                    detectionMeter = 1f;
                    isAlerted = true;
                }
                else
                {
                    FacePlayer();
                    if (state == State.Patrol) agent.SetDestination(transform.position);
                }
            }

            if (isAlerted)
            {
                state = distance <= attackRange ? State.Attack : State.Chase;
            }
        }
        else 
        {
            if (decayDelayTimer > 0f) decayDelayTimer -= Time.deltaTime;
            else detectionMeter = Mathf.Max(0f, detectionMeter - decayRate * Time.deltaTime);

            if (detectionMeter <= 0f) isAlerted = false;

            if (state == State.Chase || state == State.Attack)
            {
                state = State.Search;
                searchTimer = searchDuration;
                pointWaitTimer = 0f;
                agent.SetDestination(lastKnownPosition);
            }
            
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

        FacePlayer();
    }

    public void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        FacePlayer();

        if (!alreadyAttacked)
        {
            gunLogic.shoot();
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

    private float DetectionRate(float distance)
    {
        float closeness = 1f - Mathf.Clamp01(distance / sightRange);
        float mult = Mathf.Lerp(1f, closeFillMultiplier, closeness);

        return baseFillRate * mult;
    }

    private void FacePlayer()
    {
        Vector3 lookAt = new Vector3 (player.position.x, transform.position.y, transform.position.z);
        transform.LookAt(lookAt);
    }

    public void TakeDamage(int damage)
    {
        if (state == State.Patrol || state == State.Search)
        {
            Invoke(nameof(DestroyEnemy), 0.5f);
        }
        else
        {
            health -= damage;
        }

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
