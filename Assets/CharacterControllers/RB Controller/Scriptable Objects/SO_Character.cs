using UnityEngine;

[CreateAssetMenu(fileName = "SO_Character", menuName = "Scriptable Objects/SO_Character")]

/// <summary>
/// This script's purpose is to give the base values for a type of character.
/// Could make a character that at its base is slower or a really fast character for example.
/// Anything modifying your base values should not be used here. (powerups, boosts, etc)
/// </summary>

public class SO_Character : ScriptableObject
{
    [Header("MovementXZ")]
    public float walkSpeed = 2;
    public float jogSpeed = 4;
    public float runSpeed = 8;
    [Tooltip("Speed multiplier while in the air.")]
    public float airMultiplier = 0.5f;
    [Tooltip("What speed will be multiplied by while strafing")]
    public float strafeMagnitude = 0.5f;
    public float rotationSpeed = 2;

    [Header("Slopes")]
    public float maxSlope = 45f;
    public float slopeGravity = 10f;
    public float slopeDistance = 0.3f;

    [Header("Acceleration and Deceleration")]
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve decelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float accelerationSpeed = 4;
    public float decelerationSpeed = 4;
    public PhysicsMaterial movingFriction;
    public PhysicsMaterial stoppingFriction;
    public PhysicsMaterial runSlideFriction;
    public float maxSlideTime = 2;

    [Header("Jump")]
    public float jumpForce = 5;
    public int airJumps = 1;
    public AnimationCurve jumpCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [Tooltip("How long it takes to jump again.")]
    public float jumpCooldown = 0.5f;
    [Tooltip("Allows pressing jump to be registered before landing.")]
    public float jumpHoldTime = 0.1f;
    [Tooltip("Allows ground jump for set time before falling.")]
    public float coyoteTime = 0.1f;

    [Header("Gravity")]
    public float gravMultiplier = 2f;
    public Vector3 upDirectionDefault = Vector3.up;
    public AnimationCurve gravityCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Animation Settings")]
    public float movementAnimationDamping = 0.2f;
    public float environmentTransitionTime = 0.2f;

    [Header("Wall Running")]
    public LayerMask wallMask = ~0;
    public float wallRunTime = 2;
    public float wallRunMaxSpeed = 4;
    public float wallJumpForce = 5;
    [Tooltip("Determines how much force is applied on up and normal direction")]
    [Range(0f, 1f)]
    public float wallJumpDistribution = 0.75f;
    public float minDistanceFromGround = 2f;
    public float wallCheckDistance = 0.5f;
    public AnimationCurve WallRunCurveY = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Ledge Settings")]
    public float ledgeMoveSpeed = 1;
    public float ledgeJumpAngle = 45f;
    public float ledgeJumpForce = 8f;
    public float ledgeGrabPause = 1f;
    public float ledgeMovePause = 0.5f;

}
