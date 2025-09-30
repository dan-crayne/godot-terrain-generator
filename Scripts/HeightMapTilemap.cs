using Godot;
using System;

namespace PerlinNoiseLib;

public partial class HeightMapTilemap : Node
{
    public override void _Ready()
    {
        
        
        
        
        
        // int width = 256;
        // int height = 256;
        // var heights = PerlinNoise.GenerateHeightMap(width, height, scale: 100f, octaves: 4, persistence: 0.5f, lacunarity: 2f, seed: 12345);
        //
        // float minElevation = 0f;
        // float maxElevation = 10f;
        // float elevation = maxElevation - minElevation;
        //
        // var discreteHeights = new int[width, height];
        // for (int y = 0; y < height; y++)
        // {
        //     for (int x = 0; x < width; x++)
        //     {
        //         discreteHeights[x, y] = (int)Mathf.Clamp((int)(heights[x, y] * elevation), minElevation, maxElevation);
        //     }
        // }
        

        // Create a grayscale image to preview
        // var img = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        // for (int y = 0; y < height; y++)
        // {
        //     for (int x = 0; x < width; x++)
        //     {
        //         float v = heights[x, y];
        //         // map to 0..255
        //         byte b = (byte)Mathf.Clamp((int)(v * 255f), 0, 255);
        //         img.SetPixel(x, y, new Color(b / 255.0f, b / 255.0f, b / 255.0f));
        //     }
        // }
        //
        // ImageTexture tex = ImageTexture.CreateFromImage(img);
        // // You can now assign `tex` to a TextureRect, sprite, or use it to generate collision/tiles.
        // var sprite = new Sprite2D();
        // sprite.Texture = tex;
        // AddChild(sprite);
    }
    
    void CreateTileMapImage()
    {}
    
    
    
    string GetTileForElevation(int elevation)
    {
        if (elevation <= 2) return "Water";
        if (elevation <= 5) return "Flat";
        if (elevation <= 8) return "Grass";
        return "Mountain";
    }   
    
    Vector2I TileAtlasPosition(string tileName)
    {
        return tileName switch
        {
            "Water" => new Vector2I(0, 0),
            "Grass" => new Vector2I(1, 0),
            "Sand" => new Vector2I(2, 0),
            "Mountain" => new Vector2I(0, 1),
            _ => new Vector2I(1, 0),
        };
    }

    Vector2I TileAtlasPositionByElevation(int elevation)
    {
        return new Vector2I(elevation, 3);
    }
}
