using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZLevels.GameOfLife
{
    public class GoLPatternCellsFiletypeFactory
    {
        public GoLPattern Create(string cellsDefinition)
        {
            string[] lines = cellsDefinition.Split(
                new[] {Environment.NewLine},
                StringSplitOptions.None
            );

            string name = lines[0].Split(':')[1].Trim();
            string description = lines[1].Replace("!", "");
            var i = 2;
            while (lines[i].StartsWith("!"))
                description += Environment.NewLine + lines[i++].Replace("!", "");

            int xSize = lines.Skip(i).Max(line => line.Length);
            ushort ySize = 0;
            var patternDefinition = new List<bool>();
            for (; i < lines.Length; i++)
            {
                ySize++;
                for (var i1 = 0; i1 < lines[i].Length; i1++)
                {
                    patternDefinition.Add(char.ToLower(lines[i][i1]) == 'o');
                }

                for (int i2 = xSize - lines[i].Length; i2 > 0; i2--)
                    patternDefinition.Add(false);
            }

            var bitArray = new BitArray(patternDefinition.ToArray());

            return new GoLPattern(name, description, bitArray, (ushort) xSize, ySize);
        }
    }
}