using Godot;
using System;

namespace PerlinNoiseLib;

public partial class HeightMapTilemap : Node
{
    [Export]
    public TileMapLayer tileMapLayer;

    [Export] public int TileMapSize = 256;
    
    public override void _Ready()
    {
        var noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);
        noise.SetFrequency(0.02f);
        noise.SetFractalOctaves(4);
        noise.SetFractalLacunarity(2f);
        noise.SetFractalGain(0.5f);
        noise.SetSeed(983247);
        
        var cellHeightMap = new int[TileMapSize, TileMapSize];
        
        for (int i = 0; i < TileMapSize; i++)
        {
            for (int j = 0; j < TileMapSize; j++)
            {
                float noiseX = i;
                float noiseY = j;
                
                float v = noise.GetNoise2D(noiseX, noiseY);
                var normalizedElevation = Math.Abs((int)(v * 10));
                cellHeightMap[i, j] = normalizedElevation;
                
            }
        }
        
        CreateRiver(cellHeightMap, 15);
        CreateRiver(cellHeightMap, 5);
        CreateRiver(cellHeightMap, 3);
        
        DrawHeightMap(cellHeightMap);
        
    }
    
    private void DrawHeightMap(int[,] cellHeightMap)
    {
        for (int x = 0; x < TileMapSize; x++)
        {
            for (int y = 0; y < TileMapSize; y++)
            {
                var tilePosition = TileAtlasPositionByElevation(cellHeightMap[x, y]);
                tileMapLayer.SetCell(new Vector2I(x, y), 0, tilePosition);
            }
        }
    }

    private void CreateRiver(int[,] cellHeightMap, int riverWidth = 1)
    {
        int size = TileMapSize;
        int startX = GD.RandRange(0, size - 1);
        int y = 0; // Start at top edge

        while (y < size)
        {
            // Mark river cells with specified width
            for (int w = -riverWidth / 2; w <= riverWidth / 2; w++)
            {
                int rx = startX + w;
                if (rx >= 0 && rx < size)
                    cellHeightMap[rx, y] = -1;
            }

            // Find next step: look at down, down-left, down-right
            int nextX = startX;
            int nextY = y + 1;
            int minElevation = int.MaxValue;

            for (int dx = -1; dx <= 1; dx++)
            {
                int nx = startX + dx;
                int ny = y + 1;
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

            startX = nextX;
            y = nextY;
        }
        
        CreateRiverBed(cellHeightMap, riverWidth, bedWidth: 5, bedElevation: 0);
        // CreateStreams(cellHeightMap, streamCount: 10, streamWidth: 1);
    }
    
    private void CreateRiverBed(int[,] cellHeightMap, int riverWidth, int bedWidth, int bedElevation = 0)
    {
        int size = TileMapSize;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (cellHeightMap[x, y] == -1)
                {
                    for (int d = 1; d <= bedWidth; d++)
                    {
                        for (int dx = -d; dx <= d; dx++)
                        {
                            for (int dy = -d; dy <= d; dy++)
                            {
                                int nx = x + dx;
                                int ny = y + dy;
                                if (Math.Abs(dx) + Math.Abs(dy) == d && nx >= 0 && nx < size && ny >= 0 && ny < size)
                                {
                                    if (cellHeightMap[nx, ny] != -1)
                                    {
                                        int slopeElevation = bedElevation + d - 1;
                                        cellHeightMap[nx, ny] = Math.Min(cellHeightMap[nx, ny], slopeElevation);
                                    }
                                }
                            }
                        }
                    }
                }
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
            return new Vector2I(7, 15); // water
        
        return new Vector2I(elevation, 3);
    }
}
