using UnityEngine;
namespace CHController
{
    public class SCh_Falling : CharacterBaseState
    {
        public SCh_Falling() : base() { }
        public override void EnterState()
        {
            base.EnterState();
            Debug.Log($"Falling.is root? {isRootState}");
            
            StateMachine.animator.SetTrigger("Falling");
        }

        public override void ExitState()
        {

            StateMachine.animator.ResetTrigger("Falling");
        }

        public override void UpdateState()
        {
            base.UpdateState();
            StateMachine.HandleGravity(StateMachine.fallGravityMagnitude);
        }
        public override void CheckSwitchStates()
        {
            base.CheckSwitchStates();
            if (StateMachine.controller.isGrounded && TrySwitchStates(StateFactory.GetState<SCh_Ground>())) return;
        }
    }
}
