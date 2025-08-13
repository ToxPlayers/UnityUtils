using UnityEngine.Events;
using UnityEngine;

namespace UnityInternalExpose
{
    internal static class InternalEngineBridge
    {
        public static int GetListenerCount(this UnityEventBase eve) => eve.GetCallsCount();
    }
}