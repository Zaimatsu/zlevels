using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ZLevels.GameOfLife
{
    public static class GameOfLifeExtensions
    {
        public static GameOfLife.PresetDirection Next(this GameOfLife.PresetDirection presetDirection)
        {
            return presetDirection switch
            {
                GameOfLife.PresetDirection.Right => GameOfLife.PresetDirection.Down,
                GameOfLife.PresetDirection.Down => GameOfLife.PresetDirection.Left,
                GameOfLife.PresetDirection.Left => GameOfLife.PresetDirection.Up,
                GameOfLife.PresetDirection.Up => GameOfLife.PresetDirection.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(presetDirection), presetDirection, null)
            };
        }
    }

    public class GameOfLife : MonoBehaviour
    {
        [SerializeField] private ComputeShader gameOfLifeComputeShader;
        [SerializeField] private ComputeShader randomWorldComputeShader;
        [SerializeField] private ComputeShader lifePresetsComputeShader;
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

        #region Presets

        int[] gliderURPreset =
        {
            0, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1, 1, 0,
            0, 1, 1, 0, 0,
            0, 0, 0, 0, 0
        };

        int[] hwssRPreset =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 1, 1, 0, 0, 0, 0,
            0, 1, 0, 0, 0, 0, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 1, 0,
            0, 1, 0, 0, 0, 0, 0, 1, 0,
            0, 0, 1, 1, 1, 1, 1, 1, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0,
        };

        #endregion

        private List<int[]> presets;
        private int selectedPreset;
        [SerializeField] private PresetDirection presetDirection;

        private void Start()
        {
            presets = new List<int[]>
                {gliderURPreset, hwssRPreset};

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
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                presetDirection = presetDirection.Next();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Vector3 worldMousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 textureMousePosition = new Vector2(worldMousePosition.x, worldMousePosition.y) + size / 2.0f;
                var x = (int) textureMousePosition.x;
                var y = (int) textureMousePosition.y;

                int[] presetData = presets[selectedPreset];
                int[] presetToSet = PresetToSet(presetData, presetDirection);
                lifePresetsComputeShader.SetInts("Preset", presetToSet);
                lifePresetsComputeShader.SetInts("PresetLength", presetData.Length);
                //lifePresetsComputeShader.SetInt("PresetMask", presetMask);
                lifePresetsComputeShader.SetInts("Position", x, y);
                lifePresetsComputeShader.SetFloats("Resolution", outputTexture.width, outputTexture.height);
                lifePresetsComputeShader.SetTexture(0, "Result", outputTexture);
                lifePresetsComputeShader.Dispatch(0, outputTexture.width / 8, outputTexture.height / 8, 1);
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

        private static int[] PresetToSet(int[] presetData, PresetDirection presetDirection)
        {
            var rotatedPresetData = new int[presetData.Length];

            int side = (int) Mathf.Sqrt(presetData.Length);
            switch (presetDirection)
            {
                case PresetDirection.Right:
                    rotatedPresetData = presetData;
                    break;
                case PresetDirection.Down:
                    for (var x = 0; x < side; x++) 
                    for (var y = 0; y < side; y++)
                        rotatedPresetData[x + y * side] = presetData[(side - x) * side - 1 - y];
                    break;
                case PresetDirection.Left:
                    for (var x = 0; x < side; x++) 
                    for (var y = 0; y < side; y++)
                        rotatedPresetData[x + y * side] = presetData[side - 1 - x + y * side];
                    break;
                case PresetDirection.Up:
                    for (var x = 0; x < side; x++) 
                    for (var y = 0; y < side; y++)
                        rotatedPresetData[x + y * side] = presetData[(side - x - 1) * side + y];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(presetDirection), presetDirection, null);
            }

            var presetToSet = new int[4 * rotatedPresetData.Length];
            for (var i = 0; i < rotatedPresetData.Length; i++)
            {
                presetToSet[i * 4] = rotatedPresetData[i];
            }

            return presetToSet;
        }

        public enum PresetDirection
        {
            Right,
            Down,
            Left,
            Up
        }
    }
}