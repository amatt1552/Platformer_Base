using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SCh_Movement : CharacterBaseState
{
    public SCh_Movement() : base() { }

    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("Moving.");
        StateMachine.SetMovingFriction();
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
        SetupMovementXZ();
    }
    public override void CheckSwitchStates()
    {
        if (SwitchToWallRunXZ()) return;
        
        if (!StateMachine.Moving && CurrentSuperState.Equals(typeof(SCh_Ground)))
        {
            bool runningCheck = StateMachine.rb.linearVelocity.sqrMagnitude > Mathf.Pow(StateMachine.defaultValues.jogSpeed, 2) + 1;
            if (runningCheck && StateMachine.slideOnRun)
            {
                TrySwitchStates(StateFactory.GetState<SCh_Slide>(), StateMachine.defaultValues.maxSlideTime);
                return;
            }
            TrySwitchStates(StateFactory.GetState<SCh_Slowdown>());
            return;
        }
        
    }
    
}
