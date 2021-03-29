using UnityEngine;
using UnityEngine.UI;
using ZLevels.Utils;

/// TODO
/// 4. optional: blit texture and take into account live cells below?
/// 5. refactor all this shit
/// 6. UI:
///     a) speed of sim value
///     b) random threshold value
///     c) generate button
///     d) clear board button
///     e) life pattern dropdown
///     f) selected life preview that takes into account rotation
///     g) rotate buttons
///     h) info about controls (left mouse click to pan, mouse scroll to zoom, right mouse click to put life pattern, other key shortcuts) 
namespace ZLevels.GameOfLife
{
    public class GameOfLife : MonoBehaviour
    {
        [SerializeField] private ComputeShader gameOfLifeComputeShader;
        [SerializeField] private ComputeShader randomWorldComputeShader;
        [SerializeField] private Vector2 size = new Vector2(1920, 1080);
        [SerializeField] private RawImage rawImage;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private MouseCameraController mouseCameraController;

        [SerializeField] private float threshold = 0.5f;
        [SerializeField] private int seed;
        [SerializeField] private bool simulate;
        [SerializeField] private float timesPerSecond = 60.0f;

        [SerializeField] private Image previewImage;

        private ListWalker<GoLPattern> listWalker;
        private GoLPatternTexture selectedPresetTexture;
        private Timer.TimesPerSecondTimer perSecondTimer;
        private RenderTexture outputTexture;
        private RenderTexture bufferTexture;

        private void Start()
        {
            perSecondTimer = Timer.TimesPerSecond(timesPerSecond);
            perSecondTimer.Hit += UpdateSim;

            var patternsManager = new GoLPatternsManager(new GoLPatternsResourcesLoader().Load());
            listWalker = new ListWalker<GoLPattern>(patternsManager.Patterns);
            SetSelectedPatternTexture(new GoLPatternTexture(listWalker.Current));

            outputTexture = new RenderTexture((int) size.x, (int) size.y, 24);
            outputTexture.enableRandomWrite = true;
            outputTexture.filterMode = FilterMode.Point;
            outputTexture.Create();

            bufferTexture = new RenderTexture((int) size.x, (int) size.y, 24);
            bufferTexture.enableRandomWrite = true;
            bufferTexture.filterMode = FilterMode.Point;
            bufferTexture.Create();

            rawImage.texture = outputTexture;
            rawImage.GetComponent<RectTransform>().sizeDelta = size;

            mouseCameraController.Initialize(size);
            
            GenerateRandomWorldGPU(seed);

            gameOfLifeComputeShader.SetFloats("Resolution", outputTexture.width, outputTexture.height);
            gameOfLifeComputeShader.SetTexture(0, "Result", outputTexture);
            gameOfLifeComputeShader.SetTexture(0, "BufferTexture", bufferTexture);
        }

        private void Update()
        {
            perSecondTimer.TimesPerSecond = timesPerSecond;

            if (simulate)
                perSecondTimer.Tick();

            if (Input.GetKeyDown(KeyCode.R))
            {
                GenerateRandomWorldGPU(seed);
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                SetSelectedPatternTexture(new GoLPatternTexture(listWalker.Next()));
            }

            if (Input.GetMouseButtonDown(2))
            {
                selectedPresetTexture.Rotate();
                SetSelectedPatternTexture(selectedPresetTexture);
            }

            if (Input.GetMouseButtonDown(1))
            {
                Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 textureMousePosition = new Vector2(worldMousePosition.x, worldMousePosition.y) + size / 2.0f;
                var x = (int) textureMousePosition.x;
                var y = (int) textureMousePosition.y;

                Graphics.CopyTexture(selectedPresetTexture.Texture, 0, 0, 0, 0, selectedPresetTexture.Texture.width,
                    selectedPresetTexture.Texture.height,
                    outputTexture, 0, 0, x - selectedPresetTexture.Texture.width / 2,
                    y - selectedPresetTexture.Texture.height / 2);
            }
        }

        private void UpdateSim(Timer.TimesPerSecondTimer caller) => ProcessGameOfLife();

        private void GenerateRandomWorldGPU(int seed)
        {
            randomWorldComputeShader.SetFloat("Seed", seed);
            randomWorldComputeShader.SetFloat("Threshold", threshold);
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
            selectedPresetTexture = goLPatternTexture;
            previewImage.sprite = Sprite.Create(selectedPresetTexture.Texture,
                new Rect(0, 0, selectedPresetTexture.Texture.width, selectedPresetTexture.Texture.height),
                new Vector2(0.5f, 0.5f));
        }
    }
}