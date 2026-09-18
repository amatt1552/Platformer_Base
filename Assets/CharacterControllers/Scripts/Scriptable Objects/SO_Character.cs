using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotate with mouse rotates character based on the x of mouse / right stick movement.<br/>
/// Rotate with move direction rotates character based on the direction the character tries to move.<br/>
/// Rotate with movement rotates character when pressing left and right inputs
/// </summary>
public enum RotationStyle
{
    RotateWithMouse,
    RotateWithMoveDirection,
    RotateWithMovement
}


[CreateAssetMenu(fileName = "SO_Character", menuName = "Scriptable Objects/SO_Character")]

/// <summary>
/// This script's purpose is to give the base values for a type of character.
/// Could make a character that at its base is slower or a really fast character for example.
/// Anything modifying your base values should not be used here. (powerups, boosts, etc)
/// </summary>

public class SO_Character : ScriptableObject
{
    //I want them to only be able modify the variables in the editor so all are serialized
    #region ----------------- Editor Values ----------------
    [Header("MovementXZ")]
    [SerializeField]
    private float _walkSpeed = 2;
    [SerializeField]
    private float _jogSpeed = 4;
    [SerializeField]
    private float _runSpeed = 8;
    [Tooltip("Speed multiplier while in the air.")]
    [SerializeField]
    private float _airMultiplier = 0.5f;
    [Tooltip("What speed will be multiplied by while strafing")]
    [SerializeField]
    private float _strafeMagnitude = 0.5f;
    [SerializeField]
    private RotationStyle _rotationStyle = RotationStyle.RotateWithMouse;
    [Tooltip("Speed used with rotation styles rotate with mouse and rotate with movement")]
    [SerializeField]
    private float _rotationSpeed = 1;

    [Tooltip("Speed used with rotation style rotate with move direction")]
    [SerializeField]
    private float _directionalRotationSpeed = 15;


    [Header("Slopes")]
    [SerializeField]
    private float _maxSlope = 45f;
    [SerializeField]
    private float _slopeGravity = 10f;
    [SerializeField]
    private float _slopeDistance = 0.3f;


