using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZLevels.Hub;

namespace ZLevels
{
    public class LoadedLevelUI : MonoBehaviour
    {
        [SerializeField] private Button goBackButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private SceneAsset hubScene;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            goBackButton.onClick.AddListener(GoBackButtonOnClick);
            ViewManager.Loaded += ViewManagerOnLoaded;

            ViewManager.LevelStarted += ViewManagerOnLevelStarted;
        }

        private void OnDestroy()
        {
            goBackButton.onClick.RemoveListener(GoBackButtonOnClick);
        }

        private void ViewManagerOnLevelStarted(ViewManager viewmanager, SceneDataSO loadedscene)
        {
            gameObject.SetActive(true);
            titleText.text = loadedscene.Name;
        }

        private void ViewManagerOnLoaded(ViewManager viewmanager)
        {
            gameObject.SetActive(false);
        }

        private void GoBackButtonOnClick() => SceneManager.LoadScene(hubScene.name);
    }
}