using UnityEngine;

public class CountdownTimer : Timer {
    public CountdownTimer(float value) : base(value) { }

    public override void Tick() {
        if(IsRunning && CurrentTime > 0) {
            CurrentTime -= Time.deltaTime;
        }

        if(IsRunning && CurrentTime <= 0) {
            Stop();
        }
    }

    public virtual void DynamicUpdate(float newTime) {
        if (!IsRunning) {
            Reset(newTime);
            return;
        }

        float elapsed = InitialTime - CurrentTime;
        InitialTime = newTime;
        CurrentTime = newTime - elapsed;

        if (CurrentTime <= 0) {
            Stop();
        }
    }

    public override bool IsFinished => CurrentTime <= 0f;
}