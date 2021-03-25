using UnityEngine;

/// <summary>
/// https://youtu.be/B5p2A5mazEs
/// </summary>
public class ZoetropesGenerator : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    [SerializeField] private float angleParts = 36.0f;
    [SerializeField] private float baseOffset = 1.0f;
    [SerializeField] private float heightOffset = 0.05f;
    [SerializeField] private AnimationCurve offsetInHeight;

    [ContextMenu("Generate")]
    public void Generate()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        float angle = 360.0f / angleParts;
        var position = new Vector3(baseOffset, 0, 0);
        Quaternion rotation = Quaternion.identity;
        for (var i = 0; i < angleParts; i++)
        {
            GameObject newGO = Instantiate(obj, position, rotation, transform);
            newGO.name = $"part-{i}";
            position = Quaternion.AngleAxis(MathUtils.GOLDEN_ANGLE, Vector3.up) * position;
            position += new Vector3(0, heightOffset, 0);
            Vector2 flatPosition = new Vector2(position.x, position.z).normalized;
            flatPosition *= baseOffset *
                            offsetInHeight.Evaluate(1 - Mathf.InverseLerp(0, angleParts * heightOffset,
                                i * heightOffset));
            position = new Vector3(flatPosition.x, position.y, flatPosition.y);
            rotation *= Quaternion.Euler(Vector3.up * MathUtils.GOLDEN_ANGLE);
        }
    }
}