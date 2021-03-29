using System;
using Unity.Collections;
using UnityEngine;

namespace ZLevels.GameOfLife
{
    public class GoLPatternTexture
    {
        public event Action<GoLPatternTexture> Changed;
        
        public Texture2D Texture { get; private set; }

        public GoLPatternTexture(GoLPattern goLPattern)
        {
            Texture = new Texture2D(goLPattern.SizeX, goLPattern.SizeY, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            NativeArray<Color32> rawData = Texture.GetRawTextureData<Color32>();

            for (var y = 0; y < goLPattern.SizeY; y++)
            for (var x = 0; x < goLPattern.SizeX; x++)
            {
                int index = x + y * goLPattern.SizeX;
                rawData[index] = goLPattern.Data[index]
                    ? Color.white
                    : Color.black;
            }

            Texture.Apply();
        }

        public void Rotate(bool clockwise = true)
        {
            Texture = Texture.Rotate(clockwise);
            Changed?.Invoke(this);
        }
    }
}