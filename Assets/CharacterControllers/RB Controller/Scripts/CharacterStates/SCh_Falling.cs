using UnityEngine;
using UnityEngine.InputSystem;

public class SCh_Falling : CharacterBaseState
{
    public SCh_Falling() : base()
    {
        isRootState = true;
    }


    public override void EnterState()
    {
        base.EnterState();
        StateMachine.animator.SetTrigger("Falling");

        Debug.Log("Falling.");
    }

    public override void ExitState()
    {
        StateMachine.animator.ResetTrigger("Falling");

    }

    public override void InitializeSubState()
    {
        // Add states allowed while falling.

        // Movement
        //SetSubState(StateFactory.GetState<CharacterIdleState>());
        // Sliding
        // Emotes
        // Crouching
    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
        StateMachine.Gravity();
        StateMachine.LedgeDetection(GetLedgeGrabOffset());
    }
    public override void CheckSwitchStates()
    {
        base.CheckSwitchStates();

        if (SwitchToLedgeGrab()) return;
        if (SwitchToWallRunY()) return;
        if (SwitchToAirJump()) return;
        if (SwitchToGround()) return;
    }

}
