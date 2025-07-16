using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace AnimationCalls
{ 
    public class AnimationStateNotifier<TState> : MonoBehaviour where TState : struct, Enum
    { 
        static public readonly int StatesCount = CExtensions.EnumCount<TState>();
        public UnityEvent<TState> OnEnter, OnExit;
        [SerializeField, Get] Animator _anim;
        readonly AwaitableCompletionSource _awaitStateExit = new(), _awaitStateEnter = new();
        Dictionary<TState, UnityEvent> OnStateEnter  = new() 
            , OnStateExit = new();
        Dictionary<TState, AwaitableCompletionSource> StatesEnterAwaitable = new(),
            StatesExitAwaitable = new();

        public Awaitable AwaitStateExit => _awaitStateExit.Awaitable;
        public Awaitable AwaitStateEnter => _awaitStateEnter.Awaitable;
        public TState State { get; private set; }
        public float NormalizedStateTime => _anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        T GetValue<T>(Dictionary<TState, T> dic, TState state) where T : new()
        {
            if (!dic.TryGetValue(state, out T e))
            {
                e = new T();
                dic.Add(state, e);
            }
            return e;
        }
        public void HookOnStateEnter(TState state, UnityAction action) => GetValue( OnStateEnter ,state).AddListener(action); 
        public void UnhookOnStateEnter(TState state, UnityAction action) => GetValue(OnStateEnter, state).RemoveListener(action);
        public void HookOnStateExit(TState state, UnityAction action) => GetValue(OnStateExit, state).AddListener(action); 
        public void UnhookOnStateExit(TState state, UnityAction action) => GetValue(OnStateExit, state).RemoveListener(action);

        public Awaitable GetEnterAwaitable(TState state) => GetValue(StatesEnterAwaitable, state).Awaitable;
        public Awaitable GetExitAwaitable(TState state) => GetValue(StatesExitAwaitable, state).Awaitable;

        public void InvokeTimedEvent(string sEvent)
        {
            if (Enum.TryParse( sEvent , out TState state) )
                GetValue(OnStateEnter, state).Invoke();
            else
                Debug.LogError("Couldnt parse " + sEvent + " as an enum of " + nameof(TState));
        }

        void InvokeAwaitable(TState state, bool enter)
        {
            var awaitable = enter ? _awaitStateEnter : _awaitStateExit;
            awaitable.SetResult();
            awaitable.Reset();
            awaitable = GetValue(enter ? StatesEnterAwaitable : StatesExitAwaitable, state);
            awaitable.SetResult();
            awaitable.Reset();
        }

        public void NotifyStateEnter(TState state)
        {
            State = state;
            OnEnter.Invoke(state);
            GetValue(OnStateEnter, state).Invoke();
            InvokeAwaitable(state, true);
        } 
         
        public void NotifyStateExit(TState state)
        {
            OnExit.Invoke(state);
            GetValue(OnStateExit, state).Invoke(); 
            InvokeAwaitable(state, false);
        }
    }

}


