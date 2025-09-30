using System;

namespace PerlinNoiseLib;

public class PerlinTestConsole
{
    public static void Main()
    {
        int w = 16;
        int h = 8;
        var map = PerlinNoise.GenerateHeightMap(w, h, scale: 20f, octaves: 4, persistence: 0.5f, lacunarity: 2f, seed: 42);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Print as ASCII shades
                float v = map[x, y];
                char c = v < 0.2f ? '.' : v < 0.4f ? ',' : v < 0.6f ? '-' : v < 0.8f ? '=' : '#';
                Console.Write(c);
            }
            Console.WriteLine();
        }
    }
}
