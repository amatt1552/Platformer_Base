using System;
using System.Collections.Generic;
using UnityEngine;

public class StateFactory 
{
    private BaseStateMachine _stateMachine;
    private Dictionary<Type, BaseState> _states = new Dictionary<Type, BaseState>();

    public delegate void OnEnterState(BaseState state);
    public OnEnterState onEnterState;
    public void InvokeOnEnterState(BaseState state) => onEnterState?.Invoke(state);

    public StateFactory(BaseStateMachine stateMachine) 
    {
        this._stateMachine = stateMachine;
    }

    /// <summary>
    /// Uses Generics to avoid rewriting functions to create a new state.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public BaseState GetState<T>() where T : BaseState, new() 
    {
        // Gets type for key
        var stateType = typeof(T);
        // Adds to dictionary if does not exist
        if (!_states.ContainsKey(stateType)) 
        {
            Debug.Log($"Creating new state of {stateType}");
            var newState = new T();
            newState.Initialization(_stateMachine, this);
            _states.Add(stateType, newState);
        }

        return _states[stateType];
    }


    //public CharacterBaseState Idle()
    //{
    //    return new CharacterIdleState(_stateMachine, this);
    //}
    //public CharacterBaseState Walk()
    //{
    //    return new CharacterWalkState(_stateMachine, this);
    //}
    //public CharacterBaseState Run()
    //{
    //    return new CharacterRunState(_stateMachine, this);
    //}
    //public CharacterBaseState Jump()
    //{
    //    return new CharacterJumpState(_stateMachine, this);
    //}
    //public CharacterBaseState Ground()
    //{
    //    return new CharacterGroundState(_stateMachine, this);
    //}
}
