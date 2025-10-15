
using UnityEngine;
using UnityEngine.EventSystems;

public class SCh_WallRunXZ : CharacterBaseState
{
    public SCh_WallRunXZ() : base() { }
    public override void EnterState()
    {
        base.EnterState();
        Debug.Log($"Wall Running XZ. Is root: {isRootState}");
    }

    public override void ExitState()
    {
        StateMachine.StopWallRun();
    }

    public override void InitializeSubState()
    {
    }

    public override void FixedUpdateState()
    {
        StateMachine.WallRunningMovementXZ();
    }
    public override void CheckSwitchStates()
    {
        if (StateMachine.Grounded || !StateMachine.WallRunPossible || StateMachine.JumpPressed)
        {
            TrySwitchStates(StateFactory.GetState<SCh_Idle>(),0.5f);
        }
    }

    
}
