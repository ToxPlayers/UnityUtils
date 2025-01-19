using System;
using System.Threading;
using UnityEngine;
using TriInspector;
using System.Threading.Tasks;

[Serializable]
public struct GameBoolTimer
{
    bool _value;
    [ShowInInspector, ReadOnly]
    public bool Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                Timer.Restart();
            }
        }
    }
    [ShowInInspector, HideLabel] public GameTimer Timer;
    public bool TimerOver => Timer.TimerOver;
    public float NormalizedTime => Timer.NormalizedTime;
    public float TimeRunning => Timer.TimeRunning;

    public static implicit operator bool(GameBoolTimer timer) => timer.Value;

    public GameBoolTimer(float maxTime, bool value = false, GameTimer.TimerType type = GameTimer.TimerType.Scaled)
    {
        Timer = new GameTimer(type, maxTime);
        _value = value;
    }
}

[Serializable]
public struct GameTimer
{
    public enum TimerType { Scaled, Unscaled, Realtime }
    public TimerType timerType;
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
    public float TimeSinceStartup => GetTimeSinceStartup(timerType);
    static public float GetTimeSinceStartup(TimerType type)
    {
        return type switch
        {
            TimerType.Realtime => Time.realtimeSinceStartup,
            TimerType.Unscaled => Time.unscaledTime,
            _ => Time.time
        };
    } 
    [ShowInInspector, HideInEditMode] public float TimeRunning => TimeSinceStartup - TimeStarted;
    public float TimeLeft => TimeRunning - MaxTime;
    [ShowInInspector, HideInEditMode] public float NormalizedTime
    {
        get
        {
            if (MaxTime == 0)
                return 1f;
            return Mathf.Clamp01(TimeRunning / MaxTime);
        }
    }

    public void Restart()
    {
        TimeStarted = TimeSinceStartup; 
    }
    public void RestartWithTimeOverOffset()
    {
        if (MaxTime == 0)
            Restart();
        else TimeStarted = TimeSinceStartup + TimeRunning % MaxTime;
    }
    bool IsTimerOver() => TimerOver;
    public async Awaitable AwaitTimer(CancellationToken cancelToken = default)
    {  
        while (!TimerOver)
                await Awaitable.NextFrameAsync(cancelToken);
    }

    public void ForceTimerOver()
    {
        TimeStarted = - MaxTime;
    }

    public GameTimer(TimerType timerType, float maxTime)
    {
        this.timerType = timerType;
        MaxTime = maxTime;
        TimeStarted = UnityExtensions.IsOnUnityThread ? GetTimeSinceStartup(this.timerType) : 0f; 
    }
}
