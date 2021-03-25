using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ZLevels.Hub
{
    public class SceneInfo : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image screenshotImage;
        [SerializeField] private Button runButton;
        [SerializeField] private Camera mainCamera;

        private SceneAsset selectedSceneToRun;

        private void Start()
        {
            runButton.interactable = false;
        }

        private void OnEnable()
        {
            runButton.onClick.AddListener(RunButtonOnClick);
        }

        private void OnDisable()
        {
            runButton.onClick.RemoveListener(RunButtonOnClick);
        }

        public void Display(SceneDataSO sceneData)
        {
            titleText.text = sceneData.Name;
            descriptionText.text = sceneData.Description;
            screenshotImage.sprite = sceneData.Screenshot;
            selectedSceneToRun = sceneData.Scene;
            runButton.interactable = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(descriptionText, Input.mousePosition, mainCamera);
            if (linkIndex == -1) return;

            TMP_LinkInfo linkInfo = descriptionText.textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }

        private void RunButtonOnClick() => SceneManager.LoadScene(selectedSceneToRun.name);
    }
}