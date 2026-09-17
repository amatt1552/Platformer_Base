using UnityEngine;

public abstract class ExampleState : BaseState
{
    public ExampleState()
    {
        //everything that should happen on creation
        isRootState = true;
    }
    public override void EnterState()
    {
        Debug.Log($"This is an example! is root? {isRootState}");
        base.EnterState();
    }

    public override void ExitState()
    {
        //actions done on exiting state.
        //like holstering a weapon or stopping an animation.
    }

    public override void InitializeSubState()
    {
        // Add states allowed with root state
        // Interactions?
        // Shooting?
    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
    }
    public override void CheckSwitchStates()
    {
        base.CheckSwitchStates();
        //How you switch to another state.
        //if (TrySwitchStates(StateFactory.GetState<SomeExampleState>())) return;
    }
}
