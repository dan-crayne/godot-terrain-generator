using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpVoronoiLib;

namespace TerrainGenerator.Scripts;

public partial class TerrainGenerationByVoronoi : Node
{
    [Export] public TileMapLayer TileMapLayer;
    [Export] public int TileMapSize = 128;
    [Export] public int NumberOfSites = 176;


    private VoronoiPlane _voronoiPlane;
    
    public override void _Ready()
    {
        _voronoiPlane = new VoronoiPlane(0, 0, TileMapSize, TileMapSize);
        _voronoiPlane.GenerateRandomSites(NumberOfSites, PointGenerationMethod.Uniform);
        _voronoiPlane.Tessellate(BorderEdgeGeneration.MakeBorderEdges);
        _voronoiPlane.Relax(); // makes cells more uniform in size and shape
        
        // DrawVoronoiEdgesOnTileMap(_voronoiPlane);
        // DrawVoronoiSitesOnTileMap(_voronoiPlane, TileTypes.Water);
        
        var siteToType = AssignRandomCellTypesToSites(_voronoiPlane?.Sites);
        foreach (var kvp in siteToType)
        {
            var site = kvp.Key;
            var cellType = kvp.Value;
            var tileType = cellType switch
            {
                CellTypes.Water => TileTypes.Water,
                CellTypes.Grassland => TileTypes.Grass,
                CellTypes.Forest => TileTypes.Grass,
                _ => TileTypes.Grass
            };
            
            var randomDarknessLevel = (int)GD.RandRange(0, 3);
            
            GD.Print($"Drawing cell at ({site.X}, {site.Y})");
            ColorInVoronoiCellOnTileMap(site, tileType, randomDarknessLevel);
        }
        
        var topBorderSites = GetBorderSites(_voronoiPlane?.Sites, PointBorderLocation.Top);
        var bottomBorderSites = GetBorderSites(_voronoiPlane?.Sites, PointBorderLocation.Bottom);
        var leftBorderSites = GetBorderSites(_voronoiPlane?.Sites, PointBorderLocation.Left);
        var rightBorderSites = GetBorderSites(_voronoiPlane?.Sites, PointBorderLocation.Right);
        
        var pathStartSite = GetRandomSiteFromList(topBorderSites);
        var pathEndSite = GetRandomSiteFromList(bottomBorderSites);
        var pathSites = GetBreadthFirstSearchPath(pathStartSite, pathEndSite);
        
        ColorInVoronoiCellsOnTileMap(leftBorderSites, TileTypes.Water, 0);
        ColorInVoronoiCellsOnTileMap(bottomBorderSites, TileTypes.Water, 0);
        // ColorInVoronoiCellsOnTileMap(topBorderSites, TileTypes.Water, 0);
        // ColorInVoronoiCellsOnTileMap(rightBorderSites, TileTypes.Water, 0);
        ColorInVoronoiCellsOnTileMap(pathSites, TileTypes.Water, 0);
    }
    
    // private List<VoronoiEdge> FindEdgePathBetweenTwoPoints(VoronoiPoint startPoint, VoronoiPoint endPoint)
    // {
    //     
    // }

