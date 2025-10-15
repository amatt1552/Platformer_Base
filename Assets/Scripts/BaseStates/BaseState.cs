using UnityEngine;

public abstract class BaseState
{
    protected bool isRootState;
    protected bool StateTransitionInCooldown { get; private set; }
    private float _transitionTimer;
    protected BaseStateMachine _StateMachine { get; private set; }
    protected StateFactory StateFactory { get; private set; }
    protected BaseState CurrentSuperState { get; private set; }
    protected BaseState CurrentSubState { get; private set; }


    /// <summary>
    /// Implemented to use new T() instead of creating functions for each new State in the StateFactory.
    /// Must call initialization after if using this method.
    /// </summary>
    public BaseState() { }

    /// <summary>
    /// Used to initialize values if not already set in constructor.
    /// </summary>
    /// <param name="stateMachine"></param>
    /// <param name="characterStateFactory"></param>
    public virtual void Initialization(BaseStateMachine stateMachine, StateFactory characterStateFactory)
    {
        _StateMachine = _StateMachine == null ? stateMachine : _StateMachine;
        StateFactory = StateFactory == null ? characterStateFactory : StateFactory;
        InitializeSubState();
    }

    /// <summary>
    /// Used to set values on entering state.
    /// Base Invokes onEnterState and onEnterSubState event. 
    /// Called on Switch State.
    /// </summary>
    public virtual void EnterState()
    {
        if (isRootState)
            StateFactory.InvokeOnEnterState(this);
    }

    /// <summary>
    /// Used if needing to update some setting for this state.
    /// Do not use to switch state.
    /// </summary>
    public virtual void UpdateState()
    {
        CurrentSubState?.UpdateState();
        _transitionTimer -= Time.deltaTime;
        _transitionTimer = Mathf.Clamp(_transitionTimer, 0, Mathf.Infinity);
        if (_transitionTimer <= 0)
        {
            StateTransitionInCooldown = false;
        }
    }

    /// <summary>
    /// Used if needing to update some setting in fixed update for this state.
    /// Do not use to switch state.
    /// </summary>
    public virtual void FixedUpdateState()
    {
        CurrentSubState?.FixedUpdateState();
    }

    /// <summary>
    /// Used to set values on exiting state.
    /// </summary>
    public abstract void ExitState();

    /// <summary>
    /// Used to determine if it should change state.
    /// </summary>
    public virtual void CheckSwitchStates()
    {
        CurrentSubState?.CheckSwitchStates();
    }

    /// <summary>
    /// Initializes subStates
    /// </summary>
    public abstract void InitializeSubState();

    /// <summary>
    /// Switches state if current state is not in cooldown.
    /// </summary>
    /// <param name="newState"></param>
    /// <param name="transitionCooldown"> Sets cooldown for newState </param>
    /// <returns> returns true if switch state was successful. </returns>
    protected bool TrySwitchStates(BaseState newState, float transitionCooldown = 0)
    {
        // Checks if switching states is in cooldown
        if (!StateTransitionInCooldown)
        {
            SwitchState(newState, transitionCooldown);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Switches state even if current state is in cooldown.
    /// </summary>
    /// <param name="newState"></param>
    /// <param name="transitionCooldown"> Sets cooldown for newState </param>
    protected void ForceSwitchStates(BaseState newState, float transitionCooldown = 0)
    {
        SwitchState(newState, transitionCooldown);
    }

    private void SwitchState(BaseState newState, float transitionCooldown = 0)
    {
        // Triggers Exit State
        ExitState();
        // Sets root State
        if (CurrentSuperState == null)  isRootState = true; 

        // Sets current state
        if (isRootState)
        {
            // Sets current state in State Machine
            newState.SetSubState(CurrentSubState);
            _StateMachine.currentState = newState;
        }
        else
        {
            // Sets the current super state's sub state to the new state.
            CurrentSuperState?.SetSubState(newState);
        }

        // Sets cooldown for new state.
        newState.StartCooldown(transitionCooldown);
        // Triggers Enter State
        newState.EnterState();
    }
    protected void SetSuperState(BaseState newSuperState)
    {
        CurrentSuperState = newSuperState;
    }
    protected void SetSubState(BaseState newSubState)
    {
        newSubState.SetSuperState(this);
        CurrentSubState = newSubState;
    }

    public override bool Equals(object obj)
    {
        return GetType().Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    private void StartCooldown(float time)
    {

        _transitionTimer = time;
        StateTransitionInCooldown = true;
    }
}
