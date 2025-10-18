
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public abstract class CharacterBaseState : BaseState
{
    protected CharacterStateMachine StateMachine { get; private set; }

    public CharacterBaseState() : base() { }

    public override void Initialization(BaseStateMachine stateMachine, StateFactory characterStateFactory)
    {
        base.Initialization(stateMachine, characterStateFactory);
        StateMachine = StateMachine == null ? (CharacterStateMachine) stateMachine : StateMachine;
    }

    #region ------------------ Common Functions ----------------
    /// <summary>
    /// value used to add time to transition to falling state
    /// </summary>
    protected const float fallingTimerValue = 0.8f;

    /// <summary>
    /// Value used to offset initial ledge detection down while on a ledge.
    /// </summary>
    protected Vector3 ledgeDetectionMovementOffset = Vector3.down * 0.05f;

    // Functions used to prevent rewriting commonly used code for my states.

    /// <summary>
    /// Handles starting all the ground movement defined in Character State Machine.
    /// </summary>
    protected void SetupMovementXZ()
    {
        StateMachine.CalculateMoveDirection();
        if (CurrentSuperState.Equals(typeof(SCh_Ground)))
            StateMachine.SetSlopeDirection();
        StateMachine.ApplyMovement();
        StateMachine.RotateBody();
    }

    /// <summary>
    /// Disables movement. Also works if called in root state.
    /// </summary>
    protected void DisableMovement() 
    {
        if (isRootState)
            SetSubState(StateFactory.GetState<SCh_MoveDisabled>());
        else
            TrySwitchStates(StateFactory.GetState<SCh_MoveDisabled>());
    }

    protected Vector3 GetLedgeGrabOffset() 
    {
        return StateMachine.transform.up * (StateMachine.PlayerHeight * 0.5f - StateMachine.PlayerWidth);
    }

    /// <summary>
    /// Switches to Move state if moving. Ignores state cooldown.
    /// </summary>
    /// <returns></returns>
    protected bool ForceSwitchToMoving() 
    {
        if (StateMachine.Moving)
        {
            ForceSwitchStates(StateFactory.GetState<SCh_Movement>());
            return true;
        }
        return false;
    }
    /// <summary>
    /// Switches to the Move state if moving.
    /// </summary>
    /// <returns></returns>
    protected bool SwitchToMoving() 
    {
        bool switchStatus = false;
        if (StateMachine.Moving)
        {
            switchStatus = TrySwitchStates(StateFactory.GetState<SCh_Movement>());
        }
        return switchStatus;
    }

    /// <summary>
    /// Switches to Jump State if jump pressed.
    /// </summary>
    /// <returns> returns true if switch was successful. </returns>
    protected bool SwitchToJump() 
    {
        bool switchStatus = false;
        if (StateMachine.JumpPressed)
        {
            Debug.Log("Jump Pressed!");
            switchStatus = TrySwitchStates(StateFactory.GetState<SCh_Jump>(), 0.1f);
        }
        return switchStatus;
    }

    /// <summary>
    /// Switches to Air Jump State if max air jumps is not exceeded.
    /// </summary>
    /// <returns> returns true if switch was successful. </returns>
    protected bool SwitchToAirJump()
    {
        bool switchStatus = false;
        if (StateMachine.JumpPressed && StateMachine.CanAirJump())
        {
            Debug.Log("Air Jump Pressed!");
            switchStatus = TrySwitchStates(StateFactory.GetState<SCh_AirJump>(), 0.1f);
        }
        return switchStatus;
    }

    /// <summary>
    /// Switches to Falling State if velocity is <= 0.
    /// </summary>
    /// <returns> returns true if switch was successful. </returns>
    protected bool SwitchToFalling()
    {
        bool switchStatus = false;
        if (StateMachine.rb.linearVelocity.y <= 0)
        {
            switchStatus = TrySwitchStates(StateFactory.GetState<SCh_Falling>());
        }
        return switchStatus;
    }

    /// <summary>
    /// Forces a switch to Falling State regardless of velocity.
    /// </summary>
    /// <returns> returns true if switch was successful. </returns>
    protected bool ForceSwitchToFalling()
    {
        return TrySwitchStates(StateFactory.GetState<SCh_Falling>());
    }

    /// <summary>
    /// Switches to Ground State if Ground is detected.
    /// </summary>
    /// <returns> returns true if switch was successful. </returns>
    protected bool SwitchToGround() 
    {
        bool switchStatus = false;
        if (StateMachine.Grounded)
        {
            switchStatus = TrySwitchStates(StateFactory.GetState<SCh_Ground>());
        }
        return switchStatus;
    }

    /// <summary>
    /// Used to switch movement to wall run XZ.
    /// Prevents movement while wall running.
    /// </summary>
    /// <returns> returns true if switch was successful. </returns>
    protected bool SwitchToWallRunXZ()
    {
        bool switchStatus = false;
        if (StateMachine.WallRunPossible && StateMachine.MovingForward())
        {
            switchStatus = TrySwitchStates(StateFactory.GetState<SCh_WallRunXZ>());
        }
        return switchStatus;
    }

    /// <summary>
    /// Used to switch movement in the y (gravity, jumping, etc) to wall run Y.
    /// It could have been done in XZ function but this helps prevent gravity and jumping being applied while wall running.
    /// </summary>
    /// <returns> returns true if switch was successful. </returns>
    protected bool SwitchToWallRunY()
    {
        bool switchStatus = false;
        if (StateMachine.WallRunPossible && StateMachine.MovingForward())
        {
            switchStatus = TrySwitchStates(StateFactory.GetState<SCh_WallRunY>());
        }
        return switchStatus;
    }
    /// <summary>
    /// Wall jumps if jump is pressed.
    /// </summary>
    /// <returns> returns true if switch was successful. </returns>
    protected bool SwitchToWallJump() 
    {
        bool switchStatus = false;
        // Prevents switch to air jump or wall run after switching to wall jump. 
        float cooldown = 0.3f;
        if (StateMachine.JumpPressed) 
        {
            switchStatus = TrySwitchStates(StateFactory.GetState<SCh_WallJump>(), cooldown);
        }
        return switchStatus;
    }

    /// <summary>
    /// Switches to wall grab if ledge is detected.
    /// </summary>
    /// <returns></returns>
    protected bool SwitchToLedgeGrab() 
    {
        bool switchStatus = false;
        if (StateMachine.OnLedge)
        {
            switchStatus = TrySwitchStates(StateFactory.GetState<SCh_LedgeGrab>(), StateMachine.defaultValues.ledgeGrabPause);
        }
        return switchStatus;
    }
    /// <summary>
    /// If ledge detected in move direction switches to ledge movement.
    /// </summary>
    /// <param name="wallCheckDistance"></param>
    /// <param name="extraRadius"></param>
    /// <returns></returns>
    protected bool SwitchToLedgeMovement(float wallCheckDistance = 1) 
    {
        bool switchStatus = false;
        Vector3 rightDirection = StateMachine.transform.right;
        Vector3 offset = Vector3.zero;//(Vector3.down * 0.1f) - StateMachine.transform.forward;
        float moveValue = StateMachine.moveInputValue.x;
        float magnitude = moveValue * wallCheckDistance;

        if (StateMachine.LedgeDetection(ledgeDetectionMovementOffset, rightDirection * magnitude + offset) && Mathf.Abs(moveValue) > 0.1f)
        {
            switchStatus = TrySwitchStates(StateFactory.GetState<SCh_LedgeMovement>());
        }
        return switchStatus;
    }

    /// <summary>
    /// Switches to ledge drop if down pressed
    /// </summary>
    /// <returns></returns>
    protected bool SwitchToLedgeDrop()
    {
        bool switchStatus = false;
        if (StateMachine.moveInputValue.y < 0)
        {
            switchStatus = TrySwitchStates(StateFactory.GetState<SCh_LedgeDrop>(), 0.5f);
        }
        return switchStatus;
    }

    /// <summary>
    /// Switches to ledge jump if jump pressed.
    /// </summary>
    /// <returns></returns>
    protected bool SwitchToLedgeJump() 
    {
        bool switchStatus = false;
        if (StateMachine.JumpPressed) 
        {
            switchStatus = TrySwitchStates(StateFactory.GetState<SCh_LedgeJump>(), 0.1f);
        }
        return switchStatus;
    }
    #endregion
}
