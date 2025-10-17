using UnityEngine.InputSystem;
using UnityEngine;
using System.Runtime.InteropServices.WindowsRuntime;

public class SCh_WallJump : CharacterBaseState
{
    public SCh_WallJump() : base()
    {
        isRootState = true;
    }


    public override void EnterState()
    {
        base.EnterState();
        StateMachine.StartWallJump();
        StateMachine.DisableWallIK();
        Debug.Log($"Entered Wall Jump. is root: {isRootState}");
    }

    public override void ExitState()
    {
        StateMachine.animator.ResetTrigger("WallJump");
        StateMachine.EnableWallIK();
    }

    public override void InitializeSubState()
    {

    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
        StateMachine.Gravity();
    }

    public override void CheckSwitchStates()
    {
        base.CheckSwitchStates();

        if (SwitchToWallRunY()) return;
        if (SwitchToAirJump()) return;
        if (SwitchToGround()) return;
        //if (SwitchToFalling()) return;
    }
    
}
