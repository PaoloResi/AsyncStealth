using UnityEngine;
using UnityEngine.SceneManagement;

public class TurretController : MonoBehaviour
{

    private enum State { Search, Looking, Found }
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
    private GunLogic gunLogic;
    bool alreadyAttacked;
    public float timeBetweenAttack;
    public Transform gunHead;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SceneManager.GetActiveScene().name != "BuildingScene")
        {
            player = GameObject.Find("PlayerCapsule").transform;
            startYaw = transform.eulerAngles.y;
            gunLogic = GetComponentInChildren<GunLogic>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name != "BuildingScene")
        {
            bool canSee = CanSeePlayer(out float distance);

            print(canSee);

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

            print(state);

            switch (state)
            {
                case State.Search:
                    Move();
                    break;
                case State.Looking:
                    break;
                case State.Found:
                    AttackPlayer();
                    break;
            }
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


    private float DetectionRate(float distance)
    {
        float closeness = 1f - Mathf.Clamp01(distance / sightRange);
        float mult = Mathf.Lerp(1f, closeFillMultiplier, closeness);

        return baseFillRate * mult;
    }

    private void Move()
    {
        float offset = Mathf.Sin(Time.time * speed) * sweepAngle;
        gunHead.rotation = Quaternion.Euler(0f, startYaw + offset, 0f);
    }

    public void AttackPlayer()
    {
        FacePlayer();

        if (!alreadyAttacked)
        {
            gunLogic.shoot(player);
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttack);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void FacePlayer()
    {
        Vector3 toPlayer = player.position - gunHead.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        float desiredYaw = Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg;

        float delta = Mathf.DeltaAngle(startYaw, desiredYaw);
        float clamped = Mathf.Clamp(delta, -60f, 60f);

        gunHead.rotation = Quaternion.Euler(0f, startYaw + clamped, 0f);
    }

}