    [Header("Acceleration and Deceleration")]
    [SerializeField]
    private AnimationCurve _accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField]
    private AnimationCurve _decelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField]
    private float _accelerationSpeed = 4;
    [SerializeField]
    private float _decelerationSpeed = 4;
    [SerializeField]
    private PhysicsMaterial _movingFriction;
    [SerializeField]
    private PhysicsMaterial _stoppingFriction;
    [SerializeField]
    private PhysicsMaterial _runSlideFriction;
    [SerializeField]
    private float _maxSlideTime = 2;


    [Header("Jump")]
    [SerializeField] private float _jumpForce = 5;
    private int _airJumps = 1;

    [Tooltip("Allows for jumps to chain for new effects like in Mario")]
    [SerializeField]
    private bool _useJumpCombo = false;
    [Tooltip("How long you can be grounded before jump combo ends.")]
    [SerializeField]
    private float _jumpComboTime = 0.1f;
    [SerializeField]
    private JumpComboSetting[] _jumpComboSettings = new JumpComboSetting[] { 
        new JumpComboSetting(0, 1),
        new JumpComboSetting(2, 1.25f), 
        new JumpComboSetting(4, 1.5f) };
    private AnimationCurve _jumpCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField]
    private float _maxJumpHeight = 4f;
    [SerializeField]
    private float _maxJumpTime = 0.25f;

    [Tooltip("How long it takes to be able to execjump again.")]
    [SerializeField]
    private float _jumpCooldown = 0.5f;
    [Tooltip("Allows pressing jump to be registered before landing.")]
    [SerializeField]
    private float _jumpHoldTime = 0.1f;
    [Tooltip("Allows ground jump for set time before falling.")]
    [SerializeField]
    private float _coyoteTime = 0.1f;


    [Header("Gravity")]
    [SerializeField]
    private float _gravMultiplier = 2f;
    [SerializeField]
    private Vector3 _upDirectionDefault = Vector3.up;
    [SerializeField]
    private AnimationCurve _gravityCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField]
    private float _fallGravityMagnitude = 2;
    [SerializeField]
    private float _jumpGravityMagnitude = 1;


    [Header("Animation Settings")]
    [SerializeField]
    private float _movementAnimationDamping = 0.2f;
    [SerializeField]
    private float _environmentTransitionTime = 0.2f;


    [Header("Wall Running")]
    [SerializeField]
    private LayerMask _wallMask = ~0;
    [SerializeField]
    private float _wallRunTime = 2;
    [SerializeField]
    private float _wallRunMaxSpeed = 4;
    [SerializeField]
    private float _wallJumpForce = 5;
    [Tooltip("Determines how much force is applied on up and normal direction")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _wallJumpDistribution = 0.75f;
    [SerializeField]
    private float _minDistanceFromGround = 2f;
    [SerializeField]
    private float _wallCheckDistance = 0.5f;
    [SerializeField]
    private AnimationCurve _WallRunCurveY = AnimationCurve.Linear(0, 0, 1, 1);


    [Header("Ledge Settings")]
    [SerializeField]
    private float _ledgeMoveSpeed = 1;
    [SerializeField]
    private float _ledgeJumpAngle = 45f;
    [SerializeField]
    private float _ledgeJumpForce = 8f;
    [SerializeField]
    private float _ledgeGrabPause = 1f;
    [SerializeField]
    private float _ledgeMovePause = 0.5f;
    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\


    #region ----------------- Public Getters ---------------
    //Public Lambda Getters
    //"MovementXZ
    public float walkSpeed => _walkSpeed;
    public float jogSpeed => _jogSpeed;
    public float runSpeed => _runSpeed;
    [Tooltip("Speed multiplier while in the air.")]
    public float airMultiplier => _airMultiplier;
    [Tooltip("What speed will be multiplied by while strafing")]
    public float strafeMagnitude => _strafeMagnitude;
    public RotationStyle rotationStyle => _rotationStyle;
    //[Tooltip("Speed used with rotation styles rotate with mouse and rotate with movement")]
    //public float rotationSpeed => _rotationSpeed;

    //[Tooltip("Speed used with rotation style rotate with move direction")]
    //public float directionalRotationSpeed => _directionalRotationSpeed;

    //Slopes
    public float maxSlope => _maxSlope;
    public float slopeGravity => _slopeGravity;
    public float slopeDistance => _slopeDistance;

    //Acceleration and Deceleration
    public AnimationCurve accelerationCurve => _accelerationCurve;
    public AnimationCurve decelerationCurve => _decelerationCurve;
    public float accelerationSpeed => _accelerationSpeed;
    public float decelerationSpeed => _decelerationSpeed;
    public PhysicsMaterial movingFriction => _movingFriction;
    public PhysicsMaterial stoppingFriction => _stoppingFriction;
    public PhysicsMaterial runSlideFriction => _runSlideFriction;
    public float maxSlideTime => _maxSlideTime;

    //Jump
    public float jumpForce => _jumpForce;
    public int airJumps => _airJumps;

    [Tooltip("Allows for jumps to chain for new effects like in Mario")]
    public bool useJumpCombo => _useJumpCombo;
    [Tooltip("How long you can be grounded before jump combo ends.")]
    public float jumpComboTime => _jumpComboTime;
    public  JumpComboSetting[] jumpComboSettings => _jumpComboSettings;
    

    public AnimationCurve jumpCurve => _jumpCurve;
    public float maxJumpHeight => _maxJumpHeight;
    public float maxJumpTime => _maxJumpTime;

    [Tooltip("How long it takes to execute jump again.")]
    public float jumpCooldown => _jumpCooldown;
    [Tooltip("Allows pressing jump to be registered before landing.")]
    public float jumpHoldTime => _jumpHoldTime;
    [Tooltip("Allows ground jump for set time before falling.")]
    public float coyoteTime => _coyoteTime;

    //Gravity
    public float gravMultiplier => _gravMultiplier;
    public Vector3 upDirectionDefault => _upDirectionDefault;
    public AnimationCurve gravityCurve => _gravityCurve;
    public float fallGravityMagnitude => _fallGravityMagnitude;
    public float jumpGravityMagnitude => _jumpGravityMagnitude;
    
    //Animation Settings
    public float movementAnimationDamping => _movementAnimationDamping;
    public float environmentTransitionTime => _environmentTransitionTime;

    //Wall Running
    public LayerMask wallMask => _wallMask;
    public float wallRunTime => _wallRunTime;
    public float wallRunMaxSpeed => _wallRunMaxSpeed;
    public float wallJumpForce => _wallJumpForce;
    [Tooltip("Determines how much force is applied on up and normal direction")]
    [Range(0f, 1f)]
    public float wallJumpDistribution => _wallJumpDistribution;
    public float minDistanceFromGround => _minDistanceFromGround;
    public float wallCheckDistance => _wallCheckDistance;
    public AnimationCurve WallRunCurveY => _WallRunCurveY;

    //Ledge Settings
    public float ledgeMoveSpeed => _ledgeMoveSpeed;
    public float ledgeJumpAngle => _ledgeJumpAngle;
    public float ledgeJumpForce => _ledgeJumpForce;
    public float ledgeGrabPause => _ledgeGrabPause;
    public float ledgeMovePause => _ledgeMovePause;
    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\


    /// <summary>
    /// Returns rotation speed based on rotation style used.
    /// </summary>
    /// <returns></returns>
    public float GetRotationalSpeed() 
    {
        if(rotationStyle == RotationStyle.RotateWithMoveDirection) 
        {
            return _directionalRotationSpeed;
        }
        return _rotationSpeed;
    }
    public void JumpComboInit(JumpComboSetting[] settings) 
    {
        if (_jumpComboSettings.Length == 0)
        {
            _jumpComboSettings = settings;
        }
    }
}
[Serializable]
public class JumpComboSetting
{
    
    [Tooltip("Should be used to add to jump height")]
    public float jumpHeightModifier;
    [Tooltip("Should be used to add to jump height")]
    public float jumpTimeModifier;

    public JumpComboSetting(float jumpHeightModifier, float jumpTimeModifier)
    {
        this.jumpHeightModifier = jumpHeightModifier;
        this.jumpTimeModifier = jumpTimeModifier;
    }
}