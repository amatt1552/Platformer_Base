
using UnityEngine;
using UnityEngine.EventSystems;

public class SCh_LedgeMovement : CharacterBaseState
{
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
        StateMachine.MoveToLedge();
    }
    public override void CheckSwitchStates()
    {
        base.CheckSwitchStates();
        Vector3 ledgePos = StateMachine.targetLedgePosition;
        ledgePos.y = 0;
        Vector3 characterPos = StateMachine.transform.position;
        characterPos.y = 0;

        float distance = Vector3.Distance(ledgePos, characterPos);
        if(distance < 0.2f) 
        {
            TrySwitchStates(StateFactory.GetState<SCh_LedgeIdle>(), StateMachine.defaultValues.ledgeMovePause);
            return;
        }
    }

    
}
