using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrainGeneration
{
    public struct HeightMap
    {
        public float[] Data;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public HeightMap(int width, int height)
            : this()
        {
            Data = new float[width * height];

            Width = width;
            Height = height;
        }

        public float this[int x, int y]
        {
            get { return Data[x + y * Width]; }
            set { Data[x + y * Width] = value; }
        }
    }

    public static class DiamondSquare
    {
        public static HeightMap Generate(HeightMap baseHeights, float errorConstant, float rectangleSize)
        {
            var random = new Random();
            var newWidth = baseHeights.Width * 2 - 1;
            var newHeight = baseHeights.Height * 2 - 1;

            // Create a new height map with the appropriate dimensions
            var newMap = new HeightMap(newWidth, newHeight);

            // Copy over original data
            for (int y = 0; y < baseHeights.Height; ++y)
                for (int x = 0; x < baseHeights.Width; ++x)
                    newMap[2 * x, 2 * y] = baseHeights[x, y];

            // Create horizontal data
            for (int y = 0; y < baseHeights.Height; ++y)
                for (int x = 0; x < baseHeights.Width - 1; ++x)
                {
                    var value = (baseHeights[x, y] + baseHeights[x + 1, y]) / 2f;
                    value += (float)(random.Next() * 2.0 - 1.0) * errorConstant * rectangleSize;
                    newMap[2 * x + 1, 2 * y] = value;
                }

            // Create vertical data
            for (int y = 0; y < baseHeights.Height - 1; ++y)
                for (int x = 0; x < baseHeights.Width; ++x)
                {
                    var value = (baseHeights[x, y + 1] + baseHeights[x, y]) / 2f;
                    value += (float)(random.Next() * 2.0 - 1.0) * errorConstant * rectangleSize;
                    newMap[2 * x, 2 * y + 1] = value;
                }

            // Create midpoint data
            for (int y = 0; y < baseHeights.Height - 1; ++y)
                for (int x = 0; x < baseHeights.Width - 1; ++x)
                {
                    var value = (baseHeights[x, y + 1] + baseHeights[x, y] + baseHeights[x + 1, y + 1] + baseHeights[x + 1, y]) / 4f;
                    value += (float)(random.Next() * 2.0 - 1.0) * errorConstant * rectangleSize;
                    newMap[2 * x + 1, 2 * y + 1] = value;
                }

            return newMap;
        }

        public static HeightMap GenerateIterative(HeightMap baseHeights, float errorConstant, int iterations)
        {
            var currentMap = baseHeights;
            var rectangleSize = 1f;

            for (int i = 0; i < iterations; ++i)
            {
                currentMap = Generate(currentMap, errorConstant, rectangleSize);
                rectangleSize /= 4f;
            }

            return currentMap;
        }

        public static HeightMap GenerateRandom(float errorConstant, float maxSeedHeight, int iterations)
        {
            var random = new Random();

            var heightMap = new HeightMap(2, 2);
            heightMap[0, 0] = (float)random.Next() * maxSeedHeight;
            heightMap[1, 0] = (float)random.Next() * maxSeedHeight;
            heightMap[0, 1] = (float)random.Next() * maxSeedHeight;
            heightMap[1, 1] = (float)random.Next() * maxSeedHeight;

            return GenerateIterative(heightMap, errorConstant, iterations);
        }
    }
}
