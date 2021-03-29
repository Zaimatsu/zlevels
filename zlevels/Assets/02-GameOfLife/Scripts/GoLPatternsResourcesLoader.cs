using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZLevels.GameOfLife
{
    public class GoLPatternsResourcesLoader
    {
        GoLPatternCellsFiletypeFactory patternCellsFactory = new GoLPatternCellsFiletypeFactory();

        public List<GoLPattern> Load()
        {
            var res = Resources.LoadAll<TextAsset>("Patterns");
            return res.Select(textAsset => patternCellsFactory.Create(textAsset.text)).ToList();
        }
    }
}