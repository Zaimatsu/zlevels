using UnityEngine;
using ZLevels.Utils;

namespace ZLevels.Zoetropes
{
    public class ZoetropeRotation : MonoBehaviour
    {
        [field: SerializeField] public float Angle { get; set; } = 85.0f;
        [field: SerializeField] public float TimesPerSecond { get; set; } = 60f;

        private float currentAngle;
        private Timer.TimesPerSecondTimer rotateTimer;

        private void Start()
        {
            rotateTimer = Timer.TimesPerSecond(TimesPerSecond);
            rotateTimer.Hit += UpdateRotation;
        }

        private void Update()
        {
            rotateTimer.TimesPerSecond = TimesPerSecond;
            rotateTimer.Tick();
        }

        private void UpdateRotation(Timer.TimesPerSecondTimer caller)
        {
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            currentAngle += Angle;
        }
    }
}