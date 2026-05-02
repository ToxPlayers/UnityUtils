using System;
using UnityEngine;
using UnityEngine.Events;

public static class DisposableListenerExtension {
    static public DisposableListenerBase AddDisposableListener(this UnityEvent @event, UnityAction action) {
        return new DisposableListener(@event, action); 
    }
    static public DisposableListenerBase AddDisposableListener<T>(this UnityEvent<T> @event, UnityAction<T> action) {
        return new DisposableListener<T>(@event, action);
    }
    static public DisposableListenerBase AddDisposableListener<T1,T2>(this UnityEvent<T1, T2> @event, UnityAction<T1, T2> action) {
        return new DisposableListener<T1, T2>(@event, action);
    }


    static public DisposableListenerBase AddDisposableListener<T1, T2>(this DisposableListenerBase disp, UnityEvent @event, UnityAction action) {
        if (disp == null)
            return AddDisposableListener(@event, action);

        var newDisp = AddDisposableListener(@event, action);
        newDisp.Next = disp;
        return disp;
    }
    static public DisposableListenerBase AddDisposableListener<T>(this DisposableListenerBase disp, UnityEvent<T> @event, UnityAction<T> action) {
        if (disp == null)
            return AddDisposableListener(@event, action);

        var newDisp = AddDisposableListener(@event, action);
        newDisp.Next = disp;
        return disp;
    }
    static public DisposableListenerBase AddDisposableListener(this DisposableListenerBase disp, UnityEvent @event, UnityAction action) {
        if (disp == null)
            return AddDisposableListener(@event, action);

        var newDisp = AddDisposableListener(@event, action);
        newDisp.Next = disp;
        return disp;
    }
}  

public abstract class DisposableListenerBase {
    public bool IsDisposed;
    public DisposableListenerBase Next; 
    public void DisposeListener() {
        if (IsDisposed)
            return;
        DisposeImplementation();
        var nxt = Next;
        Next = null;
        nxt?.DisposeListener();
    }
    protected abstract void DisposeImplementation();
}

public class DisposableListener : DisposableListenerBase {
    public readonly UnityEvent Event;
    public readonly UnityAction Action;
    public DisposableListener(UnityEvent @event, UnityAction action) {
        Event = @event;
        Action = action;
        Event.AddListener(Action);
    } 
    protected override void DisposeImplementation() { Event.RemoveListener(Action); }
}

public class DisposableListener<T> : DisposableListenerBase {
    public readonly UnityEvent<T> Event;
    public readonly UnityAction<T> Action;
    public DisposableListener(UnityEvent<T> @event, UnityAction<T> action) {
        Event = @event;
        Action = action;
        Event.AddListener(Action);
    } 
    protected override void DisposeImplementation() {  Event.RemoveListener(Action); }
}

public class DisposableListener<T1,T2> : DisposableListenerBase {
    public readonly UnityEvent<T1, T2> Event;
    public readonly UnityAction<T1, T2> Action;
    public DisposableListener(UnityEvent<T1, T2> @event, UnityAction<T1, T2> action) {
        Event = @event;
        Action = action;
        Event.AddListener(Action);
    } 
    protected override void DisposeImplementation() { Event.RemoveListener(Action); }
}
