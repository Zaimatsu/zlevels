using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

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

        private float maxOrthographicSize;
        private float orthographicSize;

        [SerializeField] private Image previewImage;

        private ListWalker<GoLPattern> listWalker;
        private GoLPattern selectedPreset;
        private GoLPatternTexture selectedPresetTexture;

        private void Start()
        {
            var patternsManager = new GoLPatternsManager(new GoLPatternsResourcesLoader().Load());
            listWalker = new ListWalker<GoLPattern>(patternsManager.Patterns);
            selectedPreset = listWalker.Current;
            selectedPresetTexture = new GoLPatternTexture(selectedPreset);

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

            previewImage.sprite = Sprite.Create(selectedPresetTexture.Texture,
                new Rect(0, 0, selectedPresetTexture.Texture.width, selectedPresetTexture.Texture.height),
                new Vector2(0.5f, 0.5f));
        }

        private void GenerateRandomWorld()
        {
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = outputTexture;
            var buf = new Texture2D((int) size.x, (int) size.y);
            buf.ReadPixels(new Rect(0, 0, (int) size.x, (int) size.y), 0, 0);
            for (var x = 0; x < buf.width; x++)
            for (var y = 0; y < buf.height; y++)
                buf.SetPixel(x, y, Random.value > 0.15f ? Color.black : Color.white);
            buf.Apply();
            Graphics.Blit(buf, outputTexture);
            RenderTexture.active = currentActiveRT;
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
            if (simulate)
                Dispatch();

            if (Input.GetKeyDown(KeyCode.R))
            {
                GenerateRandomWorldGPU(seed);
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                selectedPreset = listWalker.Next();
                selectedPresetTexture = new GoLPatternTexture(selectedPreset);
                previewImage.sprite = Sprite.Create(selectedPresetTexture.Texture,
                    new Rect(0, 0, selectedPresetTexture.Texture.width, selectedPresetTexture.Texture.height),
                    new Vector2(0.5f, 0.5f));
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                selectedPresetTexture.Rotate();
                previewImage.sprite = Sprite.Create(selectedPresetTexture.Texture,
                    new Rect(0, 0, selectedPresetTexture.Texture.width, selectedPresetTexture.Texture.height),
                    new Vector2(0.5f, 0.5f));
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Vector3 worldMousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 textureMousePosition = new Vector2(worldMousePosition.x, worldMousePosition.y) + size / 2.0f;
                var x = (int) textureMousePosition.x;
                var y = (int) textureMousePosition.y;

                Graphics.CopyTexture(selectedPresetTexture.Texture, 0, 0, 0, 0, selectedPresetTexture.Texture.width,
                    selectedPresetTexture.Texture.height,
                    outputTexture, 0, 0, x, y);
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
    }
}