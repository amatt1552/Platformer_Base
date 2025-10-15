using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerInput))]

public class AddForceTest : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public CapsuleCollider characterCollider;
    public Animator animator;
    private EnvironmentManager _environmentManager;
    private PlayerInput _input;
    private IKManager _ikManager;
    public Transform playerBottom;
    public Transform topColliderTest;

    public SO_Character characterInfo;


    [Header("Movement on X and Z")]
    public bool Moving { get; private set; }

    [Tooltip("Slows down player when moving left, right, and back.")]
    public bool strafeMode = true;

    // Speed and Direction XZ
    private bool movementEnabled;
    private bool slowdown;

    private float speed;
    private float targetSpeed;
    private float oldSpeed;
    private float currentAirMultiplier = 1f;
    const float DEFAULT_AIR_RESISTANCE = 0.2f;
    private float currentAcceleration;
    private float currentDeceleration;

    public Vector2 moveInputValue { get; private set; }
    private Vector3 targetMoveDirectionXZ;
    private Vector3 currentMoveValueXZ;
    private Vector3 oldMoveValueXZ;
    private Vector3 oldMoveDirection;

    // Environment Settings
    private float _envAccelMod = 1;
    private float _envDecelMod = 1;
    private float _envSpeedMod = 1;

    // Slopes
    private RaycastHit groundInfo;
    private bool onSlope;


    // Friction
    [Header("Friction Settings")]
    public bool slideOnRun;
    public bool disableMovementOnSlide;

    [Tooltip("Allows player's friction to change based on environment.")]
    public bool useEnvironment;

    private float movementFriction;
    private float environmentFriction;

    // Force
    private Vector3 _addForceDir;
    private Vector3 _startAddForceDir;
    private Vector3 _forceTime;

    // Camera and Rotation
    Vector2 lookValue;
    Vector3 bodyRotation;

    // Movement Y
    [Header("Movement on Y")]
    private int _jumpCount;
    private bool _jumpInCooldown;
    private float _jumpAcceleration;
    private const float TARGET_GROUND_DISTANCE = 0.2f;
    public float CurrentGroundDistance { get; private set; }
    private float gravAcceleration = 1;
    private float _moveValueY;
    private Vector3 targetMoveDirectionY;
    private Vector3 currentMoveDirectionY;
    private Vector3 yDirection;
    private Vector3 upDirection;

    public bool Grounded { get; private set; }
    public bool JumpPressed { get; private set; }
    public LayerMask isGround;

    // Wall running
    public bool WallRunPossible { get; private set; }
    private bool wallLeft;
    private bool wallRight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;

    private Vector3 wallJumpValue;
    private float wallRunSpeed;
    private float wallRunTimer;

    // Wall IK
    private bool wallIKEnabled = true;
    private bool wallLeftIK;
    private bool wallRightIK;
    //checks if wall is front of left shoulder.
    private bool wallFrontLeftIK;
    //checks if wall is front of right shoulder.
    private bool wallFrontRightIK;

    private RaycastHit leftWallhitIK;
    private RaycastHit rightWallhitIK;
    private RaycastHit frontLeftWallHitIK;
    private RaycastHit frontRightWallHitIK;

    [Header("Ledge Detection")]
    public float ledgeDetectionLength;
    public float ledgeSphereRadius;
    public LayerMask whatIsLedge;

    private Transform lastLedge;
    private Transform currLedge;
    private RaycastHit ledgeHit;
    RaycastHit topLedgeHit;

    [Header("Ledge Grabbing")]
    public float moveToLedgeSpeed;
    public float maxLedgeGrabDistance;
    public Vector3 targetLedgePosition { get; private set; }

    public float minTimeOnLedge;
    private float timeOnLedge;
    public bool OnLedge { get; private set; }
    public Vector3 LedgeJumpDirection { get; private set; }

    #region ------------------ Awake, Update, Etc ----------
    protected void Awake()
    {
        upDirection = characterInfo.upDirectionDefault;
        yDirection = upDirection;
        rb = rb != null ? rb : GetComponent<Rigidbody>();
        characterCollider = characterCollider != null ? characterCollider : GetComponent<CapsuleCollider>();
        _input = GetComponent<PlayerInput>();
        if (characterInfo == null)
        {
            Debug.LogError("Character has no character info! Disabling.");
            enabled = false;
            return;
        }

        movementEnabled = true;
        SetJogging();

    }

    private void OnEnable()
    {
        _input.jump.performed += JumpPressedListener;
        _input.sprint.performed += RunPressedListener;
        _input.sprint.canceled += RunReleasedListener;
        _input.walk.performed += ToggleWalkListener;
    }

    private void OnDisable()
    {
        _input.jump.performed -= JumpPressedListener;
        _input.sprint.performed -= RunPressedListener;
        _input.sprint.canceled -= RunReleasedListener;
        _input.walk.performed -= ToggleWalkListener;
    }
    protected void Update()
    {
        // Input
        Vector2 axis = _input.move.ReadValue<Vector2>();
        MoveXZ(axis);
        lookValue = _input.look.ReadValue<Vector2>();
        if (JumpPressed)
        {
            AddForce(5, Vector3.up);
            JumpPressed = false;
        }
        CalculateMoveDirection();
        AddForce(2, targetMoveDirectionXZ);

    }

    protected void FixedUpdate()
    {
        Grounded = GroundedCheck();
        //if (!Grounded)
            //AddForce(GameSettings.GRAVITY, Vector3.down);
        ApplyPhysics();
        UpdateForce();

    }
    private void ApplyPhysics()
    {
        rb.linearVelocity = (_addForceDir);

        rb.angularVelocity = bodyRotation;
    }
    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    #region ------------------ Listeners -------------------
    /// <summary>
    /// Takes input to move character on the X and Z
    /// </summary>
    /// <param name="axis"></param>
    public void MoveXZ(Vector2 axis)
    {
        if (!movementEnabled)
        {
            axis = Vector2.zero;
        }

        Moving = axis.magnitude > 0.1f;
        moveInputValue = axis.normalized;
    }

    public void SetWalking()
    {
        oldSpeed = speed;
        speed = characterInfo.walkSpeed;
    }
    public void ToggleWalkListener(InputAction.CallbackContext context)
    {
        if (speed <= characterInfo.walkSpeed)
        {
            SetJogging();
        }
        else
        {
            SetWalking();
        }
    }

    /// <summary>
    /// Sets to the normal speed.
    /// </summary>
    public void RunReleasedListener(InputAction.CallbackContext context)
    {
        SetJogging();
    }
    private void SetJogging()
    {
        oldSpeed = speed;
        speed = characterInfo.jogSpeed;
    }
    public void RunPressedListener(InputAction.CallbackContext context)
    {
        oldSpeed = speed;
        speed = characterInfo.runSpeed;
    }
    private void JumpPressedListener(InputAction.CallbackContext context)
    {
        if (!_jumpInCooldown)
        {
            CancelInvoke(nameof(JumpReleased));
            JumpPressed = true;
            Invoke(nameof(JumpReleased), characterInfo.jumpHoldTime);
        }
    }
    void JumpReleased()
    {
        JumpPressed = false;
    }

    float _currentEnvEnabled, _currentEnvDisabled;
    float _previousEnvEnabled, _previousEnvDisabled;
    int _currentEnvLayer;
    int _previousEnvLayer;
    float _envElapsedTime;
    void EnvironmentChangedListener(EnvironmentSettings currentEnv, EnvironmentSettings previousEnv)
    {
        rb.linearDamping = currentEnv.drag;
        _envAccelMod = currentEnv.accelerationModifier;
        _envDecelMod = currentEnv.decelerationModifier;
        _envSpeedMod = currentEnv.speedModifier;

        _currentEnvEnabled = currentEnv.enabledWeight;
        _currentEnvDisabled = currentEnv.disabledWeight;
        _previousEnvEnabled = previousEnv.enabledWeight;
        _previousEnvDisabled = previousEnv.disabledWeight;

        _currentEnvLayer = animator.GetLayerIndex(currentEnv.animationLayerName);
        _previousEnvLayer = animator.GetLayerIndex(previousEnv.animationLayerName);
        _envElapsedTime = 0;
    }
    void EnvironmentTransition()
    {
        _envElapsedTime += Time.deltaTime;
        float alpha = characterInfo.environmentTransitionTime != 0 ? Mathf.Clamp01(_envElapsedTime / characterInfo.environmentTransitionTime) : 1;
        float _previousEnvWeight = Mathf.Lerp(_previousEnvEnabled, _previousEnvDisabled, alpha);
        float _currentEnvWeight = Mathf.Lerp(_currentEnvDisabled, _currentEnvEnabled, alpha);

        animator.SetLayerWeight(_previousEnvLayer, _previousEnvWeight);
        animator.SetLayerWeight(_currentEnvLayer, _currentEnvWeight);
    }

    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    #region ------------------ Limits ----------------------
    public void SpeedControl()
    {
        //Vector3 limitedVelocity = rb.linearVelocity;
        //// I split up the speeds like this to better manage when to control the speed.
        //// 

        ////x
        //if (targetMoveDirection.x > 0.01)
        //{
        //    if (rb.linearVelocity.x >= (targetSpeed * targetMoveDirection.x))
        //    {
        //        limitedVelocity.x = targetSpeed * targetMoveDirection.x;
        //        //Debug.Log("velocityX too high!");
        //    }
        //}
        //else if (targetMoveDirection.x < -0.01)
        //{
        //    if (rb.linearVelocity.x <= (targetSpeed * targetMoveDirection.x))
        //    {
        //        limitedVelocity.x = targetSpeed * targetMoveDirection.x;
        //        //Debug.Log("velocityX too high!");
        //    }
        //}

        ////y
        //if (targetMoveDirection.y > 0.01)
        //{
        //    if (rb.linearVelocity.y >= (targetSpeed * targetMoveDirection.y))
        //    {
        //        limitedVelocity.y = targetSpeed * targetMoveDirection.y;
        //        //Debug.Log("velocityY too high!");
        //    }
        //}
        //else if (targetMoveDirection.y < -0.01)
        //{
        //    if (rb.linearVelocity.y <= (targetSpeed * targetMoveDirection.y))
        //    {
        //        limitedVelocity.y = targetSpeed * targetMoveDirection.y;
        //        //Debug.Log("velocityY too high!");
        //    }
        //}

        ////z
        //if (targetMoveDirection.z > 0.01)
        //{
        //    if (rb.linearVelocity.z >= (targetSpeed * targetMoveDirection.z))
        //    {
        //        limitedVelocity.z = targetSpeed * targetMoveDirection.z;
        //        //Debug.Log("velocityZ too high!");
        //    }
        //}
        //else if (targetMoveDirection.z < -0.01)
        //{
        //    if (rb.linearVelocity.z <= (targetSpeed * targetMoveDirection.z))
        //    {
        //        limitedVelocity.z = targetSpeed * targetMoveDirection.z;
        //        //Debug.Log("velocityZ too high!");
        //    }
        //}
        //rb.linearVelocity = limitedVelocity;
    }


    public void StartAirResistance(float airResistance = DEFAULT_AIR_RESISTANCE)
    {
        currentAirMultiplier = airResistance;
    }
    public void StopAirResistance()
    {
        currentAirMultiplier = 0;
    }

    public void DisablePhysics()
    {
        rb.isKinematic = true;
    }
    public void EnablePhysics()
    {
        rb.isKinematic = false;
    }
    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    #region ------------------ Force -----------------------

    /// <summary>
    /// Simulates using AddForce from physics
    /// </summary>
    /// <param name="force"></param>
    /// <param name="direction"></param>
    public void AddForce(float force, Vector3 direction)
    {
        force = Mathf.Clamp(force, 0.1f, Mathf.Infinity) / rb.mass;
        _addForceDir += direction * force;
        _startAddForceDir = _addForceDir;
    }

    private void UpdateForce()
    {
        //update xz value based on air resistance and ground friction
        float xzDeceleration = 2;
        xzDeceleration = Mathf.Clamp(xzDeceleration, 0.1f, Mathf.Infinity);

        //make force in x approach 0
        _addForceDir.x.ApproachValue(_startAddForceDir.x, xzDeceleration);
        //make force in z approach 0 
        _addForceDir.z.ApproachValue(_startAddForceDir.z, xzDeceleration);


        //update y value based on gravity
        float yDeceleration = GameSettings.GRAVITY;
        yDeceleration = Mathf.Clamp(yDeceleration, 0.1f, Mathf.Infinity);

        //Make force in y approach 0
        _addForceDir.y.ApproachValue(_startAddForceDir.y, yDeceleration);
    }

    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\


    #region ------------------ MovementXZ ------------------
    

    public void MoveTo(float speed, Vector3 targetPosition)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        float maxDistanceDelta = speed * Time.deltaTime * (distance / 2);
        rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, maxDistanceDelta));
    }

    public void SetRotation(Quaternion targetRotation)
    {
        rb.MoveRotation(targetRotation);
    }

    /// <summary>
    /// Gives a moveDirection that accounts for slopes.
    /// Sets onSlope value.
    /// </summary>
    /// <returns></returns>
    public void CalculateMoveDirection()
    {
        // Allows local movement instead of global.
        Vector3 moveX;
        Vector3 moveZ;

        moveX = moveInputValue.x * transform.right;
        moveX.y = 0;

        moveZ = moveInputValue.y * transform.forward;
        moveZ.y = 0;

        Vector3 combinedMove = moveX + moveZ;

        // Prevents target move direction from being 0.
        // Allows for keeping velocity after jumping or falling.
        targetMoveDirectionXZ = combinedMove;

    }


    /// <summary>
    /// Determines if on a slope within the angle constraints, updates the onSlope bool, and updates onSlopeEnter and onSlopeExit events
    /// </summary>
    /// <param name="direction"></param>
    /// <returns> 
    /// returns if on a slope. 
    /// I generally use the onSlope variable instead to prevent extra raycast checks. 
    /// </returns>
    private bool SlopeCheck(Vector3 direction)
    {
        bool slopeCheck = false;
        Vector3 origin = transform.position + (direction * (characterCollider.radius * characterInfo.slopeDistance));
        float end = (characterCollider.height * 0.5f) + (TARGET_GROUND_DISTANCE * 2);
        Debug.DrawLine(origin, origin + (Vector3.down * end), Color.red);
        RaycastHit hit;

        if (Physics.Raycast(origin, Vector3.down, out hit, end))
        {
            // Checks angle of ground.
            float angle = Vector3.Angle(Vector3.up, hit.normal);
            slopeCheck = angle < characterInfo.maxSlope /*&& angle != 0*/;
            //Invokes onSlopeEnter once after entering slope
        }

        if (!onSlope && slopeCheck)
        {
            //onSlopeEnter.Invoke();
            onSlope = true;
        }
        //Invokes onSlopeExit once after exiting slope
        else if (onSlope && !slopeCheck)
        {
            //onSlopeExit.Invoke();
            onSlope = false;
        }
        return slopeCheck;
    }

    /// <summary>
    /// Converts direction to account for slope.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public void SetSlopeDirection()
    {
        if (SlopeCheck(targetMoveDirectionXZ))
        {
            targetMoveDirectionXZ = Vector3.ProjectOnPlane(targetMoveDirectionXZ, groundInfo.normal);
        }
    }

    public bool MovingForward()
    {
        return moveInputValue.y > 0;
    }
    public void RotateBody()
    {
        bodyRotation = new(rb.angularVelocity.x, lookValue.x * characterInfo.rotationSpeed, rb.angularVelocity.z);
        //transform.rotation *= Quaternion.AngleAxis(lookValue.x * characterInfo.rotationSpeed, Vector3.up);
    }

    public void StopRotation()
    {
        bodyRotation = Vector3.zero;
    }

    public void SnapRotation(Vector3 forward)
    {
        transform.forward = forward;
    }

    public bool GroundedCheck()
    {
        LayerMask environmentLayer;
        float cylinderHeight = characterCollider.height - characterCollider.radius * 2;
        Vector3 pointA = characterCollider.center + (transform.up * cylinderHeight * 0.5f) + transform.position;
        Vector3 pointB = characterCollider.center - (transform.up * cylinderHeight * 0.5f) + transform.position;
        Debug.DrawLine(pointA, pointB);
        float radius = characterCollider.radius * 0.9f;
        if (Physics.CapsuleCast(pointA, pointB, radius, -transform.up, out groundInfo, 5, isGround))
        {
            CurrentGroundDistance = groundInfo.distance;
            if (groundInfo.distance < TARGET_GROUND_DISTANCE)
            {
                // Checks and updates what kind of ground you are on.
                environmentLayer = 1 << groundInfo.collider.gameObject.layer;
                _environmentManager.UpdateEnvironment(environmentLayer);
                return true;
            }
        }
        return false;

    }

    public void ApplyGround()
    {
        _moveValueY = 0;
        StopAirResistance();
    }
    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
}