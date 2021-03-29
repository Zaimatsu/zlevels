using UnityEngine;

namespace ZLevels.Utils
{
    public static class Timer
    {
        public class TimesPerSecondTimer
        {
            public float TimesPerSecond
            {
                get => timesPerSecond;
                set => timesPerSecond = Mathf.Abs(value);
            }

            private float dt;
            private float currentTime;
            private float timesPerSecond;
            public event OnHit Hit;

            public TimesPerSecondTimer(float timesPerSecond)
            {
                TimesPerSecond = timesPerSecond;
            }

            public void Tick()
            {
                if (TimesPerSecond == 0) return;
                dt = 1.0f / TimesPerSecond;
                currentTime += Time.deltaTime;

                while (currentTime >= dt)
                {
                    currentTime -= dt;
                    Hit?.Invoke(this);
                }
            }

            public delegate void OnHit(TimesPerSecondTimer caller);
        }

        public static TimesPerSecondTimer TimesPerSecond(float seconds) => new TimesPerSecondTimer(seconds);
    }
}