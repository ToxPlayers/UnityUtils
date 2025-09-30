using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace StateMachines
{  
    public class StateMachine<TContext> : MonoBehaviour
    {
        public abstract class StateTransition
        {
            public StateBase State;
            public abstract bool ShouldTransition(StateMachine<TContext> stateMachine);
			public abstract void OnPreTransition();
			public abstract void OnPostTransition();
        } 
        public abstract class StateBase
        {
            [field: SerializeField] public List<StateTransition> Transitions { get; private set; }
            public bool IsInState { get; private set; }
            public StateMachine<TContext> StateMachine { get; private set; }
            public TContext Context => StateMachine.Context;
            public void Init(StateMachine<TContext> stateMachine)
            {
                IsInState = false;
                StateMachine = stateMachine;
            }
            internal abstract void StateEnter();
            internal abstract void StateUpdate();
            internal abstract void StateExit();
        }

        [SerializeReference] public StateBase StartingState; 
        public TContext Context { get; private set; }
        [ShowInInspector, ReadOnly] public StateBase State { get; private set; } 
        public bool IsInState(StateBase state) => ReferenceEquals(state, State);
        public bool IsInState<T>() => State is T;
        public T GetCurStateAs<T>() where T : StateBase
        {
            if (State is T s)
                return s;
            return default;
        }
        public void SetState(StateBase state)
        { 
            State?.StateExit();
            State = state;
            State?.StateEnter();
        }
        private void OnEnable()
        {
            SetState(StartingState);
        }
        private void Update()
        {
            if(State != null)
            {
                foreach (var trans in State.Transitions)
                {
                    if (trans.ShouldTransition(this))
					{
						trans.OnPreTransition();
						SetState(trans.State);
						trans.OnPostTransition();
					}
                }
                State.StateUpdate();
            }
        } 
        private void OnDisable()
        {
            SetState(null);
        }

    }
}
