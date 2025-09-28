using Sirenix.OdinInspector;
using UnityEngine;

public interface IState
{
    void StateEnter(StateMachine stateMachine);
    void StateUpdate();
    void StateExit();
}

public abstract class StateBase : IState
{
    public StateMachine StateMachine { get; set; } 
    protected void SetState(IState state) => StateMachine.SetState(state);
    public abstract void OnStateEnter();
    public abstract void OnStateUpdate();
    public abstract void OnStateExit();
    void IState.StateEnter(StateMachine stateMachine)
    {
        StateMachine = stateMachine;
        OnStateEnter();
    }
    void IState.StateUpdate() => OnStateUpdate();
    void IState.StateExit() => OnStateExit();
}

public abstract class StateMono : MonoBehaviour, IState
{
    public StateMachine StateMachine { get; set; }
    protected void SetState(IState state) => StateMachine.SetState(state);
    public abstract void OnStateEnter();
    public abstract void OnStateUpdate();
    public abstract void OnStateExit();
    void IState.StateEnter(StateMachine stateMachine)
    {
        StateMachine = stateMachine;
        OnStateEnter();
    }
    void IState.StateUpdate() => OnStateUpdate();
    void IState.StateExit() => OnStateExit();
}

public class StateMachine : MonoBehaviour
{
    [SerializeReference] IState _defaultState;
    [ShowInInspector, ReadOnly] public IState CurrentState { get; private set; }
    public bool IsInState<T>() => CurrentState is T;
    public T GetCurStateAs<T>() where T : IState
    {
        if (CurrentState is T s)
            return s;
        return default;
    }
    public void SetState(IState state)
    {
        CurrentState?.StateExit();
        CurrentState = state;
        CurrentState?.StateEnter(this);
    }
    private void OnEnable()
    {
        SetState(_defaultState);
    }
    private void Update()
    {
        CurrentState?.StateUpdate();
    }
    private void OnDisable()
    {
        SetState(null);
    }

}
