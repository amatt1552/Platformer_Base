
using UnityEngine;
using UnityEngine.EventSystems;

public class SCh_MoveDisabled : CharacterBaseState
{
    public SCh_MoveDisabled() : base() { }
    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("Movement Disabled");
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
    }
    public override void CheckSwitchStates()
    {
    }

    
}
