
using UnityEngine;
using UnityEngine.EventSystems;

public class SCh_LedgeJump : CharacterBaseState
{
    float fallingTimer;
    public SCh_LedgeJump()
    { 
        isRootState = true;
    }
    public override void EnterState()
    {
        base.EnterState();
        StateMachine.animator.SetTrigger("LedgeJump");
        StateMachine.EnablePhysics();
        StateMachine.StartLedgeJump();
        StateMachine.ExitLedge();
        StateMachine.ResetLastLedge();
        fallingTimer = fallingTimerValue;
    }

    public override void ExitState()
    {
        StateMachine.animator.ResetTrigger("LedgeJump");
        SetSubState(StateFactory.GetState<SCh_Idle>());
    }

    public override void InitializeSubState()
    {
        // Add states allowed while Idle.
        // Interactions?
        // Shooting?
    }
    public override void FixedUpdateState()
    {
        StateMachine.Gravity();
        StateMachine.LedgeDetection(GetLedgeGrabOffset(), Vector3.zero);
    }
    public override void CheckSwitchStates()
    {
        base.CheckSwitchStates();
        //used to prevent movement while jumping
        fallingTimer -= 1 * Time.deltaTime;
        if (fallingTimer <= 0 && SwitchToFalling()) return;
        if (SwitchToLedgeGrab()) return;
    }

    
}
