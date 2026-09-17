using UnityEngine;
using UnityEngine.InputSystem;

namespace CHController
{

    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(CharacterController))]
    public class CharacterStateMachine : BaseStateMachine
    {
        //movement
        private CharacterActions _actions;
        private Vector2 _lookAxis;
        private EnvironmentManager _environmentManager; //make environments scriptable objects
        public SO_Character defaultValues;
        public Animator animator;

        public bool isMoving;
        private bool _movementEnabled = true;
        public Vector2 MoveInputValue { get; private set; }
        public CharacterController controller;
        private Vector3 _currentMoveValueXZ;
        private Vector3 _currentMoveValueY;
        private float _moveSpeedXZ;

        //rotation
        private Vector3 _lookRotation;
        private Quaternion _targetRotation;

        //gravity
        Vector3 _gravityDirection = Vector3.down;
        float _gravity = 9.8f;
        float _groundedGravity = 1.0f;
        public float fallGravityMagnitude = 2;
        public float jumpGravityMagnitude = 1;
        public bool IsFalling { get; private set; }

        //jump
        float _initialJumpVelocity;
        public bool JumpPressed { get; private set; }
        public bool isJumping { get; private set; }
        private int _jumpCount;
        private bool _jumpInCooldown;

        protected override void Awake()
        {
            _actions = new CharacterActions();
            _actions.Enable();
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            _stateFactory = new(this);
            currentState = (CharacterBaseState)_stateFactory.GetState<SCh_Ground>();
            currentState.EnterState();
            SetupJumpVariables();
        }

        private void OnEnable()
        {
            _actions.Player.Jump.started += JumpPressedListener;
            _actions.Player.Jump.canceled += JumpReleasedListener;
            //_input.sprint.performed += RunPressedListener;
            //_input.sprint.canceled += RunReleasedListener;
            //input.walk.performed += ToggleWalkListener;
            //_environmentManager.OnEnvironmentChanged += EnvironmentChangedListener;
            _stateFactory.onEnterState += EnterStateListener;
        }

        private void OnDisable()
        {
            _actions.Player.Jump.started -= JumpPressedListener;
            _actions.Player.Jump.canceled -= JumpReleasedListener;
            //_input.sprint.performed -= RunPressedListener;
            //_input.sprint.canceled -= RunReleasedListener;
            //_input.walk.performed -= ToggleWalkListener;
            //_environmentManager.OnEnvironmentChanged -= EnvironmentChangedListener;
            _stateFactory.onEnterState -= EnterStateListener;
        }
        protected override void Update()
        {
            Vector2 axis = _actions.Player.Move.ReadValue<Vector2>();
            CreateMoveAxisMetaData(axis);
            base.Update();
            UpdateMovement();
            UpdateRotation();
            UpdateMovementAnimations();

        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        private void SetupJumpVariables()
        {
            float timeToApex = defaultValues.maxJumpTime / 2;
            _gravity = (2 * defaultValues.maxJumpHeight) / (timeToApex * timeToApex);
            _initialJumpVelocity = (2 * defaultValues.maxJumpHeight) / timeToApex;

        }

        protected void UpdateMovement()
        {
            _moveSpeedXZ = defaultValues.jogSpeed;
            Vector3 combinedMovement = (_currentMoveValueXZ * _moveSpeedXZ) + _currentMoveValueY;
            controller.Move(combinedMovement * Time.deltaTime);
        }

        protected void UpdateRotation()
        {

            transform.rotation = _targetRotation;
        }

        #region ------------------ Listeners -------------------
        /// <summary>
        /// Creates data used to observe the state of horizontal movement input. 
        /// </summary>
        /// <param name="axis"></param>
        public void CreateMoveAxisMetaData(Vector2 axis)
        {
            if (!_movementEnabled)
            {
                axis = Vector2.zero;
            }

            isMoving = axis.magnitude > 0.1f;
            MoveInputValue = axis.normalized;
        }
        private void JumpPressedListener(InputAction.CallbackContext context)
        {
            if (!_jumpInCooldown)
            {
                CancelInvoke(nameof(JumpCooldownComplete));
                JumpPressed = true;
                Invoke(nameof(JumpCooldownComplete), defaultValues.jumpHoldTime);
            }
        }
        void JumpReleasedListener(InputAction.CallbackContext context)
        {
            JumpPressed = false;
            isJumping = false;
            IsFalling = true;
        }
        #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

        #region ------------------ MovementXZ ------------------

        public void ApplyMovementXZ()
        {
            switch (defaultValues.rotationStyle)
            {
                case RotationStyle.RotateWithMouse:
                    Vector3 moveX = MoveInputValue.x * transform.right;
                    Vector3 moveZ = MoveInputValue.y * transform.forward;
                    _currentMoveValueXZ = moveX + moveZ;
                    break;
                case RotationStyle.RotateWithMoveDirection:
                    _currentMoveValueXZ = new(MoveInputValue.x, 0, MoveInputValue.y);
                    break;
                case RotationStyle.RotateWithMovement:
                    _currentMoveValueXZ = MoveInputValue.y * transform.forward;
                    break;

            }
        }

        #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\


        #region ------------------ MovementY -------------------

        public void HandleGravityGround() 
        {
            _currentMoveValueY = _gravityDirection * _groundedGravity;
        }
        public void HandleGravity(float gravityMultiplier = 1)
        {         
            //basically extracts the current vertical speed from the direction
            float verticalSpeed = Vector3.Dot(_currentMoveValueY, _gravityDirection);
            IsFalling = verticalSpeed > _groundedGravity;
            //+= apparently varies based on framerate so this tells the variable exactly where it should move per frame instead of using +=.

            Vector3 previousYVelocity = _currentMoveValueY;
            Vector3 newYVelocity = _currentMoveValueY + (_gravityDirection * _gravity * gravityMultiplier * Time.deltaTime);
            Vector3 nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
                
            _currentMoveValueY = nextYVelocity;
        }
        
        public void TryJump(bool inAir = false) 
        {
            
            isJumping = true;
            //this allows character to jump in the opposite direction of the gravity.
            _currentMoveValueY = -_gravityDirection * _initialJumpVelocity * 0.5f;
            if(inAir) 
            {
                _jumpCount++;
            }

            //stops player from spamming jump.
            Invoke(nameof(ResetJumpCooldown), defaultValues.jumpCooldown);
            _jumpInCooldown = true;
            JumpPressed = false;
            
        }
        public void JumpComplete() 
        {
            isJumping = false;
        }
        void ResetJumpCooldown()
        {
            _jumpInCooldown = false;
        }
        public void ResetJump()
        {
            _jumpCount = 0;
            //_currentMoveValueY = gravityDirection * (groundedGravity * Time.deltaTime);
        }
        public bool CanAirJump()
        {
            return _jumpCount < defaultValues.airJumps;
        }
        public void JumpCooldownComplete() 
        {
            JumpPressed = false;
        }
        #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\


        #region ------------------ Rotation --------------------
        public void ApplyRotation()
        {
            _lookAxis = _actions.Player.Look.ReadValue<Vector2>();
            switch (defaultValues.rotationStyle)
            {
                case RotationStyle.RotateWithMouse:
                    _targetRotation = transform.rotation * Quaternion.AngleAxis(_lookAxis.x * defaultValues.GetRotationalSpeed(), Vector3.up);
                    break;
                case RotationStyle.RotateWithMoveDirection:
                    Vector3 positionToLookAt;
                    positionToLookAt.x = MoveInputValue.x;
                    positionToLookAt.y = 0.0f;
                    positionToLookAt.z = MoveInputValue.y;

                    Quaternion currentRotation = transform.rotation;

                    if (isMoving)
                    {
                        Quaternion turnDirection = Quaternion.LookRotation(positionToLookAt);
                        _targetRotation = transform.rotation * Quaternion.Slerp(currentRotation, turnDirection, defaultValues.GetRotationalSpeed() * Time.deltaTime);
                    }
                    break;
                case RotationStyle.RotateWithMovement:
                    _targetRotation = Quaternion.AngleAxis(MoveInputValue.x * defaultValues.GetRotationalSpeed(), Vector3.up);
                    break;
            }
        }
        public void StopRotation() 
        {
            _lookAxis = Vector2.zero;
        }

        public void SnapRotation(Vector3 forward)
        {
            transform.forward = forward;
        }
        #endregion \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

        private void UpdateMovementAnimations()
        {
            //values for rotation style Rotate with Mouse
            float maxSpeed = defaultValues.runSpeed;
            float moveSpeed = MoveInputValue.magnitude * _moveSpeedXZ / maxSpeed;
            float dirX = MoveInputValue.x;
            float dirZ = MoveInputValue.y;
            //needed to compensate for including multiple types of character rotation.
            float lookAxisConversion = _lookAxis.x;
            switch (defaultValues.rotationStyle) 
            {
                case RotationStyle.RotateWithMoveDirection:
                    lookAxisConversion = 0;
                    dirX = 0;
                    dirZ = MoveInputValue.magnitude;
                    break;
                case RotationStyle.RotateWithMovement:
                    lookAxisConversion = MoveInputValue.x;
                    dirX = 0;
                    break;
            }

            animator.SetFloat("MoveSpeed", moveSpeed, defaultValues.movementAnimationDamping, Time.deltaTime);
            animator.SetFloat("DirX", dirX, defaultValues.movementAnimationDamping, Time.deltaTime);
            animator.SetFloat("DirZ", dirZ, defaultValues.movementAnimationDamping, Time.deltaTime);
            animator.SetFloat("RotationSpeed", lookAxisConversion, defaultValues.movementAnimationDamping, Time.deltaTime);

        }
    }
}
