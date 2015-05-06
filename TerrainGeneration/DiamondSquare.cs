using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

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

        public TerrainData ToTerrainData(Vector3 cellSize)
        {
            var data = new TerrainData(new Vector3[Width * Height], Width, Height);

            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                    data[x, y] = new Vector3(cellSize.X * (float)x, cellSize.Y * this[x, y], cellSize.Z * (float)y);

            return data;
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

            // Square step
            // Create midpoint data
            for (int y = 0; y < baseHeights.Height - 1; ++y)
                for (int x = 0; x < baseHeights.Width - 1; ++x)
                {
                    var value = (baseHeights[x, y + 1] + baseHeights[x, y] + baseHeights[x + 1, y + 1] + baseHeights[x + 1, y]) / 4f;
                    value += (float)(random.NextDouble() * 2.0 - 1.0) * errorConstant * rectangleSize;
                    newMap[2 * x + 1, 2 * y + 1] = value;
                }

            // Diamond step
            for (int y = 0; y < newMap.Height - 1; ++y)
            {
                var dx = (y + 1) % 2;
                for (int x = 0; x < baseHeights.Width - 1; ++x)
                {
                    var sampleSum = 0f;
                    var xCoord = 2 * x + dx;
                    var sampleTotal = 0f;

                    // Take samples from the surrounding diamond
                    if (xCoord > 0)
                    {
                        sampleSum += 1f;
                        sampleTotal += newMap[xCoord - 1, y];
                    }
                    if (xCoord < newMap.Width - 1)
                    {
                        sampleSum += 1f;
                        sampleTotal += newMap[xCoord + 1, y];
                    }
                    if (y > 0)
                    {
                        sampleSum += 1f;
                        sampleTotal += newMap[xCoord, y - 1];
                    }
                    if (y < newMap.Height - 1)
                    {
                        sampleSum += 1f;
                        sampleTotal += newMap[xCoord, y + 1];
                    }

                    sampleTotal /= sampleSum;
                    sampleTotal += (float)(random.NextDouble() * 2.0 - 1.0) * errorConstant * rectangleSize;

                    newMap[xCoord, y] = sampleTotal;
                }
            }

            return newMap;
        }

        public static HeightMap GenerateIterative(HeightMap baseHeights, float errorConstant, int iterations)
        {
            var currentMap = baseHeights;
            var rectangleSize = (float)Math.Pow(2f, iterations);

            for (int i = 0; i < iterations; ++i)
            {
                currentMap = Generate(currentMap, errorConstant, rectangleSize);
                rectangleSize /= 2f;
            }

            return currentMap;
        }

        public static HeightMap GenerateRandom(float errorConstant, float maxSeedHeight, int iterations)
        {
            var random = new Random();

            var heightMap = new HeightMap(2, 2);
            heightMap[0, 0] = (float)random.NextDouble() * maxSeedHeight;
            heightMap[1, 0] = (float)random.NextDouble() * maxSeedHeight;
            heightMap[0, 1] = (float)random.NextDouble() * maxSeedHeight;
            heightMap[1, 1] = (float)random.NextDouble() * maxSeedHeight;

            return GenerateIterative(heightMap, errorConstant, iterations);
        }
    }
}
