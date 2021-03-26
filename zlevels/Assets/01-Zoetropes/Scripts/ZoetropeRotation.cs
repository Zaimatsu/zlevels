using UnityEngine;

namespace ZLevels.Zoetropes
{
    public class ZoetropeRotation : MonoBehaviour
    {
        [field: SerializeField] public float Angle { get; set; } = 85.0f;
        [field: SerializeField] public float TimesPerSecond { get; set; } = 60f;

        private float dt;
        private float currentAngle;
        private float currentTime;

        private void Start()
        {
            dt = 1000.0f / TimesPerSecond;
        }

        private void Update()
        {
            // comment out after testing
            dt = 1000.0f / TimesPerSecond;

            currentTime += Time.deltaTime * 1000.0f;

            while (currentTime >= dt)
            {
                currentTime -= dt;
                transform.rotation = Quaternion.Euler(0, currentAngle, 0);
                currentAngle += Angle;
            }
        }
    }
}