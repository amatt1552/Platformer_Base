using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.LightAnchor;
using static UnityEngine.Rendering.DebugUI;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnvironmentManager))]
[RequireComponent (typeof(CapsuleCollider))]
[RequireComponent (typeof(PlayerInput))]

public class CharacterStateMachine : BaseStateMachine
{
    [Header("References")]
    public Rigidbody rb;
    public CapsuleCollider characterCollider;
    public Animator animator;
    private EnvironmentManager _environmentManager;
    private PlayerInput _input;
    private IKManager _ikManager;
    public Transform playerBottom;
    public float PlayerHeight { get; private set; }
    public float PlayerWidth { get; private set; }
    public Transform topColliderTest;

    public SO_Character defaultValues;


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
    public Vector3 _addForceDir;
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
    private Vector3 _moveValueY;
    private Vector3 targetMoveDirectionY;
    private Vector3 currentMoveDirectionY;
    private Vector3 yDirection;

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
    public Vector3 targetLedgePosition {  get; private set; }
    public Vector3 LedgeJumpDirection { get; private set; }

    public float minTimeOnLedge;
    private float timeOnLedge;
    private bool _ledgeMovementLocked;
    public bool OnLedge { get; private set; }

    #region ------------------ Awake, Update, Etc ----------
    protected override void Awake()
    {
        yDirection = defaultValues.upDirectionDefault;
        rb = rb != null ? rb : GetComponent<Rigidbody>();
        characterCollider = characterCollider != null ? characterCollider : GetComponent<CapsuleCollider>();
        _environmentManager = GetComponent<EnvironmentManager>();
        _ikManager = GetComponent<IKManager>();
        _input = GetComponent<PlayerInput>();
        if (animator == null)
        {
            Debug.LogError("Character has no animator! Disabling.");
            enabled = false;
            return;
        }
        if (defaultValues == null)
        {
            Debug.LogError("Character has no character info! Disabling.");
            enabled = false;
            return;
        }

        movementEnabled = true;
        SetJogging();

        _stateFactory = new(this);
        currentState = (CharacterBaseState)_stateFactory.GetState<SCh_Ground>();
        currentState.EnterState();
    }

    private void OnEnable()
    {
        _input.jump.performed += JumpPressedListener;
        _input.sprint.performed += RunPressedListener;
        _input.sprint.canceled += RunReleasedListener;
        _input.walk.performed += ToggleWalkListener;
        _environmentManager.OnEnvironmentChanged += EnvironmentChangedListener;
        _stateFactory.onEnterState += EnterStateListener;
    }

    private void OnDisable() 
    {
        _input.jump.performed -= JumpPressedListener;
        _input.sprint.performed -= RunPressedListener;
        _input.sprint.canceled -= RunReleasedListener;
        _input.walk.performed -= ToggleWalkListener;
        _environmentManager.OnEnvironmentChanged -= EnvironmentChangedListener;
        _stateFactory.onEnterState -= EnterStateListener;
    }
    protected override void Update()
    {
        // Input
        Vector2 axis = _input.move.ReadValue<Vector2>();
        MoveXZ(axis);
        lookValue = _input.look.ReadValue<Vector2>();
        PlayerHeight = characterCollider.height;
        PlayerWidth = characterCollider.radius;

        EnvironmentTransition();
        base.Update();

    }

