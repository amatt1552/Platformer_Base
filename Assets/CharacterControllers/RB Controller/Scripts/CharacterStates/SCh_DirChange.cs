using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SCh_DirChange : CharacterBaseState
{
    public SCh_DirChange() : base() { }

    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("Changing direction");
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
        
        if (!StateMachine.Moving)
        {
            bool runningCheck = StateMachine.rb.linearVelocity.sqrMagnitude > Mathf.Pow(StateMachine.defaultValues.runSpeed, 2);
            if (runningCheck && StateMachine.slideOnRun)
            {
                TrySwitchStates(StateFactory.GetState<SCh_Slide>(), StateMachine.defaultValues.maxSlideTime);
                return;
            }
            TrySwitchStates(StateFactory.GetState<SCh_Idle>());
            return;
        }
        
    }
    
}
