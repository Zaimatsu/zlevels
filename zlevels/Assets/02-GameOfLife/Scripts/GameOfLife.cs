using System;
using UnityEngine;
using UnityEngine.UI;
using ZLevels.Utils;

/// TODO
/// 4. optional: blit texture and take into account live cells below?
namespace ZLevels.GameOfLife
{
    public class GameOfLife : MonoBehaviour
    {
        public event Action<GoLPatternTexture> SelectedPatternChanged;
        
        [SerializeField] private ComputeShader gameOfLifeComputeShader;
        [SerializeField] private ComputeShader randomWorldComputeShader;
        [SerializeField] private Vector2 size = new Vector2(1920, 1080);
        [SerializeField] private RawImage gameOfLifeRawImage;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private MouseCameraController mouseCameraController;
        [SerializeField] private GameObject ui;

        [field: SerializeField] public float Threshold { get; set; } = 0.5f;
        [field: SerializeField] public int Seed { get; set; }
        [SerializeField] private bool simulate;
        [field: SerializeField] public float TimesPerSecond { get; set; } = 60.0f;

        [SerializeField] private Image previewImage;

        public ListWalker<GoLPattern> PatternWalker { get; private set; }
        public GoLPatternTexture SelectedPresetTexture { get; private set; }
        private Timer.TimesPerSecondTimer perSecondTimer;
        private RenderTexture outputTexture;
        private RenderTexture bufferTexture;

        private void Awake()
        {
            perSecondTimer = Timer.TimesPerSecond(TimesPerSecond);
            perSecondTimer.Hit += UpdateSim;

            var patternsManager = new GoLPatternsManager(new GoLPatternsResourcesLoader().Load());
            PatternWalker = new ListWalker<GoLPattern>(patternsManager.Patterns);
            PatternWalker.Changed += PatternWalkerOnChanged;
            SetSelectedPatternTexture(new GoLPatternTexture(PatternWalker.Current));
            SelectedPresetTexture.Changed += SelectedPresetTextureOnChanged;

            outputTexture = new RenderTexture((int) size.x, (int) size.y, 24);
            outputTexture.enableRandomWrite = true;
            outputTexture.filterMode = FilterMode.Point;
            outputTexture.Create();

            bufferTexture = new RenderTexture((int) size.x, (int) size.y, 24);
            bufferTexture.enableRandomWrite = true;
            bufferTexture.filterMode = FilterMode.Point;
            bufferTexture.Create();

            gameOfLifeRawImage.texture = outputTexture;
            gameOfLifeRawImage.GetComponent<RectTransform>().sizeDelta = size;

            mouseCameraController.Initialize(size);
            
            GenerateRandomWorldGPU(Seed);

            gameOfLifeComputeShader.SetFloats("Resolution", outputTexture.width, outputTexture.height);
            gameOfLifeComputeShader.SetTexture(0, "Result", outputTexture);
            gameOfLifeComputeShader.SetTexture(0, "BufferTexture", bufferTexture);
        }

        private void SelectedPresetTextureOnChanged(GoLPatternTexture changedTexture)
        {
            SetSelectedPatternTexture(SelectedPresetTexture);
        }

        private void PatternWalkerOnChanged(GoLPattern currentPattern)
        {
            SetSelectedPatternTexture(new GoLPatternTexture(currentPattern));
        }

        private void Update()
        {
            perSecondTimer.TimesPerSecond = TimesPerSecond;

            if (simulate)
                perSecondTimer.Tick();

            if (Input.GetKeyDown(KeyCode.R)) GenerateRandomWorldGPU(Seed);
            if (Input.GetKeyDown(KeyCode.N)) PatternWalker.Next();
            if (Input.GetKeyDown(KeyCode.B)) PatternWalker.Previous();
            if (Input.GetKeyDown(KeyCode.H)) ui.SetActive(!ui.activeSelf);
            if (Input.GetMouseButtonDown(2)) SelectedPresetTexture.Rotate();

            if (Input.GetMouseButtonDown(1))
            {
                Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 textureMousePosition = new Vector2(worldMousePosition.x, worldMousePosition.y) + size / 2.0f;
                var x = (int) textureMousePosition.x;
                var y = (int) textureMousePosition.y;

                Graphics.CopyTexture(SelectedPresetTexture.Texture, 0, 0, 0, 0, SelectedPresetTexture.Texture.width,
                    SelectedPresetTexture.Texture.height,
                    outputTexture, 0, 0, x - SelectedPresetTexture.Texture.width / 2,
                    y - SelectedPresetTexture.Texture.height / 2);
            }
        }

        private void UpdateSim(Timer.TimesPerSecondTimer caller) => ProcessGameOfLife();

        public void GenerateRandomWorldGPU(int seed)
        {
            randomWorldComputeShader.SetFloat("Seed", seed);
            randomWorldComputeShader.SetFloat("Threshold", Threshold);
            randomWorldComputeShader.SetFloats("Resolution", outputTexture.width, outputTexture.height);
            randomWorldComputeShader.SetTexture(0, "Result", outputTexture);
            randomWorldComputeShader.Dispatch(0, outputTexture.width / 8, outputTexture.height / 8, 1);
        }

        [ContextMenu("ProcessGameOfLife")]
        public void ProcessGameOfLife()
        {
            gameOfLifeComputeShader.Dispatch(0, outputTexture.width / 8, outputTexture.height / 8, 1);
            Graphics.Blit(bufferTexture, outputTexture);
        }

        private void SetSelectedPatternTexture(GoLPatternTexture goLPatternTexture)
        {
            SelectedPresetTexture = goLPatternTexture;
            SelectedPresetTexture.Changed -= SelectedPresetTextureOnChanged;
            SelectedPresetTexture.Changed += SelectedPresetTextureOnChanged;
            SelectedPatternChanged?.Invoke(SelectedPresetTexture);
        }
    }
}