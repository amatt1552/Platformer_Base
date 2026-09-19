using UnityEngine;
namespace CHController
{
    public class SCh_AirJump : CharacterBaseState
    {
        public SCh_AirJump() : base() { }
        public override void EnterState()
        {
            base.EnterState();
            Debug.Log($"Jumped. is root? {isRootState}");
            StateMachine.TryJump(true);
            StateMachine.animator.SetTrigger("AirJump");
        }

        public override void ExitState()
        {
            StateMachine.JumpComplete();
            StateMachine.animator.ResetTrigger("AirJump");
        }

        public override void UpdateState()
        {
            base.UpdateState();
            StateMachine.HandleGravity(StateMachine.defaultValues.jumpGravityMagnitude);
        }
        public override void FixedUpdateState()
        {
            //SetupMovementXZ();
        }
        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
            if (StateMachine.controller.isGrounded && TrySwitchStates(StateFactory.GetState<SCh_Ground>())) return;
            if (StateMachine.IsFalling && TrySwitchStates(StateFactory.GetState<SCh_Falling>())) return;
        }
    }

}
