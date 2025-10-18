using UnityEngine.InputSystem;
using UnityEngine;
using System.Runtime.InteropServices.WindowsRuntime;

public class SCh_AirJump : CharacterBaseState
{
    public SCh_AirJump() : base()
    {
        isRootState = true;
    }

    public override void EnterState()
    {
        base.EnterState();
        StateMachine.ApplyJump(true);
        StateMachine.animator.SetTrigger("AirJump");
    }

    public override void ExitState()
    {
        StateMachine.animator.ResetTrigger("AirJump");
    }

    public override void InitializeSubState()
    {
        // Moving
    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
        StateMachine.Gravity();
        StateMachine.LedgeDetection(GetLedgeGrabOffset(), Vector3.zero);
    }
    public override void CheckSwitchStates()
    {
        base.CheckSwitchStates();

        if (SwitchToLedgeGrab()) return;
        if (SwitchToAirJump()) return;
        if (SwitchToGround()) return;
        if (SwitchToFalling()) return;
    }
    
}
