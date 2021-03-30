using UnityEngine;

namespace ZLevels.Hub
{
    [CreateAssetMenu(menuName = "Create SceneDataSO", fileName = "SceneDataSO", order = 0)]
    public class SceneDataSO : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: SerializeField] public SceneReference Scene { get; private set; }
        [field: SerializeField] public Sprite Screenshot { get; private set; }
        [field: SerializeField] public bool isUsingComputeShaders { get; private set; }
    }
}