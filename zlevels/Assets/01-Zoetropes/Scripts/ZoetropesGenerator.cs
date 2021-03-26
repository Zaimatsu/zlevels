using UnityEngine;
using ZLevels.Utils;

namespace ZLevels.Zoetropes
{
    /// <summary>
    /// https://youtu.be/B5p2A5mazEs
    /// </summary>
    public class ZoetropesGenerator : MonoBehaviour
    {
        [field: SerializeField] public int Parts { get; set; } = 300;
        [field: SerializeField] public float Radius { get; set; } = 1.0f;
        [field: SerializeField] public float Height { get; set; } = 0.005f;

        [SerializeField] private GameObject obj;
        [SerializeField] private AnimationCurve offsetInHeight;

        [ContextMenu("Generate")]
        public void Generate()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            var position = new Vector3(Radius, 0, 0);
            Quaternion rotation = Quaternion.identity;
            for (var i = 0; i < Parts; i++)
            {
                position = Quaternion.AngleAxis(MathUtils.GOLDEN_ANGLE, Vector3.up) * position;
                position += new Vector3(0, Height, 0);
                Vector2 flatPosition = new Vector2(position.x, position.z).normalized;
                flatPosition *= Radius *
                                offsetInHeight.Evaluate(1 - Mathf.InverseLerp(0, Parts * Height,
                                    i * Height));
                position = new Vector3(flatPosition.x, position.y, flatPosition.y);
                rotation *= Quaternion.Euler(Vector3.up * MathUtils.GOLDEN_ANGLE);
                
                GameObject newGO = Instantiate(obj, position, rotation, transform);
                newGO.name = $"part-{i}";
            }
        }

        public void SetShapeCurve(params float[] values)
        {
            offsetInHeight = new AnimationCurve();
            for (var index = 0; index < values.Length; index++)
            {
                float value = values[index];
                offsetInHeight.AddKey((float) index / (values.Length - 1), value);
            }
        }
    }
}