using Godot;
using System;

namespace TerrainGenerator.Scripts;

public partial class TerrainGeneratorByNoise : Node
{
    [Export]
    public TileMapLayer TileMapLayer;

    [Export] public int TileMapSize = 1024;
    
    public override void _Ready()
    {
        var cellHeightMap = GenerateHeightMap(); 
        
        // create these in order from smallest to largest width so that larger rivers can overwrite smaller ones
        CreateRiver(cellHeightMap, 3, RiverDirection.LeftToRight);
        CreateRiver(cellHeightMap, 3, RiverDirection.LeftToRight);
        CreateRiver(cellHeightMap, 5);
        CreateRiver(cellHeightMap, 15, RiverDirection.LeftToRight);
        
        DrawHeightMap(cellHeightMap);
    }

    private int[,] GenerateHeightMap()
    {
        // notes on parameters:
        // - frequency controls the "zoom" level of the noise
        // - fractal octaves add layers of detail
        // - lacunarity controls the frequency multiplier for each octave
        // - gain controls the amplitude multiplier for each octave
        // - seed ensures reproducibility
        
        // notes on noise types:
        // - ValueCubic: Smooth and continuous, good for terrain
        // - Perlin: Classic smooth noise, good for natural patterns
        // - Simplex: More complex, can produce more varied patterns
        // - Cellular: Produces cell-like patterns, good for caves or islands
        // - Value: Basic noise, can be more abrupt
        
        var noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Simplex);
        noise.SetFrequency(0.02f);
        noise.SetFractalOctaves(4);
        noise.SetFractalLacunarity(2f);
        noise.SetFractalGain(0.5f);
        noise.SetSeed((int)GD.Randi());
        // noise.SetFrequency(0.02f);
        // noise.SetFractalOctaves(4);
        // noise.SetFractalLacunarity(2f);
        // noise.SetFractalGain(0.5f);
        // noise.SetSeed(42);
        
        var cellHeightMap = new int[TileMapSize, TileMapSize];
        
        for (int i = 0; i < TileMapSize; i++)
        {
            for (int j = 0; j < TileMapSize; j++)
            {
                float noiseX = i;
                float noiseY = j;
                
                float v = noise.GetNoise2D(noiseX, noiseY);
                // var normalizedElevation = Math.Abs((int)(v * 10));
                if (v < 0) v += 0.3f; // shove the noise up a bit so more land is above water
                var normalizedElevation = (int)(v * 7);
                cellHeightMap[i, j] = normalizedElevation;
            }
        }

        return cellHeightMap;
    }
    
    private void DrawHeightMap(int[,] cellHeightMap)
    {
        for (int x = 0; x < TileMapSize; x++)
        {
            for (int y = 0; y < TileMapSize; y++)
            {
                var tilePosition = TileAtlasPositionByElevation(cellHeightMap[x, y]);
                TileMapLayer.SetCell(new Vector2I(x, y), 0, tilePosition);
            }
        }
    }

    public enum RiverDirection
    {
        TopToBottom,
        LeftToRight
    }

    private void CreateRiver(int[,] cellHeightMap, int riverWidth = 1, RiverDirection direction = RiverDirection.TopToBottom)
    {
        int size = TileMapSize;
        int x, y;

        if (direction == RiverDirection.TopToBottom)
        {
            // Start river at a random x on the top edge
            x = GD.RandRange((int)(size * .50f) , (int)(size * 0.80));
            y = 0;
            while (y < size)
            {
                // Mark river cells with specified width
                for (int w = -riverWidth / 2; w <= riverWidth / 2; w++)
                {
                    int rx = x + w;
                    if (rx >= 0 && rx < size)
                    {
                        int distanceFromCenter = Math.Abs(w);
                        cellHeightMap[rx, y] = -1 - ((riverWidth / 2) - distanceFromCenter); // Deepest at center
                    }
                }

                // Find next x position with lowest elevation in the next row
                int nextX = x, nextY = y + 1, minElevation = int.MaxValue;
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, ny = y + 1;
                    if (nx >= 0 && nx < size && ny < size)
                    {
                        int elevation = cellHeightMap[nx, ny];
                        if (elevation < minElevation)
                        {
                            minElevation = elevation;
                            nextX = nx;
                        }
                    }
                }
                x = nextX;
                y = nextY;
            }
        }
        else // LeftToRight
        {
            // Start river at a random y on the left edge
            x = 0;
            y = GD.RandRange((int)(size * .50), (int)(size * 0.80));
            while (x < size)
            {
                // Mark river cells with specified width
                for (int w = -riverWidth / 2; w <= riverWidth / 2; w++)
                {
                    int ry = y + w;
                    if (ry >= 0 && ry < size)
                    {
                        int distanceFromCenter = Math.Abs(w);
                        cellHeightMap[x, ry] = -1 - ((riverWidth / 2) - distanceFromCenter); // Deepest at center
                    }
                }

                // Find next y position with lowest elevation in the next column
                int nextY = y, nextX = x + 1, minElevation = int.MaxValue;
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + 1, ny = y + dy;
                    if (nx < size && ny >= 0 && ny < size)
                    {
                        int elevation = cellHeightMap[nx, ny];
                        if (elevation < minElevation)
                        {
                            minElevation = elevation;
                            nextY = ny;
                        }
                    }
                }
                x = nextX;
                y = nextY;
            }
        }
    }
    
    private int NormalizeElevation(float rawElevation, float minElevation, float maxElevation, int discreteLevels)
    {
        float normalized = (rawElevation - minElevation) / (maxElevation - minElevation);
        int discreteElevation = (int)(normalized * (discreteLevels - 1));
        return Mathf.Clamp(discreteElevation, 0, discreteLevels - 1);
    }
    
    Vector2I TileAtlasPositionByElevation(int elevation)
    {
        if (elevation < 0)
        {
            return new Vector2I(Math.Abs(elevation), 4); // water tiles start 1 tile from left
        }
        
        return new Vector2I(elevation, 3);
    }
}
