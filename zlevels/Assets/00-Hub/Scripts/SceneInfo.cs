using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZLevels.Hub
{
    public class SceneInfo : MonoBehaviour, IPointerClickHandler
    {
        public event OnRunButtonClicked RunButtonClicked;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image screenshotImage;
        [SerializeField] private Button runButton;
        [SerializeField] private Camera mainCamera;

        [SerializeField] private string unsupportedComputeShadersMessage =
            "<color=#ff0000><b>Info: Your environment doesn't support compute shaders. Maybe try running standalone version?</b></color>";

        private SceneDataSO selectedSceneToRun;

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
            selectedSceneToRun = sceneData;

            if (sceneData.isUsingComputeShaders && SystemInfo.supportsComputeShaders)
                runButton.interactable = true;
            else if (sceneData.isUsingComputeShaders && !SystemInfo.supportsComputeShaders)
            {
                descriptionText.text =
                    $"{unsupportedComputeShadersMessage}\n\n{descriptionText.text}";
                runButton.interactable = false;
            }
            else if (!sceneData.isUsingComputeShaders)
                runButton.interactable = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(descriptionText, Input.mousePosition, mainCamera);
            if (linkIndex == -1) return;

            TMP_LinkInfo linkInfo = descriptionText.textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }

        private void RunButtonOnClick() => RunButtonClicked?.Invoke(this, selectedSceneToRun);

        public delegate void OnRunButtonClicked(SceneInfo caller, SceneDataSO sceneData);
    }
}