    private List<VoronoiSite> GetBreadthFirstSearchPath(VoronoiSite startSite, VoronoiSite endSite)
    { var visited = new HashSet<VoronoiSite>();
        var queue = new Queue<List<VoronoiSite>>();
        queue.Enqueue(new List<VoronoiSite> { startSite });

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var currentSite = path.Last();

            if (currentSite == endSite)
            {
                return path; // Return the path if the end site is reached
            }

            if (!visited.Contains(currentSite))
            {
                visited.Add(currentSite);

                // Get neighboring sites connected by edges
                var neighbors = currentSite.Neighbours;

                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        var newPath = new List<VoronoiSite>(path) { neighbor };
                        queue.Enqueue(newPath);
                    }
                }
            }
        }

        return new List<VoronoiSite>(); // Return an empty list if no path is found
    }
    
    private Dictionary<VoronoiSite, CellTypes> AssignRandomCellTypesToSites(List<VoronoiSite> voronoiSites)
    {
        if (voronoiSites == null) throw new ArgumentNullException(nameof(voronoiSites));
        
        var siteToType = new Dictionary<VoronoiSite, CellTypes>();
        foreach (var site in voronoiSites)
        {
            var randomValue = GD.Randf();
            CellTypes cellType;
            // if (randomValue < 0.3f)
            // {
            //     cellType = CellTypes.Water;
            // }
            if (randomValue < 0.6f)
            {
                cellType = CellTypes.Grassland;
            }
            else
            {
                cellType = CellTypes.Forest;
            }
            siteToType[site] = cellType;
        }
        return siteToType;
    }
    
    private Vector2I NormalizeToTileMapCoordinates(Vector2 point)
    {
        var x = (int)Mathf.Clamp(point.X, 0, TileMapSize - 1);
        var y = (int)Mathf.Clamp(point.Y, 0, TileMapSize - 1);
        return new Vector2I(x, y);
    }
    
    private void ColorInVoronoiCellOnTileMap(VoronoiSite site, TileTypes tileType = TileTypes.Grass, int darknessLevel = 0)
    {
        if (site == null) throw new ArgumentNullException(nameof(site));
        if (TileMapLayer == null) throw new InvalidOperationException("TileMapLayer is not set.");

        for (var x = 0; x < TileMapSize; x++)
        {
            for (var y = 0; y < TileMapSize; y++)
            {
                if (site.Contains(x, y))
                {
                    Vector2I gridPosition = new Vector2I(x, y);
                    TileMapLayer.SetCell(gridPosition, 0, TileAtlasPositionByType(tileType, darknessLevel));
                }
            }
        }
    }
    
    private void ColorInVoronoiCellsOnTileMap(List<VoronoiSite> sites, TileTypes tileType = TileTypes.Grass, int darknessLevel = 0)
    {
        if (sites == null) throw new ArgumentNullException(nameof(sites));
        if (TileMapLayer == null) throw new InvalidOperationException("TileMapLayer is not set.");

        foreach (var site in sites)
        {
            for (var x = 0; x < TileMapSize; x++)
            {
                for (var y = 0; y < TileMapSize; y++)
                {
                    if (site.Contains(x, y))
                    {
                        Vector2I gridPosition = new Vector2I(x, y);
                        TileMapLayer.SetCell(gridPosition, 0, TileAtlasPositionByType(tileType, darknessLevel));
                    }
                }
            }
        }
    }

    private List<VoronoiSite> GetBorderSites(List<VoronoiSite> sites, PointBorderLocation borderLocation)
    {
        var borderSites = new List<VoronoiSite>();
        foreach (var site in sites)
        {
            var points = site.Points;
            foreach (var point in points)
            {
                if (point.BorderLocation == borderLocation)
                {
                    borderSites.Add(site);
                }
            }
        }
        
        return borderSites;
    }

    private VoronoiSite GetRandomSiteFromList(List<VoronoiSite> voronoiSites)
    {
        var randomIndex = GD.RandRange(0, voronoiSites.Count - 1);
        return voronoiSites[randomIndex];
    }
    
    private VoronoiEdge GetRandomEdgeFromList(List<VoronoiEdge> voronoiEdges)
    {
        var randomIndex = GD.RandRange(0, voronoiEdges.Count - 1);
        return voronoiEdges[randomIndex];
    }
    
    private void DrawVoronoiSitesOnTileMap(VoronoiPlane voronoiPlane, TileTypes tileType = TileTypes.Grass)
    {
        if (voronoiPlane == null) throw new ArgumentNullException(nameof(voronoiPlane));
        if (TileMapLayer == null) throw new InvalidOperationException("TileMapLayer is not set.");
        
        var sites = voronoiPlane.Sites;
        foreach (var site in sites)
        {
            Vector2I gridPosition = new Vector2I((int)site.X, (int)site.Y);
            TileMapLayer.SetCell(gridPosition, 0, TileAtlasPositionByType(tileType, 0));
        }
    }
    
    private void DrawVoronoiEdgesOnTileMap(VoronoiPlane voronoiPlane, TileTypes tileType = TileTypes.Grass)
    {
        if (voronoiPlane == null) throw new ArgumentNullException(nameof(voronoiPlane));
        if (TileMapLayer == null) throw new InvalidOperationException("TileMapLayer is not set.");
        
        var edges = voronoiPlane.Tessellate(BorderEdgeGeneration.MakeBorderEdges);
        foreach (var edge in edges)
        {
            DrawEdgeLineOnTileMap(edge);
        }
    }

    private void DrawEdgeLineOnTileMap(VoronoiEdge edge, TileTypes tileType = TileTypes.Grass)
    {
        if (TileMapLayer == null) throw new InvalidOperationException("TileMapLayer is not set.");
        
        // Get start and end coordinates of the edge
        int x0 = (int)edge.Start.X;
        int y0 = (int)edge.Start.Y;
        int x1 = (int)edge.End.X;
        int y1 = (int)edge.End.Y;

        // Calculate differences and step directions
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1; // Step direction for x
        int sy = y0 < y1 ? 1 : -1; // Step direction for y
        int err = dx - dy;         // Error term for Bresenham's algorithm

        // Loop to draw the line from (x0, y0) to (x1, y1)
        while (true)
        {
            // Set the tile at the current position
            Vector2I gridPosition = new Vector2I(x0, y0);
            TileMapLayer.SetCell(gridPosition, 0, TileAtlasPositionByType(tileType, 0));

            // Break if the end point is reached
            if (x0 == x1 && y0 == y1) break;

            int err2 = err * 2;
            // Adjust error and position for x
            if (err2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            // Adjust error and position for y
            if (err2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
    
    private Vector2I TileAtlasPositionByType(TileTypes tileType, int darknessLevel)
    {
        switch (tileType)
        {
            case TileTypes.Water:
                return new Vector2I(darknessLevel, 0);
            case TileTypes.Grass:
                return new Vector2I(darknessLevel, 1);
            default:
                throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null);

        }
    }
}
