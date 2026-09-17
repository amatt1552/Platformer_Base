
using UnityEngine;
namespace CHController
{
    public class SCh_Idle : CharacterBaseState
    {
        public SCh_Idle() : base() { }
        public override void EnterState()
        {
            base.EnterState();
            Debug.Log("Idle.");
        }

        public override void ExitState()
        {

        }

        public override void UpdateState()
        {
            base.UpdateState();
            StateMachine.ApplyRotation();
            StateMachine.ApplyMovementXZ();
        }
        public override void FixedUpdateState()
        {
            //SetupMovementXZ();
        }
        public override void CheckSwitchStates()
        {
            if (StateMachine.isMoving && TrySwitchStates(StateFactory.GetState<SCh_Movement>())) return;
        }


    }
}
