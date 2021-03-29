using System.Collections;

namespace ZLevels.GameOfLife
{
    public class GoLPattern
    {
        public string Name { get; }
        public string Description { get; }
        public BitArray Data { get; }
        public ushort SizeX { get; }
        public ushort SizeY { get; }

        public GoLPattern(string name, string description, BitArray data, ushort sizeX, ushort sizeY)
        {
            Name = name;
            Description = description;
            Data = data;
            SizeX = sizeX;
            SizeY = sizeY;
        }

        
        
        public bool IsAlive(int x, int y)
        {
            return Data[x + y * SizeY];
        }
    }
}