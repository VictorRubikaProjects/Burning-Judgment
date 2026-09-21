using System.Collections.Generic;

public static class TimerManager
{
        private static readonly List<Timer> m_timers = new();

        public static void RegisterTimer(Timer timer)
        {
                if (!m_timers.Contains(timer)) m_timers.Add(timer);
        }
        
        public static void UnregisterTimer(Timer timer) => m_timers.Remove(timer);

        public static void UpdateTimers()
        {
                for (int i = 0; i < m_timers.Count; i++)
                {
                        m_timers[i].Tick();
                }
        }
        
        public static void ClearTimers() => m_timers.Clear();
        
}