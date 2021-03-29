using UnityEngine;

namespace ZLevels.GameOfLife
{
    public static class Texture2DExtensions
    {
        public static Texture2D Rotate(this Texture2D originalTexture, bool clockwise = true)
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
    }
}