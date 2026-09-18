using UnityEngine;
namespace CHController
{
    public class SCh_Jump : CharacterBaseState
    {
        public SCh_Jump() : base() { }
        public override void EnterState()
        {
            base.EnterState();
            Debug.Log($"Jumped. is root? {isRootState}");
            StateMachine.TryJump();
            StateMachine.animator.SetTrigger("Jump");
        }

        public override void ExitState()
        {
            StateMachine.JumpComplete();
            StateMachine.animator.ResetTrigger("Jump");
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
