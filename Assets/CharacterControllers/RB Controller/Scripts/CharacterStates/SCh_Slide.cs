using UnityEngine;

public class SCh_Slide : CharacterBaseState
{
    public SCh_Slide() : base() { }


    public override void EnterState()
    {
        base.EnterState();
        if (StateMachine.disableMovementOnSlide)
        { 
            StateMachine.StopRotation();
        }
        StateMachine.SetSlidingFriction();
        StateMachine.animator.SetTrigger("Sliding"); 
        StateMachine.animator.ResetTrigger("SlidingEnd");
        Debug.Log("Started Slide");
    }

    public override void ExitState()
    {
        StateMachine.animator.ResetTrigger("Sliding");
        StateMachine.animator.SetTrigger("SlidingEnd");
        Debug.Log("Exiting Slide");
    }

    public override void InitializeSubState()
    {
        // Add states allowed while Sliding.
        // Interactions?
        // Shooting?
    }
    public override void FixedUpdateState()
    {
        StateMachine.SlowDown();
        if (!StateMachine.disableMovementOnSlide)
        {
            //SetupMovementXZ();
            StateMachine.RotateBody();
        }
    }
    public override void CheckSwitchStates()
    {
        if (!StateMachine.disableMovementOnSlide)
        {
            if (ForceSwitchToMoving()) return;
        }

        // Switches state when state's cooldown is done.
        if (TrySwitchStates(StateFactory.GetState<SCh_Idle>())) return;

        // Forces switch to state regardless of cooldown.
        if (StateMachine.rb.linearVelocity.magnitude < 0.1f || !CurrentSuperState.Equals(typeof(SCh_Ground))) 
        {
            ForceSwitchStates(StateFactory.GetState<SCh_Idle>());
            return;
        }
    }

}
