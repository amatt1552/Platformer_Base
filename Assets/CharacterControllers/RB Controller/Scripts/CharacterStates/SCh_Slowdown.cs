using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SCh_Slowdown : CharacterBaseState
{
    public SCh_Slowdown() : base() { }

    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("SlowingDown.");
        StateMachine.SetStoppingFriction();
    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {
        // Add states allowed while Moving.
        // Interactions?
        // Shooting?
    }

    public override void FixedUpdateState()
    {
        StateMachine.SlowDown();
        StateMachine.RotateBody();
    }
    public override void CheckSwitchStates()
    {

        if (StateMachine.Moving)
        {
            TrySwitchStates(StateFactory.GetState<SCh_Movement>());
        }
        if (StateMachine.ApproachingZeroXZ())
        {
            TrySwitchStates(StateFactory.GetState<SCh_Idle>());
        }

    }
    
}
