using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.Animations;
namespace AnimationCalls
{ 
    public class AnimationEventStateBehaviour<TState> : StateMachineBehaviour where TState : struct, Enum
    {
        [SerializeField] TState _state;
        AnimationStateNotifier<TState> _notifier;
        bool _firstEnter = true; 
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {  
            if (_firstEnter)
            {
                _notifier = animator.GetComponent<AnimationStateNotifier<TState>>();
                _firstEnter = false;
            }
            if(_notifier)
                _notifier.NotifyStateEnter(_state);
        } 
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_notifier)
                _notifier.NotifyStateExit(_state);
        }
    }
}


