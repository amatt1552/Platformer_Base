using UnityEngine;

public class BaseStateMachine : MonoBehaviour
{
    [Header("State Settings")]
    [HideInInspector]
    public BaseState currentState;
    protected StateFactory _stateFactory;
    protected virtual void Awake()
    {
        // Initializes character states
        _stateFactory = new(this);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        currentState?.CheckSwitchStates();
        currentState?.UpdateState();

    }

    protected virtual void FixedUpdate() 
    {
        currentState?.FixedUpdateState();
    }

    protected virtual void EnterStateListener(BaseState state) 
    {
        Debug.Log($"Entered State {state}");
    }
}
