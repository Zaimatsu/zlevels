using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace ZLevels.GameOfLife
{
    [ScriptedImporter(1, "cells")]
    public class CellsImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            TextAsset subAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("text", subAsset);
            ctx.SetMainObject(subAsset);
        }
    }
}