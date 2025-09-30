using System;

namespace PerlinNoiseLib;
    
public static class PerlinNoise
{
    // Generates a height map of size width x height with values in [0,1].
    public static float[,] GenerateHeightMap(int width, int height, float scale = 50f,
                                            int octaves = 4, float persistence = 0.5f, float lacunarity = 2f,
                                            int seed = 0, float offsetX = 0f, float offsetY = 0f)
    {
        if (scale <= 0) scale = 0.0001f;

        float[,] heights = new float[width, height];

        // Create permutation table from seed
        int[] perm = BuildPermutationTable(seed);

        // Precompute octave offsets so maps differ per octave and are repeatable via seed
        Random rng = new Random(seed);
        (float x, float y)[] octaveOffsets = new (float, float)[octaves];
        for (int i = 0; i < octaves; i++)
        {
            // large range reduces correlation artifacts
            float offsetOctX = (float)(rng.NextDouble() * 100000 - 50000) + offsetX;
            float offsetOctY = (float)(rng.NextDouble() * 100000 - 50000) + offsetY;
            octaveOffsets[i] = (offsetOctX, offsetOctY);
        }

        float maxPossibleHeight = 0f;
        float amplitude = 1f;
        for (int i = 0; i < octaves; i++)
        {
            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        // center coordinates so zooming is centered on map
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float frequency = 1f;
                float amp = 1f;
                float noiseHeight = 0f;

                for (int o = 0; o < octaves; o++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[o].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[o].y;

                    // Perlin noise returns approximately in range [-1,1]
                    float perlinVal = Perlin(sampleX, sampleY, perm);
                    noiseHeight += perlinVal * amp;

                    amp *= persistence;
                    frequency *= lacunarity;
                }

                // Normalize to [0,1] using theoretical maxPossibleHeight
                // (noiseHeight in roughly [-maxPossibleHeight, maxPossibleHeight])
                heights[x, y] = (noiseHeight + maxPossibleHeight) / (2f * maxPossibleHeight);
                // clamp numerical edge cases
                if (heights[x, y] < 0f) heights[x, y] = 0f;
                if (heights[x, y] > 1f) heights[x, y] = 1f;
            }
        }

        return heights;
    }

    // --- PERLIN IMPLEMENTATION (Improved Perlin) ---
    private static int[] BuildPermutationTable(int seed)
    {
        int[] p = new int[256];
        for (int i = 0; i < 256; i++) p[i] = i;

        Random rnd = new Random(seed);
        for (int i = 255; i > 0; i--)
        {
            int swap = rnd.Next(i + 1);
            int tmp = p[i];
            p[i] = p[swap];
            p[swap] = tmp;
        }

        // duplicate the permutation table to avoid overflow
        int[] perm = new int[512];
        for (int i = 0; i < 512; i++) perm[i] = p[i & 255];
        return perm;
    }

    // Fade function as defined by Ken Perlin
    private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);

    private static float Lerp(float a, float b, float t) => a + t * (b - a);

    // Gradient function -- convert hashed value to gradient and compute dot product
    private static float Grad(int hash, float x, float y)
    {
        // Convert low 4 bits of hash code into 12 gradient directions (2D)
        int h = hash & 7; // 0..7
        float u = (h < 4) ? x : y;
        float v = (h < 4) ? y : x;
        return (((h & 1) == 0) ? u : -u) + (((h & 2) == 0) ? v : -v);
    }

    // 2D Perlin noise
    // Returns in range approx [-1, 1]
    private static float Perlin(float x, float y, int[] perm)
    {
        int xi = FastFloor(x) & 255;
        int yi = FastFloor(y) & 255;

        float xf = x - FastFloor(x);
        float yf = y - FastFloor(y);

        float u = Fade(xf);
        float v = Fade(yf);

        int aa = perm[perm[xi] + yi];
        int ab = perm[perm[xi] + yi + 1];
        int ba = perm[perm[xi + 1] + yi];
        int bb = perm[perm[xi + 1] + yi + 1];

        float x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
        float x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);
        float result = Lerp(x1, x2, v);

        // result is roughly in [-1,1] but not exactly; leave as-is for octave stacking
        return result;
    }

    private static int FastFloor(float x) => (x > 0) ? (int)x : (int)x - 1;
}
