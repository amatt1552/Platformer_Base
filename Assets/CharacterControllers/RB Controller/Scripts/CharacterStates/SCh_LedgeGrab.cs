
using UnityEngine;
using UnityEngine.EventSystems;

public class SCh_LedgeGrab : CharacterBaseState
{
    const float _grabSpeed = 4;
    public SCh_LedgeGrab() 
    {
        isRootState = true;
    }
    public override void EnterState()
    {
        Debug.Log($"LedgeGrabbed. is root? {isRootState}");
        base.EnterState(); 
        StateMachine.animator.SetTrigger("LedgeGrab");
        StateMachine.DisableWallIK();
        StateMachine.DisablePhysics();
        SetSubState(StateFactory.GetState<SCh_MoveDisabled>());
    }

    public override void ExitState()
    {

        StateMachine.animator.ResetTrigger("LedgeGrab");
    }

    public override void InitializeSubState()
    {
        
    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
        StateMachine.MoveToLedge(_grabSpeed);
    }
    public override void CheckSwitchStates()
    {
        base.CheckSwitchStates();
        if(TrySwitchStates(StateFactory.GetState<SCh_LedgeIdle>())) return;
    }

    
}
