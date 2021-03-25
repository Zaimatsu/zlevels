using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZLevels
{
    public class GameLoader : MonoBehaviour
    {
        [SerializeField] private SceneReference[] sceneReferencesToLoad;

        private void Awake()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            foreach (SceneReference sceneReference in sceneReferencesToLoad)
            {
                SceneManager.LoadScene(sceneReference, LoadSceneMode.Additive);
            }

            SceneManager.UnloadSceneAsync(activeScene);
        }
    }
}