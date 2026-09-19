using System.Collections.Generic;

public static class TimerManager
{
        private static readonly List<Timer> m_timers = new();

        public static void RegisterTimer(Timer timer) => m_timers.Add(timer);
        public static void UnregisterTimer(Timer timer) => m_timers.Remove(timer);

        public static void UpdateTimers()
        {
                foreach (Timer timer in m_timers)
                {
                        timer.Tick();
                }        
        }
        
        public static void ClearTimers() => m_timers.Clear();
        
}