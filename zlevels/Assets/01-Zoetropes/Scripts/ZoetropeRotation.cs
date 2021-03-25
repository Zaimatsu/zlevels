using UnityEngine;

namespace Zoetropes
{
    public class ZoetropeRotation : MonoBehaviour
    {
        [SerializeField] private float angle = 30.0f;
        [SerializeField] private float timesPerSecond = 60f;

        private float dt;
        private float currentAngle;
        private float currentTime;

        private void Start()
        {
            dt = 1000.0f / timesPerSecond;
        }

        private void Update()
        {
            // comment out after testing
            dt = 1000.0f / timesPerSecond;

            currentTime += Time.deltaTime * 1000.0f;

            while (currentTime >= dt)
            {
                currentTime -= dt;
                transform.rotation = Quaternion.Euler(0, currentAngle, 0);
                currentAngle += angle;
            }
        }
    }
}