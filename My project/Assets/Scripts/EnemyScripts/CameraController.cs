using UnityEngine;

public class CameraController : MonoBehaviour
{

    private enum State {Search,Looking, Found}
    private State state = State.Search;


    public Transform player;
    public float sightRange;
    [Range(0f, 180f)] public float sightAngle = 60f;

    public float eyeHeight = 1f;
    public float closeFillMultiplier = 4f;

    public float baseFillRate = 0.35f;
    public float detectionMeter;
    public float decayRate = 0.25f;
    public float decayDelay = 0.5f;
    private float decayDelayTimer;

    private bool alerted;

    float sweepAngle = 60f;
    float speed = 1f;
    float startYaw;
    EnemyController[] enemyControllers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("PlayerCapsule").transform;
        startYaw = transform.eulerAngles.y;
        enemyControllers = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

    }

    // Update is called once per frame
    void Update()
    {
        bool canSee = CanSeePlayer(out float distance);

        if (canSee)
        {
            state = State.Looking;
            decayDelayTimer = decayDelay;

            if (!alerted)
            {
                detectionMeter += DetectionRate(distance) * Time.deltaTime;

                if (detectionMeter >= 1f)
                {
                    detectionMeter = 1f;
                    alerted = true;
                }
            }

            if (alerted)
            {
                state = State.Found;
            }
           
        }
        else
        {
            if (decayDelayTimer > 0f) decayDelayTimer -= Time.deltaTime;
            else detectionMeter = Mathf.Max(0f, detectionMeter - decayRate * Time.deltaTime);

            if (detectionMeter <= 0f)
            {
                alerted = false;
                state = State.Search;
            }
        }

        switch (state)
        {
            case State.Search:
                Move();
                break;
            case State.Looking:
                break;
            case State.Found:
                GameObject nearestEnemy = FindNearestEnemy();
                if (nearestEnemy != null)
                {
                    nearestEnemy.GetComponent<EnemyController>().lastKnownPosition = player.position;
                }
                break;
        }

    }

    private bool CanSeePlayer(out float distance)
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * eyeHeight;
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

    private GameObject FindNearestEnemy()
    {
        Transform bestTarget = null;
        float closesDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (EnemyController potentialTarget in enemyControllers)
        {
            Transform potentialTargetTransform = potentialTarget.transform;
            Vector3 directionToTarget = potentialTargetTransform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closesDistanceSqr)
            {
                closesDistanceSqr = dSqrToTarget;
                bestTarget = potentialTargetTransform;
            }
        }

        return bestTarget.gameObject;
    }

    private float DetectionRate(float distance)
    {
        float closeness = 1f - Mathf.Clamp01(distance / sightRange);
        float mult = Mathf.Lerp(1f, closeFillMultiplier, closeness);

        return baseFillRate * mult;
    }

    private void Move()
    {
        float offset = Mathf.Sin(Time.time * speed) * sweepAngle;
        transform.rotation = Quaternion.Euler(0f, startYaw + offset, 0f);
    }

}
