using UnityEngine.InputSystem;
using UnityEngine;
using System.Runtime.InteropServices.WindowsRuntime;

public class SCh_Jump : CharacterBaseState
{
    public SCh_Jump() : base()
    {
        isRootState = true;
    }


    public override void EnterState()
    {
        base.EnterState();
        StateMachine.ApplyJump();
        StateMachine.animator.SetTrigger("Jump");
    }

    public override void ExitState()
    {
        StateMachine.animator.ResetTrigger("Jump");
    }

    public override void InitializeSubState()
    {
        // Movement
        //SetSubState(StateFactory.GetState<CharacterIdleState>());
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
        if (SwitchToFalling()) return;
    }
    
}
