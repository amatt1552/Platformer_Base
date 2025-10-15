
using UnityEngine;
using UnityEngine.EventSystems;

public class SCh_WallRunY : CharacterBaseState
{
    public SCh_WallRunY()
    { 
        isRootState = true;
    }
    public override void EnterState()
    {
        base.EnterState();
        StateMachine.StartWallRun();
        Debug.Log($"Wall Running Y. Is root: {isRootState}");
    }

    public override void ExitState()
    {
        StateMachine.StopWallRun();
        StateMachine.animator.ResetTrigger("WallRun");
    }

    public override void InitializeSubState()
    {

    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
        StateMachine.WallRunningMovementY();
    }
    public override void CheckSwitchStates()
    {
        base.CheckSwitchStates();

        if (SwitchToWallJump()) return;

        if (SwitchToGround()) return;

        if (!StateMachine.WallRunPossible)
        {
            ForceSwitchToFalling();
            return;
        }
    }

    
}
