using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZLevels
{
    public class GameLoader : MonoBehaviour
    {
        [SerializeField] private SceneAsset[] sceneAssetsToLoad;

        private void Awake()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            foreach (SceneAsset sceneAsset in sceneAssetsToLoad)
            {
                SceneManager.LoadScene(sceneAsset.name, LoadSceneMode.Additive);
            }

            SceneManager.UnloadSceneAsync(activeScene);
        }
    }
}