
using UnityEngine;
using UnityEngine.EventSystems;

public class SCh_LedgeIdle : CharacterBaseState
{
    public SCh_LedgeIdle() 
    {
        isRootState = true;
    }
    public override void EnterState()
    {
        Debug.Log($"Idle on Ledge. is root? {isRootState}");
        base.EnterState(); 
        StateMachine.DisableWallIK();
        StateMachine.UnlockLedgeTargetPosition();
        StateMachine.animator.SetTrigger("LedgeIdle");
    }

    public override void ExitState()
    {

        StateMachine.animator.ResetTrigger("LedgeIdle");
    }

    public override void InitializeSubState()
    {
        // Add states allowed while Idle.
        // Interactions?
        // Shooting?
    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
    }
    public override void CheckSwitchStates()
    {
        base.CheckSwitchStates();
        if (SwitchToLedgeJump()) return;
        if (SwitchToLedgeDrop()) return;
        if (SwitchToLedgeMovement()) return;
    }

    
}
