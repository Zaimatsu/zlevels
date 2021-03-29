using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZLevels
{
    public class SliderUI : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private GameObject textGameObject;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private EventTrigger eventTrigger;

        public void Start()
        {
            slider.onValueChanged.AddListener(SliderOnValueChanged);

            var beginDragEntry = new EventTrigger.Entry {eventID = EventTriggerType.BeginDrag};
            beginDragEntry.callback.AddListener((data) => { OnBeginDragDelegate((PointerEventData) data); });
            eventTrigger.triggers.Add(beginDragEntry);
            var endDragEntry = new EventTrigger.Entry {eventID = EventTriggerType.EndDrag};
            endDragEntry.callback.AddListener((data) => { OnEndDragDelegate((PointerEventData) data); });
            eventTrigger.triggers.Add(endDragEntry);
            textGameObject.SetActive(false);
        }

        private void OnBeginDragDelegate(PointerEventData _) => textGameObject.SetActive(true);
        private void OnEndDragDelegate(PointerEventData _) => textGameObject.SetActive(false);

        private void SliderOnValueChanged(float value)
        {
            valueText.text = value.ToString(slider.wholeNumbers ? "0" : "0.00");
        }
    }
}