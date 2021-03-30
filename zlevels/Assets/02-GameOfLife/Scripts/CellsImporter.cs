#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor.AssetImporters;

namespace ZLevels.GameOfLife
{
    [ScriptedImporter(1, "cells")]
    public class CellsImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var subAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("text", subAsset);
            ctx.SetMainObject(subAsset);
        }
    }
}
#endif