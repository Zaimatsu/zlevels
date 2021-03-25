using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZLevels.Hub
{
    public class SceneListElement : MonoBehaviour
    {
        public static event OnClicked Clicked;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button button;

        public SceneDataSO Data { get; private set; }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(ButtonOnClick);
        }

        public void Initialize(SceneDataSO sceneDataSo)
        {
            Data = sceneDataSo;
            titleText.text = sceneDataSo.Name;
            button.onClick.RemoveListener(ButtonOnClick);
            button.onClick.AddListener(ButtonOnClick);
        }

        private void ButtonOnClick() => Clicked?.Invoke(this);

        public delegate void OnClicked(SceneListElement caller);
    }
}