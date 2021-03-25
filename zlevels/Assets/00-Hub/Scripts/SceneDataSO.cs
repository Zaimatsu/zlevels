using UnityEditor;
using UnityEngine;

namespace ZLevels.Hub
{
    [CreateAssetMenu(menuName = "Create SceneDataSO", fileName = "SceneDataSO", order = 0)]
    public class SceneDataSO : ScriptableObject
    {
        [field: SerializeField, Multiline] public string Name { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: SerializeField] public SceneAsset Scene { get; private set; }
        [field: SerializeField] public Sprite Screenshot { get; private set; }
    }
}