using Cinemachine;
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
        [SerializeField] private RenderTexture outputTexture;
        [SerializeField] private RenderTexture bufferTexture;
        [SerializeField] private Vector2 size = new Vector2(1920, 1080);
        [SerializeField] private RawImage rawImage;
        [SerializeField] private PolygonCollider2D polygonCollider2D;
        [SerializeField] private CinemachineConfiner cinemachineConfiner;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private Camera camera;
        [SerializeField] private Transform cameraFollowTarget;
        [SerializeField] private float zoomSpeed = 500.0f;
        [SerializeField] private float threshold = 0.5f;
        [SerializeField] private int seed;
        [SerializeField] private bool simulate;
        [SerializeField] private float timesPerSecond = 60.0f;

        private float maxOrthographicSize;
        private float orthographicSize;

        [SerializeField] private Image previewImage;

        private ListWalker<GoLPattern> listWalker;
        private GoLPatternTexture selectedPresetTexture;
        private Timer.TimesPerSecondTimer perSecondTimer;

        private void Start()
        {
            perSecondTimer = Timer.TimesPerSecond(timesPerSecond);
            perSecondTimer.Hit += UpdateSim;
            
            var patternsManager = new GoLPatternsManager(new GoLPatternsResourcesLoader().Load());
            listWalker = new ListWalker<GoLPattern>(patternsManager.Patterns);
            SetSelectedPatternTexture(new GoLPatternTexture(listWalker.Current));

            outputTexture.Release();
            outputTexture.width = (int) size.x;
            outputTexture.height = (int) size.y;
            outputTexture.enableRandomWrite = true;
            outputTexture.filterMode = FilterMode.Point;
            outputTexture.Create();

            rawImage.GetComponent<RectTransform>().sizeDelta = size;
            polygonCollider2D.SetPath(0, new[]
            {
                new Vector2(-size.x / 2, -size.y / 2),
                new Vector2(-size.x / 2, size.y / 2),
                new Vector2(size.x / 2, size.y / 2),
                new Vector2(size.x / 2, -size.y / 2)
            });
            cinemachineConfiner.InvalidatePathCache();
            orthographicSize = maxOrthographicSize = Mathf.Min(size.x, size.y) / 2.0f;
            virtualCamera.m_Lens.OrthographicSize = orthographicSize;

            bufferTexture = new RenderTexture((int) size.x, (int) size.y, 24);
            bufferTexture.enableRandomWrite = true;
            bufferTexture.filterMode = FilterMode.Point;
            bufferTexture.Create();

            GenerateRandomWorldGPU(seed);

            gameOfLifeComputeShader.SetFloats("Resolution", outputTexture.width, outputTexture.height);
            gameOfLifeComputeShader.SetTexture(0, "Result", outputTexture);
            gameOfLifeComputeShader.SetTexture(0, "BufferTexture", bufferTexture);
        }

        private void UpdateSim(Timer.TimesPerSecondTimer caller)
        {
            Dispatch();
        }

        private void GenerateRandomWorldGPU(int seed)
        {
            randomWorldComputeShader.SetFloat("Seed", seed);
            randomWorldComputeShader.SetFloat("Threshold", threshold);
            randomWorldComputeShader.SetFloats("Resolution", outputTexture.width, outputTexture.height);
            randomWorldComputeShader.SetTexture(0, "Result", outputTexture);
            randomWorldComputeShader.Dispatch(0, outputTexture.width / 8, outputTexture.height / 8, 1);
        }

        [ContextMenu("Dispatch")]
        public void Dispatch()
        {
            gameOfLifeComputeShader.Dispatch(0, outputTexture.width / 8, outputTexture.height / 8, 1);
            Graphics.Blit(bufferTexture, outputTexture);
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
                Vector3 worldMousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 textureMousePosition = new Vector2(worldMousePosition.x, worldMousePosition.y) + size / 2.0f;
                var x = (int) textureMousePosition.x;
                var y = (int) textureMousePosition.y;

                Graphics.CopyTexture(selectedPresetTexture.Texture, 0, 0, 0, 0, selectedPresetTexture.Texture.width,
                    selectedPresetTexture.Texture.height,
                    outputTexture, 0, 0, x - selectedPresetTexture.Texture.width/2, y - selectedPresetTexture.Texture.height/2);
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 worldMousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
                cameraFollowTarget.position = new Vector3(worldMousePosition.x, worldMousePosition.y, 0);
            }

            orthographicSize = Mathf.Clamp(orthographicSize - Input.mouseScrollDelta.y * Time.deltaTime * zoomSpeed, 10,
                maxOrthographicSize);
            virtualCamera.m_Lens.OrthographicSize = orthographicSize;
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