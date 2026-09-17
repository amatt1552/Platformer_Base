
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
namespace CHController
{
    public class SCh_Movement : CharacterBaseState
    {
        public SCh_Movement() : base() { }
        public override void EnterState()
        {
            base.EnterState();
            
            Debug.Log("Moving");
        }

        public override void ExitState()
        {

        }


        public override void UpdateState()
        {
            base.UpdateState();
            StateMachine.ApplyMovementXZ();
            StateMachine.ApplyRotation();
        }
        public override void CheckSwitchStates()
        {
            if (!StateMachine.isMoving && TrySwitchStates(StateFactory.GetState<SCh_Idle>())) return;
        }
    }
}
