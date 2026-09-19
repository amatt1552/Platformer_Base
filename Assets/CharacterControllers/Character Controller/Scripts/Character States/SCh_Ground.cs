
using UnityEngine;
namespace CHController
{
    public class SCh_Ground : CharacterBaseState
    {
        public SCh_Ground()
        {
            isRootState = true;
        }


        public override void EnterState()
        {
            
            base.EnterState();
            Debug.Log($"Grounded. is root: {isRootState}");
            StateMachine.ResetJump();
            StateMachine.animator.SetTrigger("Grounded");
        }

        public override void ExitState()
        {

            StateMachine.animator.ResetTrigger("Grounded");
        }

        public override void InitializeSubState()
        {
            SetSubState(StateFactory.GetState<SCh_Idle>());
        }

        public override void UpdateState()
        {
            base.UpdateState();
            StateMachine.HandleGravityGround();
           
        }
        public override void CheckSwitchStates()
        {
            // Used to call check switch states on child state.
            base.CheckSwitchStates();
            if (StateMachine.JumpPressed && TrySwitchStates(StateFactory.GetState<SCh_Jump>(), 0.1f)) return;
            if (!StateMachine.controller.isGrounded && TrySwitchStates(StateFactory.GetState<SCh_Falling>())) return;
        }
    }
}
