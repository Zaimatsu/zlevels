using System.Linq;
using UnityEngine;

namespace ZLevels.Hub
{
    public class ViewManager : MonoBehaviour
    {
        [SerializeField] private SceneDataSO[] sceneDatas;
        [SerializeField] private SceneListElement sceneListElementPrefab;
        [SerializeField] private Transform sceneListElementsParent;
        [SerializeField] private SceneInfo sceneInfo;

        private void Start()
        {
            foreach (SceneDataSO sceneDataSo in sceneDatas)
            {
                SceneListElement newSceneListElement = Instantiate(sceneListElementPrefab, sceneListElementsParent);
                newSceneListElement.Initialize(sceneDataSo);
            }

            sceneInfo.Display(sceneDatas.First());
            SceneListElement.Clicked += SceneListElementOnClicked;
        }

        private void SceneListElementOnClicked(SceneListElement caller) => sceneInfo.Display(caller.Data);
    }
}