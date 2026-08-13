using UnityEngine;
using System.Collections.Generic;

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
    private List<PatrolIdentity> patrolPoints = new List<PatrolIdentity>();



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        enemyCapsuleCollider = GetComponentInChildren<Collider>();
    }

    public void SetPatrol(PatrolIdentity startPoint, Dictionary<string, PatrolIdentity> patrolPointDictionary)
    {
        patrolPoints.Clear();
        if (startPoint == null) return;

        patrolPoints.Add(startPoint);
        PatrolIdentity currentPoint = startPoint;
        

        while (currentPoint.nextPoint != null)
        {
            currentPoint = patrolPointDictionary.TryGetValue(currentPoint.nextPoint, out PatrolIdentity next) ? next : null;
            patrolPoints.Add(currentPoint);
        }

    }

    public void Move()
    {

    }
}
