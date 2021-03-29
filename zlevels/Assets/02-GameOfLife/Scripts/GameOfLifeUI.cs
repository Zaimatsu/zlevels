using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZLevels.GameOfLife
{
    // ToDo seed
    public class GameOfLifeUI : MonoBehaviour
    {
        [SerializeField] private GameOfLife gameOfLife;
        [SerializeField] private TMP_Text patternName;
        [SerializeField] private TMP_Text patternDescription;
        [SerializeField] private Image patternPreview;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button rotateButton;
        [SerializeField] private Slider simSpeedSlider;
        [SerializeField] private Slider densitySlider;
        [SerializeField] private Button generateButton;

        private void Start()
        {
            GoLPattern currentPattern = gameOfLife.PatternWalker.Current;
            patternName.text = currentPattern.Name;
            patternDescription.text = currentPattern.Description;
            GameOfLifeOnSelectedPatternChanged(gameOfLife.SelectedPresetTexture);

            gameOfLife.SelectedPatternChanged += GameOfLifeOnSelectedPatternChanged;
            nextButton.onClick.AddListener(NextButtonOnClick);
            previousButton.onClick.AddListener(PreviousButtonOnClick);
            rotateButton.onClick.AddListener(RotateButtonOnClick);

            simSpeedSlider.onValueChanged.AddListener(SimSpeedSliderOnChanged);
            densitySlider.onValueChanged.AddListener(DensitySliderOnChanged);
            generateButton.onClick.AddListener(GenerateButtonOnClick);

            gameOfLife.PatternWalker.Changed += PatternWalkerOnChanged;
        }

        private void PatternWalkerOnChanged(GoLPattern current)
        {
            patternName.text = current.Name;
            patternDescription.text = current.Description;
        }

        private void GenerateButtonOnClick() => gameOfLife.GenerateRandomWorldGPU(Random.Range(0, 100));
        private void DensitySliderOnChanged(float value) => gameOfLife.Threshold = 1.0f - value;
        private void SimSpeedSliderOnChanged(float value) => gameOfLife.TimesPerSecond = value;
        private void RotateButtonOnClick() => gameOfLife.SelectedPresetTexture.Rotate();
        private void PreviousButtonOnClick() => gameOfLife.PatternWalker.Previous();
        private void NextButtonOnClick() => gameOfLife.PatternWalker.Next();


        private void GameOfLifeOnSelectedPatternChanged(GoLPatternTexture patternTexture)
        {
            patternPreview.sprite = Sprite.Create(patternTexture.Texture,
                new Rect(0, 0, patternTexture.Texture.width, patternTexture.Texture.height),
                new Vector2(0.5f, 0.5f));
        }
    }
}