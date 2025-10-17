
using UnityEngine;
using UnityEngine.EventSystems;

public class SCh_LedgeMovement : CharacterBaseState
{
    bool doneMoving;
    public SCh_LedgeMovement() { isRootState = true; }
    public override void EnterState()
    {
        Debug.Log($"Moving on Ledge. is root? {isRootState}");
        base.EnterState();
        StateMachine.LockLedgeTargetPosition();

        StateMachine.animator.SetTrigger("LedgeMove");
    }

    public override void ExitState()
    {
        StateMachine.animator.ResetTrigger("LedgeMove");
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
        doneMoving = StateMachine.MoveToLedge();
    }
    public override void CheckSwitchStates()
    {
        base.CheckSwitchStates();
        if(doneMoving) 
        {
            TrySwitchStates(StateFactory.GetState<SCh_LedgeIdle>(), StateMachine.defaultValues.ledgeMovePause);
            return;
        }
    }

    
}
