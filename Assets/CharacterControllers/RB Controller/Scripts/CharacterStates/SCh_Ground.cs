using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class SCh_Ground : CharacterBaseState
{
    float coyoteCountdown;
    bool trueGrounded;
    public SCh_Ground() 
    {
        isRootState = true;
    }


    public override void EnterState()
    {
        SetSubState(StateFactory.GetState<SCh_Slowdown>());
        base.EnterState();
        StateMachine.ResetJump();
        coyoteCountdown = StateMachine.defaultValues.coyoteTime;
        StateMachine.animator.SetTrigger("Grounded");
        Debug.Log($"Grounded. is root: {isRootState}");
    }

    public override void ExitState()
    {
        StateMachine.animator.ResetTrigger("Grounded"); 
        trueGrounded = false;
    }

    public override void InitializeSubState()
    {
        SetSubState(StateFactory.GetState<SCh_Idle>());
    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
        // Used to prevent player from floating after grounded
        if (StateMachine.Grounded && StateMachine.CurrentGroundDistance > 0.1f)
        {
            
            StateMachine.Gravity();
        }
        else if(!trueGrounded)
        {
            StateMachine.ApplyGround();
            trueGrounded = true;
            
        }
    }
    public override void CheckSwitchStates()
    {
        // Used to call check switch states on child state.
        base.CheckSwitchStates();

        // Jump
        if (SwitchToJump()) return;
        
        // Allows extra time before falling.
        // Useful for giving more time for a jump.
        if (!StateMachine.Grounded)
        {
            coyoteCountdown -= Time.deltaTime;

            if (coyoteCountdown <= 0)
            {
                TrySwitchStates(StateFactory.GetState<SCh_Falling>());

                //Remove the recorded seconds.
                coyoteCountdown = StateMachine.defaultValues.coyoteTime;
                return;
            }
        }
        else
        {
            coyoteCountdown = StateMachine.defaultValues.coyoteTime;
        }
    }

}