    protected override void FixedUpdate()
    {
        Grounded = GroundedCheck();
        WallCheck();
        base.FixedUpdate();
        ApplyPhysics();
        ApplyMovementAnimations();
        IKHandRaycast();
        ApplyIK();
    }
    private void ApplyPhysics() 
    {
        rb.linearVelocity = currentMoveValueXZ + _moveValueY;

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
        speed = defaultValues.walkSpeed;
    }
    public void ToggleWalkListener(InputAction.CallbackContext context)
    {
        if (speed <= defaultValues.walkSpeed)
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
    public void RunReleasedListener (InputAction.CallbackContext context)
    {
        SetJogging();
    }
    private void SetJogging()
    {
        oldSpeed = speed;
        speed = defaultValues.jogSpeed;
    }
    public void RunPressedListener(InputAction.CallbackContext context)
    {
        oldSpeed = speed;
        speed = defaultValues.runSpeed;
    }
    private void JumpPressedListener(InputAction.CallbackContext context)
    {
        if (!_jumpInCooldown)
        {
            CancelInvoke(nameof(JumpReleased));
            JumpPressed = true;
            Invoke(nameof(JumpReleased), defaultValues.jumpHoldTime);
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
        float alpha = defaultValues.environmentTransitionTime != 0 ? Mathf.Clamp01(_envElapsedTime / defaultValues.environmentTransitionTime) : 1;
        float _previousEnvWeight = Mathf.Lerp(_previousEnvEnabled, _previousEnvDisabled, alpha);
        float _currentEnvWeight = Mathf.Lerp(_currentEnvDisabled, _currentEnvEnabled, alpha);

        animator.SetLayerWeight(_previousEnvLayer, _previousEnvWeight);
        animator.SetLayerWeight(_currentEnvLayer, _currentEnvWeight);
    }

    protected override void EnterStateListener(BaseState state) 
    {
        // This is used to get the movement direction right on entering a state instead of updating regularly.
        // It will allow the blend tree for jump/fall direction to stay in the direction used at the start.
        animator.SetFloat("DirX_OnEnterState", moveInputValue.x);
        animator.SetFloat("DirY_OnEnterState", moveInputValue.y);
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

    public void ZeroMovement() 
    {
        currentMoveValueXZ = Vector3.zero;
        _moveValueY = Vector3.zero;
    }
    public bool ApproachingZeroXZ() 
    {
        Vector3 velocityXZ = rb.linearVelocity;
        velocityXZ.y = 0;
        return Vector3.Distance(velocityXZ, Vector3.zero) < 0.1f;
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

    #region ------------------ Friction --------------------
    public void SetMovingFriction()
    {
        characterCollider.material = defaultValues.movingFriction;
    }
    public void SetStoppingFriction(float frictionMagnitude = 1)
    {
        characterCollider.material = defaultValues.stoppingFriction;
    }
    public void SetSlidingFriction()
    {
        characterCollider.material = defaultValues.runSlideFriction;
    }
    //private void SetEnvironmentFriction(float frictionValue) 
    //{
    //    environmentFriction = frictionValue;
    //}

    //private float GetFriction() 
    //{
    //    return movementFriction + environmentFriction;
    //}
    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    #region ------------------ MovementXZ ------------------
    /// <summary>
    /// Checks for slope, strafe speed, and movement while in the air
    /// </summary>
    public void ApplyMovement()
    {
        float strafeMultiplier = 1f;
        if (strafeMode)
        {
            if (moveInputValue.y < 0)
            {
                strafeMultiplier = defaultValues.strafeMagnitude;
            }
        }
        //accounts for movement modifiers
        targetSpeed = speed * strafeMultiplier * _envSpeedMod;

        //checks for direction change
        if (Vector3.Distance(targetMoveDirectionXZ, oldMoveDirection) > 1f) 
        {
            //currentAcceleration = 0;
            oldMoveDirection = targetMoveDirectionXZ;
            //Debug.Log("Acceleration Reset.");
        }

        float targetAcceleration = defaultValues.accelerationSpeed;

        // Increases acceleration based on target speed value.
        // This was done to make the character seem a bit faster when sprinting starts.
        // I used the clamp to prevent this from slowing down or speeding up acceleration too much.
        targetAcceleration *= Mathf.Clamp(targetSpeed * 0.25f, 1, 1.50f);

        // accounts for the environment modifiers
        targetAcceleration *= _envAccelMod;

        // accounts for acceleration when in the air
        targetAcceleration -= targetAcceleration * currentAirMultiplier;

        currentAcceleration += 1 * Time.deltaTime;
        currentAcceleration = Mathf.Clamp01(currentAcceleration);
        currentDeceleration = currentAcceleration;

        float accelerationCurve = defaultValues.accelerationCurve.Evaluate(currentAcceleration) * targetAcceleration;
        //Debug.Log($"Acceleration curve value: {accelerationCurve}");

        //rb.AddForce(targetDirection.normalized * targetAcceleration, ForceMode.Force);
        oldMoveValueXZ = currentMoveValueXZ;
        currentMoveValueXZ += targetMoveDirectionXZ * accelerationCurve * Time.deltaTime;

        // Determines how to limit speed.
        if (speed < oldSpeed)
        {
            // Used when switching from faster to slower speed (eg: run to walk)
            currentMoveValueXZ = currentMoveValueXZ.magnitude <= targetSpeed ? targetMoveDirectionXZ * targetSpeed : currentMoveValueXZ;
            oldSpeed = speed;
        }
        else
        {
            // Used when switching from slower to faster speed (eg: walk to run)
            currentMoveValueXZ = currentMoveValueXZ.magnitude >= targetSpeed ? targetMoveDirectionXZ * targetSpeed : currentMoveValueXZ;
        }
    }

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

    public void Idle() 
    {
        currentMoveValueXZ = Vector3.zero;
    }

    public void SlowDown() 
    {
        float targetDeceleration = defaultValues.decelerationSpeed * _envDecelMod;

        targetDeceleration -= targetDeceleration * currentAirMultiplier;

        currentDeceleration -= targetDeceleration * Time.deltaTime;
        currentDeceleration = Mathf.Clamp01(currentDeceleration);
        currentAcceleration = currentDeceleration;

        float decelerationCurve = defaultValues.decelerationCurve.Evaluate(currentDeceleration);

        if (!ApproachingZeroXZ())
        {
            //float x = rb.linearVelocity.x / (2 * decelerationCurve);
            Vector3 tempVelocity = oldMoveValueXZ * decelerationCurve;
            currentMoveValueXZ = tempVelocity;
        }
        else
        {

            currentMoveValueXZ = Vector3.zero;
        }
        //rb.linearVelocity += yValue;
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
        targetMoveDirectionXZ = combinedMove.magnitude != 0 ? combinedMove : targetMoveDirectionXZ;
        
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
        Vector3 origin = transform.position + (direction * (characterCollider.radius * defaultValues.slopeDistance));
        float end = (characterCollider.height * 0.5f) + (TARGET_GROUND_DISTANCE * 2);
        Debug.DrawLine(origin, origin + (Vector3.down * end), Color.red);
        RaycastHit hit;

        if (Physics.Raycast(origin, Vector3.down, out hit, end))
        {
            // Checks angle of ground.
            float angle = Vector3.Angle(Vector3.up, hit.normal);
            slopeCheck = angle < defaultValues.maxSlope /*&& angle != 0*/;
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

    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    #region ------------------ Rotation --------------------

    public void RotateBody() 
    {
        bodyRotation = new(rb.angularVelocity.x, lookValue.x * defaultValues.rotationSpeed, rb.angularVelocity.z);
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

    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    #region ------------------ Wall Run --------------------
    public void WallCheck()
    {
        Vector3 positionMid = transform.position;
        Vector3 positionBottom = playerBottom.position;

        bool midRight, midLeft;
        bool bottomRight, bottomLeft;

        // Ray goes to the right of center position.
        midRight = Physics.Raycast(positionMid, transform.right, out rightWallhit, defaultValues.wallCheckDistance, defaultValues.wallMask);
        Debug.DrawLine(positionMid, positionMid + transform.right * defaultValues.wallCheckDistance);

        // Ray goes to the right of bottom position.
        bottomRight = Physics.Raycast(positionBottom, transform.right, defaultValues.wallCheckDistance, defaultValues.wallMask);
        Debug.DrawLine(positionBottom, positionBottom + transform.right * defaultValues.wallCheckDistance);
        
        // Ray goes to the left of center position.
        midLeft = Physics.Raycast(positionMid, -transform.right, out leftWallhit, defaultValues.wallCheckDistance, defaultValues.wallMask);
        Debug.DrawLine(positionMid, positionMid - transform.right * defaultValues.wallCheckDistance);
        
        // Ray goes to the right of bottom position.
        bottomLeft = Physics.Raycast(positionBottom, -transform.right, defaultValues.wallCheckDistance, defaultValues.wallMask);
        Debug.DrawLine(positionBottom, positionBottom - transform.right * defaultValues.wallCheckDistance);

        wallRight = midRight && bottomRight;
        wallLeft = midLeft && bottomLeft;

        WallRunPossible = (wallLeft || wallRight) && AboveGround();
    }

    public void StartWallRun() 
    {
        StopRotation();
        wallRunSpeed = targetSpeed;
        ZeroMovement();
        wallRunTimer = defaultValues.wallRunTime;

        float wallRunDir = wallRight ? 1 : 0;
        animator.SetFloat("WallRunDir", wallRunDir);
        animator.SetTrigger("WallRun");
    }

    public void StopWallRun() 
    {
        yDirection = defaultValues.upDirectionDefault;
    }

    private bool AboveGround() 
    {
        if (groundInfo.collider == null) 
        {
            return true;
        }
        return groundInfo.distance > defaultValues.minDistanceFromGround;
    }


    public void WallRunningMovementXZ()
    {

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        //transform.right = wallRight? -wallNormal : wallNormal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((transform.forward - wallForward).magnitude > (transform.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        currentMoveValueXZ = wallForward * wallRunSpeed;

    }


    public void WallRunningMovementY() 
    {
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 runDirection = Vector3.ProjectOnPlane(Vector3.up, wallNormal);
        wallRunTimer -= Time.deltaTime;
        _moveValueY = GameSettings.GRAVITY * defaultValues.WallRunCurveY.Evaluate(wallRunTimer) * runDirection;
    }

    //TODO: combine xz with y and place in _moveValueY.
    public void StartWallJump() 
    {
        float xzPercent = 1 - defaultValues.wallJumpDistribution;
        float yPercent = defaultValues.wallJumpDistribution;

        currentMoveValueXZ = wallRight ? rightWallhit.normal : leftWallhit.normal;
        currentMoveValueXZ *= xzPercent * defaultValues.wallJumpForce;
        oldMoveValueXZ = currentMoveValueXZ;

        _moveValueY = yPercent * defaultValues.wallJumpForce * yDirection;
        animator.SetTrigger("WallJump");
    }

    public void UpdateWallJump() 
    {
        //_moveValueY -= GameSettings.GRAVITY * defaultValues.gravMultiplier * Time.deltaTime;
    }

    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    #region ------------------ Ledges ----------------------
    public bool LedgeDetection(Vector3 offset) 
    {

        //Vector3 halfExtents = new Vector3(ledgeSphereRadius + extraRadius, 0.1f, ledgeSphereRadius + extraRadius);
        bool ledgeDetection = Physics.Raycast(transform.position + offset, transform.forward, out ledgeHit, maxLedgeGrabDistance, whatIsLedge);
        
        if (!ledgeDetection) return false;
        
        Debug.DrawLine(transform.position + offset, transform.position + offset + transform.forward, Color.blue);

        //Gets top of ledge for more consistent position on ledge
        if (!GetTop(ledgeHit, out topLedgeHit, whatIsLedge)) return false;

        //Makes Movement have to finish before moving again
        if (!_ledgeMovementLocked)
        {
            // adds normal * radius to offset target from ledge.
            targetLedgePosition = topLedgeHit.point + (ledgeHit.normal * characterCollider.radius);
            topColliderTest.position = targetLedgePosition;
        }

        float distanceToLedge = Vector3.Distance(transform.position, targetLedgePosition);

        if (distanceToLedge < maxLedgeGrabDistance && !OnLedge && ledgeHit.transform != lastLedge)
        {
            GrabLedge();
            lastLedge = ledgeHit.transform;
        }
        return true;
    }
    /// <summary>
    /// allows you to grab the same ledge.
    /// </summary>
    public void ResetLastLedge() 
    {
        lastLedge = null;
    }


    /// <summary>
    /// Gets top point of a collider found from raycastHit.
    /// </summary>
    bool GetTop(RaycastHit hit, out RaycastHit topHit, LayerMask layerMask) 
    {
        Collider col = hit.collider;
        Vector3 point = hit.point;
        Vector3 forward = -hit.normal;

        //I need the extents of the collider to set origin position of raycast pointing down.
        // this is set to full size in case point is at the bottom of the checked collider
        float sizeY = col.bounds.extents.y * 2;
        float forwardMagnitude = 0.05f;

        Vector3 origin = point;
        origin += Vector3.up * (sizeY + 1);
        origin += forward * forwardMagnitude;

        Vector3 direction = Vector3.down;
        Debug.DrawLine(origin, origin + direction, Color.green);
        return Physics.Raycast(origin, direction, out topHit, sizeY + 2, layerMask);

       
    }

    private void GrabLedge() 
    {
        currLedge = ledgeHit.transform;
        OnLedge = true;
        ZeroMovement();
        SnapRotation(-ledgeHit.normal);
    }
    
    /// <summary>
    /// Forces target position for ledge movement to stay the same until unlocked.
    /// </summary>
    public void LockLedgeTargetPosition() 
    { 
        _ledgeMovementLocked = true;
    }
    /// <summary>
    /// Allows target position to update.
    /// </summary>
    public void UnlockLedgeTargetPosition()
    {
        _ledgeMovementLocked = false;
    }

    public void MoveToLedge(float magnitude = 1) 
    {
        //Vector3 directionToLedge = topLedgeHit.point - transform.position;
        Vector3 targetPostion = topLedgeHit.point + (ledgeHit.normal * characterCollider.radius);
        float distanceToLedge = Vector3.Distance(transform.position, topLedgeHit.point);
        Debug.DrawRay(ledgeHit.point, ledgeHit.normal,Color.yellow);

        // Move to ledge
        if (distanceToLedge > 0.1f)
        {
            MoveTo(defaultValues.ledgeMoveSpeed * magnitude, targetPostion);
            SnapRotation(-ledgeHit.normal);
        }

        //Exit if moved away somehow
        if (distanceToLedge > maxLedgeGrabDistance) 
        {
            ExitLedge();
        }
    }

    public void StartLedgeJump() 
    {
        Vector3 jumpDirection = transform.up;
        if (moveInputValue.x != 0)
        {
            Vector3 inputDirection = transform.right * moveInputValue.x;
            float angle = defaultValues.ledgeJumpAngle;
            Quaternion rotation = Quaternion.AngleAxis(angle, transform.forward * moveInputValue.x);
            Vector3 rotatedDirection = rotation * inputDirection;
            jumpDirection = rotatedDirection ;
        }
        _moveValueY = jumpDirection * defaultValues.ledgeJumpForce;
    }


    public void ExitLedge() 
    {
        EnablePhysics();
        OnLedge = false;
        timeOnLedge = 0;
    }
    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    #region ------------------ MovementY -------------------
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
    
    public bool CanAirJump() 
    {
        return _jumpCount < defaultValues.airJumps;
    }

    public void ApplyJump(bool inAir = false)
    {
        // Trigger InAirVelocity change
        StartAirResistance();

        // Reset y velocity.
        _jumpAcceleration = 0;
        _moveValueY = defaultValues.jumpForce * yDirection;
        
        // Updates air jumps used.
        if (inAir)
            _jumpCount++;
        
        // Setups cooldowns for jump.
        Invoke(nameof(ResetJumpCooldown), defaultValues.jumpCooldown);
        _jumpInCooldown = true;
        JumpPressed = false;
    }

    public void ApplyJump(Vector3 direction, bool inAir = false) 
    {
        yDirection = direction;
        ApplyJump(inAir);
    }

    /// <summary>
    /// Deprecated
    /// </summary>
    public void UpdateJump() 
    {
        //_jumpAcceleration += 1 * Time.deltaTime;
        //_moveValueY -= defaultValues.jumpCurve.Evaluate(_jumpAcceleration)
        //    * rb.mass
        //    * GameSettings.GRAVITY 
        //    * defaultValues.gravMultiplier
        //    * Time.deltaTime;

        //gravAcceleration = _jumpAcceleration;
    }
    void ResetJumpCooldown() 
    {
        _jumpInCooldown = false;
    }
    public void ResetJump()
    {
        _jumpCount = 0;
    }

    public void StartFalling() 
    {
        // Trigger InAirVelocity change
        StartAirResistance();
    }

    
    public void Gravity()
    {

        //F = (G * m1 * m2) / d^2 might be useful for orbiting or something
        //Ray ray = new Ray(transform.position, Vector3.down);
        //RaycastHit hit;
        //float distance = 1;
        //if(controller.Raycast(ray, out hit, Mathf.Infinity)) 
        //{
        //    distance = hit.distance;
        //}
        gravAcceleration += 1 * Time.fixedDeltaTime;
        gravAcceleration = Mathf.Clamp01(gravAcceleration);
        float gravCurve = defaultValues.gravityCurve.Evaluate(gravAcceleration);
        _moveValueY -= GameSettings.GRAVITY
            * rb.mass
            * Time.deltaTime
            * defaultValues.gravMultiplier
            * gravCurve
            * yDirection;
        
        //if (currentState.Equals(typeof(CharacterGroundState)))
        //{
        //    _moveValueY = 0f;
        //}

    }

    public void ApplyGround() 
    {
        _moveValueY = Vector3.zero;
        StopAirResistance();
    }
    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    #region ------------------ States ----------------------
    #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    private void ApplyMovementAnimations()
    {

        float maxSpeed = defaultValues.runSpeed;
        float moveSpeed = moveInputValue.magnitude * speed / maxSpeed;
        float dirX = moveInputValue.x;
        float dirY = moveInputValue.y;

        animator.SetFloat("MoveSpeed", moveSpeed, defaultValues.movementAnimationDamping, Time.deltaTime);
        animator.SetFloat("DirX", dirX, defaultValues.movementAnimationDamping, Time.deltaTime);
        animator.SetFloat("DirY", dirY, defaultValues.movementAnimationDamping, Time.deltaTime);
        animator.SetFloat("RotationSpeed", lookValue.x, defaultValues.movementAnimationDamping, Time.deltaTime);

    }
    public void IKHandRaycast() 
    {
        Vector3 centerShoulderPosition = _ikManager.centerShoulder.position;
        Vector3 rightShoulderPosition = _ikManager.rightShoulder.position;
        Vector3 leftShoulderPosition = _ikManager.leftShoulder.position;
        Vector3 centerRight = _ikManager.centerShoulder.right;
        Vector3 rightForward = _ikManager.rightShoulder.forward;
        Vector3 leftForward = _ikManager.leftShoulder.forward;

        // Ray goes to the right of center shoulder position.
        wallRightIK = Physics.Raycast(centerShoulderPosition, centerRight, out rightWallhitIK, defaultValues.wallCheckDistance, defaultValues.wallMask);
        Debug.DrawLine(centerShoulderPosition, centerShoulderPosition + centerRight * defaultValues.wallCheckDistance);

        // Ray goes to the left of center shoulder position.
        wallLeftIK = Physics.Raycast(centerShoulderPosition, -centerRight, out leftWallhitIK, defaultValues.wallCheckDistance, defaultValues.wallMask);
        Debug.DrawLine(centerShoulderPosition, centerShoulderPosition + -transform.right * defaultValues.wallCheckDistance);

        // Ray goes in front of right shoulder position
        wallFrontRightIK = Physics.Raycast(rightShoulderPosition, rightForward, out frontRightWallHitIK, defaultValues.wallCheckDistance, defaultValues.wallMask);
        Debug.DrawLine(rightShoulderPosition, rightShoulderPosition + rightForward * defaultValues.wallCheckDistance);

        // Ray goes in front of left shoulder position
        wallFrontLeftIK = Physics.Raycast(leftShoulderPosition, leftForward, out frontLeftWallHitIK, defaultValues.wallCheckDistance, defaultValues.wallMask);
        Debug.DrawLine(leftShoulderPosition, leftShoulderPosition + leftForward * defaultValues.wallCheckDistance);

    }

    public void EnableWallIK() 
    {
        wallIKEnabled = true;
    }
    public void DisableWallIK()
    {
        wallIKEnabled = false;
    }

    //TODO: Refactor function
    private void ApplyIK() 
    {
         
        float handOffsetPercent = .05f;
        
        // Handles left hand.
        float leftHandWeight = 0;
        Vector3 leftHandPosition = Vector3.zero;
        Vector3 leftHandNormal = Vector3.zero;
        if (wallIKEnabled)
        {
            if (frontLeftWallHitIK.transform != null)
            {
                leftHandPosition = frontLeftWallHitIK.point + (frontLeftWallHitIK.normal * handOffsetPercent);
                leftHandNormal = frontLeftWallHitIK.normal;
                leftHandWeight = 1;
            }
            if (leftWallhitIK.transform != null)
            {
                leftHandPosition = leftWallhitIK.point + (leftWallhitIK.normal * handOffsetPercent);
                leftHandNormal = leftWallhitIK.normal;
                leftHandWeight = 1;
            }
        }

        // Handles right hand.
        float rightHandWeight = 0;
        Vector3 rightHandPosition = Vector3.zero;
        Vector3 rightHandNormal = Vector3.zero;
        if (wallIKEnabled)
        {
            if (frontRightWallHitIK.transform != null)
            {
                rightHandPosition = frontRightWallHitIK.point + (frontRightWallHitIK.normal * handOffsetPercent);
                rightHandNormal = frontRightWallHitIK.normal;
                rightHandWeight = 1;
            }
            if (rightWallhitIK.transform != null)
            {
                rightHandPosition = rightWallhitIK.point + (rightWallhitIK.normal * handOffsetPercent);
                rightHandNormal = rightWallhitIK.normal;
                rightHandWeight = 1;
            }
        }
        _ikManager.LeftHandTarget(leftHandPosition, leftHandNormal, leftHandWeight);
        _ikManager.RightHandTarget(rightHandPosition, rightHandNormal, rightHandWeight);
    }
}
