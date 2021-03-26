using UnityEngine;
using UnityEngine.UI;

namespace ZLevels.Zoetropes
{
    public class Parameters : MonoBehaviour
    {
        [SerializeField] private ZoetropesGenerator zoetropesGenerator;
        [SerializeField] private ZoetropeRotation zoetropeRotation;

        [SerializeField] private Slider shapeCurveParameter1Slider;
        [SerializeField] private Slider shapeCurveParameter2Slider;
        [SerializeField] private Slider shapeCurveParameter3Slider;
        [SerializeField] private Slider shapeCurveParameter4Slider;
        [SerializeField] private Slider shapeCurveParameter5Slider;
        [SerializeField] private Slider partsSlider;
        [SerializeField] private Slider radiusSlider;
        [SerializeField] private Slider heightSlider;
        [SerializeField] private Slider rotationSlider;
        [SerializeField] private Slider speedSlider;

        private void Start()
        {
            shapeCurveParameter1Slider.onValueChanged.AddListener(ShapeCurveParameterSliderOnValueChanged);
            shapeCurveParameter2Slider.onValueChanged.AddListener(ShapeCurveParameterSliderOnValueChanged);
            shapeCurveParameter3Slider.onValueChanged.AddListener(ShapeCurveParameterSliderOnValueChanged);
            shapeCurveParameter4Slider.onValueChanged.AddListener(ShapeCurveParameterSliderOnValueChanged);
            shapeCurveParameter5Slider.onValueChanged.AddListener(ShapeCurveParameterSliderOnValueChanged);
            partsSlider.onValueChanged.AddListener(PartsSliderOnValueChanged);
            radiusSlider.onValueChanged.AddListener(RadiusSliderOnValueChanged);
            heightSlider.onValueChanged.AddListener(HeightSliderOnValueChanged);
            rotationSlider.onValueChanged.AddListener(RotationSliderOnValueChanged);
            speedSlider.onValueChanged.AddListener(SpeedSliderOnValueChanged);
        }

        private void PartsSliderOnValueChanged(float value)
        {
            zoetropesGenerator.Parts = (int) value;
            zoetropesGenerator.Generate();
        }

        private void ShapeCurveParameterSliderOnValueChanged(float value)
        {
            zoetropesGenerator.SetShapeCurve(shapeCurveParameter1Slider.value, shapeCurveParameter2Slider.value,
                shapeCurveParameter3Slider.value, shapeCurveParameter4Slider.value, shapeCurveParameter5Slider.value);
            zoetropesGenerator.Generate();
        }

        private void RadiusSliderOnValueChanged(float value)
        {
            zoetropesGenerator.Radius = value;
            zoetropesGenerator.Generate();
        }

        private void HeightSliderOnValueChanged(float value)
        {
            zoetropesGenerator.Height = value;
            zoetropesGenerator.Generate();
        }

        private void RotationSliderOnValueChanged(float value)
        {
            zoetropeRotation.Angle = value;
        }

        private void SpeedSliderOnValueChanged(float value)
        {
            zoetropeRotation.TimesPerSecond = value;
        }
    }
}