using System.Collections.Generic;
using Godot;
using SharpVoronoiLib;

namespace TerrainGenerator.Scripts.Libraries;

public static class PointHelper
{
    public static List<Vector2I> GetPointsInLineBetweenVoronoiPoints(VoronoiPoint start, VoronoiPoint end)
    {
        var startI = new Vector2I((int)start.X, (int)start.Y);
        var endI = new Vector2I((int)end.X, (int)end.Y);
        return GetPointsInLineBetweenIntVectors(startI, endI);
    }
    
    public static List<Vector2I> GetPointsInLineBetweenIntVectors(Vector2I start, Vector2I end)
    {
        if (start == end)
        {
            return [start];
        }
        
        var points = new List<Vector2I>();
        var dx = System.Math.Abs(end.X - start.X);
        var dy = System.Math.Abs(end.Y - start.Y);
        var sx = start.X < end.X ? 1 : -1;
        var sy = start.Y < end.Y ? 1 : -1;
        var err = dx - dy;
        var current = start;
        while (true)
        {
            points.Add(current);
            if (current == end) break;
            var err2 = err * 2;
            if (err2 > -dy)
            {
                err -= dy;
                current.X += sx;
            }
            if (err2 < dx)
            {
                err += dx;
                current.Y += sy;
            }
        }
        
        return points;
    }
}