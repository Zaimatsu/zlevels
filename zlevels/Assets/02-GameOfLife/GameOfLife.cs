using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

        [SerializeField] private Texture2D presetTexture;
        [SerializeField] private Image previewImage;

        #region Presets

        int[][] gliderPreset =
        {
            new[] {1, 0, 0},
            new[] {0, 1, 1},
            new[] {1, 1, 0}
        };

        int[][] hwssPreset =
        {
            new[] {0, 0, 1, 1, 0, 0, 0},
            new[] {1, 0, 0, 0, 0, 1, 0},
            new[] {0, 0, 0, 0, 0, 0, 1},
            new[] {1, 0, 0, 0, 0, 0, 1},
            new[] {0, 1, 1, 1, 1, 1, 1}
        };

        #endregion

        private List<int[][]> presets;
        private int selectedPreset;

        private void Start()
        {
            presets = new List<int[][]>
                {gliderPreset, hwssPreset};

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

            presetTexture = PresetToTexture(presets[selectedPreset]);
            previewImage.sprite = Sprite.Create(presetTexture, new Rect(0,0, presetTexture.width, presetTexture.height), new Vector2(0.5f, 0.5f));
        }

        private Texture2D PresetToTexture(int[][] preset)
        {
            int xSize = preset.Length;
            int ySize = preset.Select(ints => ints.Length).Prepend(Int32.MinValue).Max();

            var texture = new Texture2D(ySize, xSize, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            NativeArray<Color32> rawData = texture.GetRawTextureData<Color32>();
            for (var index = 0; index < rawData.Length; index++)
            {
                Debug.Log($"{index}: {xSize - 1 - index / ySize}, {index % ySize}");
                rawData[index] = preset[xSize - 1 - index / ySize][index % ySize] == 0
                    ? Color.black
                    : Color.white;
            }

            texture.Apply();

            return texture;
        }

        Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
        {
            Color32[] original = originalTexture.GetPixels32();
            var rotated = new Color32[original.Length];
            int w = originalTexture.width;
            int h = originalTexture.height;

            int iRotated, iOriginal;

            for (var j = 0; j < h; ++j)
            for (var i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }

            var rotatedTexture = new Texture2D(h, w);
            rotatedTexture.filterMode = originalTexture.filterMode;
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            return rotatedTexture;
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
                selectedPreset = (selectedPreset + 1) % presets.Count;
                presetTexture = PresetToTexture(presets[selectedPreset]);
                previewImage.sprite = Sprite.Create(presetTexture, new Rect(0,0, presetTexture.width, presetTexture.height), new Vector2(0.5f, 0.5f));
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                presetTexture = RotateTexture(presetTexture, true);
                previewImage.sprite = Sprite.Create(presetTexture, new Rect(0,0, presetTexture.width, presetTexture.height), new Vector2(0.5f, 0.5f));
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Vector3 worldMousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 textureMousePosition = new Vector2(worldMousePosition.x, worldMousePosition.y) + size / 2.0f;
                var x = (int) textureMousePosition.x;
                var y = (int) textureMousePosition.y;

                Graphics.CopyTexture(presetTexture, 0, 0, 0, 0, presetTexture.width, presetTexture.height,
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