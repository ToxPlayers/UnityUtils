using System;
using System.Threading;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using System.Collections.Generic;

#else
using TriInspector;
#endif

#if ODIN_INSPECTOR
using HideInEdit = Sirenix.OdinInspector.HideInEditorModeAttribute;
#else
using HideInEdit = TriInspector.HideInEditModeAttribute;
#endif
 
 
[Serializable]
public struct RealTimer
{ 
    public bool UseInconsistentFrameTime;
    public float MaxTime;
    public float TimeStarted { get; private set; }
    public bool TimerOver => IsTimerOver(MaxTime);
    public bool IsTimerOver(float maxTime) => TimeRunning >= maxTime;
    public int TimerOverCount 
    {
        get
        {
            if (MaxTime == 0)
                return 0;
            return Mathf.FloorToInt(TimeRunning / MaxTime);
        }
    }
    public float TimeSinceStartup => GetTimeSinceStartup(UseInconsistentFrameTime);
    static public float GetTimeSinceStartup(bool UseFrameTime)
    {
        if (UseFrameTime)
            return Time.unscaledTime;
        return Time.realtimeSinceStartup;
    } 

    [ShowInInspector, HideInEdit] public float TimeRunning => TimeSinceStartup - TimeStarted;
    public float TimeLeft => TimeRunning - MaxTime;
    [ShowInInspector, HideInEdit] public float NormalizedTime
    {
        get
        {
            if (MaxTime == 0)
                return 1f;
            return Mathf.Clamp01(TimeRunning / MaxTime);
        }
    }
    public void Restart(float timeOffset = 0f)
    {
        TimeStarted = TimeSinceStartup + timeOffset; 
    }
    public void RestartWithTimeOverOffset()
    {
        if (MaxTime == 0)
            Restart();
        else TimeStarted = TimeSinceStartup + TimeRunning % MaxTime;
    }
    public async Awaitable AwaitTimer(CancellationToken cancelToken = default)
    {  
        while (!TimerOver)
                await Awaitable.NextFrameAsync(cancelToken);
    }

    public void ForceTimerOver()
    {
        TimeStarted = - MaxTime;
    }

    public RealTimer(bool useInconsistentFrameTime, float maxTime)
    { 
        MaxTime = maxTime;
        TimeStarted = UnityExtensions.IsOnUnityThread ? GetTimeSinceStartup(useInconsistentFrameTime) : 0f;
        UseInconsistentFrameTime = useInconsistentFrameTime;
    }
	public RealTimer(float maxTime)
    { 
        MaxTime = maxTime;
        TimeStarted = UnityExtensions.IsOnUnityThread ? GetTimeSinceStartup(false) : 0f;
        UseInconsistentFrameTime = false;
    } 
}
