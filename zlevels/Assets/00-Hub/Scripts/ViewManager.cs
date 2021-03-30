using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        [SerializeField] private Button exitButton;

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

#if UNITY_WEBGL
            exitButton.gameObject.SetActive(false);
#else
            exitButton.gameObject.SetActive(true);
            exitButton.onClick.AddListener(Application.Quit);
#endif
        }

        private void OnDestroy()
        {
            SceneListElement.Clicked -= SceneListElementOnClicked;
            sceneInfo.RunButtonClicked -= SceneInfoOnRunButtonClicked;
        }

        private void SceneInfoOnRunButtonClicked(SceneInfo caller, SceneDataSO sceneData)
        {
            SceneManager.LoadScene(sceneData.Scene);
            LevelStarted?.Invoke(this, sceneData);
        }

        private void SceneListElementOnClicked(SceneListElement caller) => sceneInfo.Display(caller.Data);

        public delegate void OnLoaded(ViewManager viewManager);

        public delegate void OnLevelStarted(ViewManager viewManager, SceneDataSO sceneData);
    }
}