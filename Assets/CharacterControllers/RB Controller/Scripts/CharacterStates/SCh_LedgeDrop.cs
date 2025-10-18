
using UnityEngine;
using UnityEngine.EventSystems;

public class SCh_LedgeDrop : CharacterBaseState
{
    float fallingTimer;
    public SCh_LedgeDrop() 
    {
        isRootState = true;
    }
    public override void EnterState()
    {
        Debug.Log($"Ledge Dropped. is root? {isRootState}");
        base.EnterState(); 
        StateMachine.animator.SetTrigger("LedgeDrop");
        StateMachine.LedgeDetection(Vector3.zero, Vector3.zero); //This is needed to allow to get the ledge it is currently on before dropping.
        StateMachine.EnablePhysics();
        StateMachine.ExitLedge();
        fallingTimer = fallingTimerValue;
    }

    public override void ExitState()
    {

        StateMachine.animator.ResetTrigger("LedgeDrop"); 
        SetSubState(StateFactory.GetState<SCh_Idle>());
    }

    public override void InitializeSubState()
    {
        
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
        fallingTimer -= 1 * Time.deltaTime;
        if (fallingTimer <= 0 && SwitchToFalling()) return;
        if (SwitchToGround()) return;
        if (SwitchToLedgeGrab()) return;
    }

    
}
