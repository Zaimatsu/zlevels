using System.Collections.Generic;

namespace ZLevels.GameOfLife
{
    public class GoLPatternsManager
    {
        public List<GoLPattern> Patterns { get; }

        public GoLPatternsManager(List<GoLPattern> patterns)
        {
            Patterns = patterns;
        }
    }
}