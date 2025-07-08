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
        AwaitableCompletionSource _awaitStateExit = new();
        Dictionary<TState, UnityEvent> OnStateEnter  = new() 
            , OnStateExit = new();
        public Awaitable AwaitStateExit => _awaitStateExit.Awaitable;
        public TState State { get; private set; }
        public float NormalizedStateTime => _anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        UnityEvent GetEvent(Dictionary<TState, UnityEvent> dic, TState state)
        {
            if (!dic.TryGetValue(state, out UnityEvent e))
            {
                e = new UnityEvent();
                dic.Add(state, e);
            }
            return e;
        }
        public void HookOnStateEnter(TState state, UnityAction action) => GetEvent( OnStateEnter ,state).AddListener(action); 
        public void UnhookOnStateEnter(TState state, UnityAction action) => GetEvent(OnStateEnter, state).RemoveListener(action);
        public void HookOnStateExit(TState state, UnityAction action) => GetEvent(OnStateExit, state).AddListener(action); 
        public void UnhookOnStateExit(TState state, UnityAction action) => GetEvent(OnStateExit, state).RemoveListener(action);

        public void InvokeTimedEvent(string sEvent)
        {
            if (Enum.TryParse( sEvent , out TState state) )
                GetEvent(OnStateEnter, state).Invoke();
            else
                Debug.LogError("Couldnt parse " + sEvent + " as an enum of " + nameof(TState));
        }

        public void NotifyStateEnter(TState state)
        {
            Debug.Log($"{gameObject.name} {State}->{state}");
            State = state;
            OnEnter.Invoke(state);
            GetEvent(OnStateEnter, state).Invoke();
        } 
         
        public void NotifyStateExit(TState state)
        {
            OnExit.Invoke(state);
            GetEvent(OnStateExit, state).Invoke();
            _awaitStateExit.SetResult();
            _awaitStateExit.Reset();
        } 
    }

}


