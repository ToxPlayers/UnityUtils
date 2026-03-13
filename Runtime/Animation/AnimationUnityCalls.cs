using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace AnimationCalls
{ 
     public class AnimationUnityCalls : MonoBehaviour {
     public UnityEvent OnAnimatorUpdated { get; private set; } = new();
     public UnityEvent OnPreIK { get; private set; } = new();
     private void OnAnimatorMove() {
         OnAnimatorUpdated.Invoke();
     }

     private void OnAnimatorIK(int layerIndex) {
         OnPreIK.Invoke();
     }

     private void OnDestroy() {
         OnAnimatorUpdated.RemoveAllListeners();
         OnAnimatorUpdated = null;
         OnPreIK.RemoveAllListeners();
         OnPreIK = null;
     }

 }

}


