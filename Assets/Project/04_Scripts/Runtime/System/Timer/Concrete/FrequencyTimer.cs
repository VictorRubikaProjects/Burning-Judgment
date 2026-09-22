using System;
using UnityEngine;

public class FrequencyTimer : Timer
{
    public float TicksPerSecond { get; private set; }

    public Action OnTick = delegate { };

    float _timeThreshold;

    public FrequencyTimer(float ticksPerSecond) : base(0) {
        CalculateTimeThreshold(ticksPerSecond);
    }

    public override void Tick() {
           
        if (IsRunning && CurrentTime >= _timeThreshold) {
            CurrentTime -= _timeThreshold;
            OnTick.Invoke();
        }
            
        if (IsRunning && CurrentTime < _timeThreshold) {
            CurrentTime += Time.deltaTime;
        }
    }

    public override bool IsFinished => !IsRunning;

    public override void Reset() {
        CurrentTime = 0;
    }

    public override void Reset(float newTicksPerSecond) {
        CalculateTimeThreshold(newTicksPerSecond);
        Reset();
    }

    void CalculateTimeThreshold(float ticksPerSecond) {
        TicksPerSecond = ticksPerSecond;
        _timeThreshold = 1f / TicksPerSecond;
    }
}