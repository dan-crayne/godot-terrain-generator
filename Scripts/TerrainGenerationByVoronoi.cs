using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpVoronoiLib;
using TerrainGenerator.Scripts.Libraries;

namespace TerrainGenerator.Scripts;

public partial class TerrainGenerationByVoronoi : Node
{
    [Export] public TileMapLayer TileMapLayer;
    [Export] public int TileMapSize = 128;
    [Export] public int NumberOfSites = 176;


    private VoronoiPlane _voronoiPlane;
    private VoronoiEdgeHelper _voronoiEdgeHelper;
    private VoronoiSiteHelper _voronoiSiteHelper;
    private List<VoronoiSite> _rightBorderSites;
    private List<VoronoiSite> _leftBorderSites;
    private List<VoronoiSite> _bottomBorderSites;
    private List<VoronoiSite> _topBorderSites;
    
    private List<VoronoiSite> _largeRiverPathSites;
    
    public override void _Ready()
    {
        _voronoiPlane = new VoronoiPlane(0, 0, TileMapSize, TileMapSize);
        _voronoiPlane.GenerateRandomSites(NumberOfSites, PointGenerationMethod.Uniform);
        _voronoiPlane.Tessellate(BorderEdgeGeneration.MakeBorderEdges);
        _voronoiPlane.Relax(5, 0.7f); // makes cells more uniform in size and shape

        _voronoiEdgeHelper = new VoronoiEdgeHelper(_voronoiPlane);
        _voronoiSiteHelper = new VoronoiSiteHelper(_voronoiPlane);

        _rightBorderSites = _voronoiSiteHelper.FindBorderSites(_voronoiPlane.Sites, PointBorderLocation.Right);
        _leftBorderSites = _voronoiSiteHelper.FindBorderSites(_voronoiPlane.Sites, PointBorderLocation.Left);
        _bottomBorderSites = _voronoiSiteHelper.FindBorderSites(_voronoiPlane.Sites, PointBorderLocation.Bottom);
        _topBorderSites = _voronoiSiteHelper.FindBorderSites(_voronoiPlane.Sites, PointBorderLocation.Top);
        
        DrawSitesWithRandomTypes();
        DrawTopAndLeftWaterBodies();
        DrawLargeRiver();
        DrawSmallRiverBetweenCentroids();
    }

    private void DrawTopAndLeftWaterBodies()
    {
        ColorInVoronoiCellsOnTileMap(_leftBorderSites, TileTypes.Water, 0);
        ColorInVoronoiCellsOnTileMap(_bottomBorderSites, TileTypes.Water, 0);
    }

    private void DrawLargeRiver()
    {
        var pathStartSite = _voronoiSiteHelper.FindSiteAsPercentageFromTopLeftOfPlane(0.5f, 0.0f);
        var pathEndSite = _voronoiSiteHelper.FindRandomSiteFromList(_topBorderSites);
        var pathSites = _voronoiSiteHelper.FindBfsSites(pathStartSite, pathEndSite);

        _largeRiverPathSites = pathSites;
        
        ColorInVoronoiCellsOnTileMap(pathSites, TileTypes.Water, 0);
    }
    

    private void DrawSmallRiverBetweenCentroids()
    {
        var startingSite = _voronoiSiteHelper.FindRandomSiteFromList(_rightBorderSites);
        var endingSite = _voronoiSiteHelper.FindRandomSiteFromList(_largeRiverPathSites);
        // var sitePath = _voronoiSiteHelper.FindBfsSites(startingSite, endingSite);
        var sitePath = _voronoiSiteHelper.FindSitePathByVectorDirection(startingSite, -1f, 0f, 100);
        for (int i = 0; i < sitePath.Count - 1; i++)
        {
            var line = PointHelper.GetPointsInLineBetweenVoronoiPoints(sitePath[i].Centroid, sitePath[i + 1].Centroid);
            foreach (var point in line)
            {
                var gridPosition = NormalizeToTileMapCoordinates(point);
                TileMapLayer.SetCell(gridPosition, 0, TileAtlasPositionByType(TileTypes.Water, 0));
            }
        }
    }
    
    private void DrawSitesWithRandomTypes()
    {
        var siteToType = AssignRandomCellTypesToSites(_voronoiPlane.Sites);
        foreach (var kvp in siteToType)
        {
            var site = kvp.Key;
            var cellType = kvp.Value;
            TileTypes tileType;
            int darknessLevel = (int)GD.RandRange(0, 3);
            switch (cellType)
            {
                case CellTypes.Grassland:
                    tileType = TileTypes.Grass;
                    break;
                case CellTypes.Forest:
                    tileType = TileTypes.Grass;
                    break;
                default:
                    tileType = TileTypes.Grass;
                    break;
            }
            ColorInVoronoiCellOnTileMap(site, tileType, darknessLevel);
        }
    }
    
    private void DrawTilesWithRandomTypes()
    {
        if (TileMapLayer == null) throw new InvalidOperationException("TileMapLayer is not set.");
        
        for (var x = 0; x < TileMapSize; x++)
        {
            for (var y = 0; y < TileMapSize; y++)
            {
                var randomValue = GD.Randf();
                TileTypes tileType;
                int darknessLevel = (int)GD.RandRange(0, 3);
                if (randomValue < 0.3f)
                {
                    tileType = TileTypes.Water;
                }
                else
                {
                    tileType = TileTypes.Grass;
                }
                
                Vector2I gridPosition = new Vector2I(x, y);
                TileMapLayer.SetCell(gridPosition, 0, TileAtlasPositionByType(tileType, darknessLevel));
            }
        }
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
    
    public void DrawLineOnTileMap(Vector2I start, Vector2I end, TileTypes tileType = TileTypes.Water)
    {
        if (TileMapLayer == null) throw new InvalidOperationException("TileMapLayer is not set.");
        
        var points = PointHelper.GetPointsInLineBetweenIntVectors(start, end);
        foreach (var point in points)
        {
            TileMapLayer.SetCell(point, 0, TileAtlasPositionByType(tileType, 0));
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
