using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZLevels.Hub
{
    public class ViewManager : MonoBehaviour
    {
        public static event OnLoaded Loaded;
        public static event OnLevelStarted LevelStarted;

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
            sceneInfo.RunButtonClicked += SceneInfoOnRunButtonClicked;
            Loaded?.Invoke(this);
        }

        private void SceneInfoOnRunButtonClicked(SceneInfo caller, SceneDataSO sceneData)
        {
            SceneManager.LoadScene(sceneData.Scene.name);
            LevelStarted?.Invoke(this, sceneData);
        }

        private void SceneListElementOnClicked(SceneListElement caller) => sceneInfo.Display(caller.Data);

        public delegate void OnLoaded(ViewManager viewManager);

        public delegate void OnLevelStarted(ViewManager viewManager, SceneDataSO sceneData);
    }
}