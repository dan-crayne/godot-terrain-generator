using Godot;
using System;

namespace PerlinNoiseLib;

public partial class HeightMapImage : Node
{
    public override void _Ready()
    {
        int width = 256;
        int height = 256;
        var heights = PerlinNoise.GenerateHeightMap(width, height, scale: 80f, octaves: 5, persistence: 0.45f, lacunarity: 2f, seed: 12345);

        // Create a grayscale image to preview
        var img = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float v = heights[x, y];
                // map to 0..255
                byte b = (byte)Mathf.Clamp((int)(v * 255f), 0, 255);
                img.SetPixel(x, y, new Color(b / 255.0f, b / 255.0f, b / 255.0f));
            }
        }

        ImageTexture tex = ImageTexture.CreateFromImage(img);
        // You can now assign `tex` to a TextureRect, sprite, or use it to generate collision/tiles.
        var sprite = new Sprite2D();
        sprite.Texture = tex;
        AddChild(sprite);
    }
}
