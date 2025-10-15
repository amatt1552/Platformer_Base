
using UnityEngine;
using UnityEngine.EventSystems;

public class SCh_Idle : CharacterBaseState
{
    public SCh_Idle() : base() { }
    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("Idle.");
        StateMachine.Idle();
        StateMachine.SetStoppingFriction(); 
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
        //SetupMovementXZ();
        StateMachine.RotateBody();
    }
    public override void CheckSwitchStates()
    {
        if (SwitchToMoving()) return;
    }

    
}
