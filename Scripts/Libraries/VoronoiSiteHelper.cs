using System.Collections.Generic;
using System.Linq;
using Godot;
using SharpVoronoiLib;

namespace TerrainGenerator.Scripts.Libraries;

public class VoronoiSiteHelper(VoronoiPlane voronoiPlane)
{
    public VoronoiSite FindSiteAsPercentageFromTopLeftOfPlane(float percentageX, float percentageY)
    {
        var x = voronoiPlane.MinX + (voronoiPlane.MaxX - voronoiPlane.MinX) * percentageX;
        var y = voronoiPlane.MinY + (voronoiPlane.MaxY - voronoiPlane.MinY) * percentageY;

        return voronoiPlane.GetNearestSiteTo(x, y);
    }
    
    public List<VoronoiSite> FindAllBorderSitesByLocation(PointBorderLocation borderLocation)
    {
        var sites = new List<VoronoiSite>();
        
        if (voronoiPlane is null) return sites;
        
        foreach (var site in voronoiPlane.Sites)
        {
            foreach (var point in site.Points)
            {
                if (point.BorderLocation == borderLocation)
                {
                    sites.Add(site);
                }
            }
        }
        
        return sites;
    }
    
    public List<VoronoiSite> FindSitePathByVectorDirection(VoronoiSite startSite, float directionX, float directionY, int maxSteps = 100)
    {
        var path = new List<VoronoiSite> { startSite };
        var currentSite = startSite;

        for (int i = 0; i < maxSteps; i++)
        {
            VoronoiSite nextSite = null;
            float bestDotProduct = float.NegativeInfinity;

            foreach (var neighbor in currentSite.Neighbours)
            {
                var toNeighborX = (float) neighbor.Centroid.X - (float) currentSite.Centroid.X;
                var toNeighborY = (float) neighbor.Centroid.Y - (float) currentSite.Centroid.Y;
                var magnitude = System.MathF.Sqrt(toNeighborX * toNeighborX + toNeighborY * toNeighborY);
                if (magnitude == 0) continue;

                toNeighborX /= magnitude;
                toNeighborY /= magnitude;

                var dotProduct = toNeighborX * directionX + toNeighborY * directionY;

                if (dotProduct > bestDotProduct)
                {
                    bestDotProduct = dotProduct;
                    nextSite = neighbor;
                }
            }

            if (nextSite == null || path.Contains(nextSite))
            {
                break; // No valid next site or already visited
            }

            path.Add(nextSite);
            currentSite = nextSite;
        }

        return path;
    }
    
    public List<VoronoiSite> FindBfsSites(VoronoiSite startSite, VoronoiSite endSite)
    {
        var visited = new HashSet<VoronoiSite>();
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
    
    
    public List<VoronoiSite> FindDfsSites(VoronoiSite startSite, VoronoiSite endSite)
    {
        var visited = new HashSet<VoronoiSite>();
        var stack = new Stack<List<VoronoiSite>>();
        stack.Push(new List<VoronoiSite> { startSite });

        while (stack.Count > 0)
        {
            var path = stack.Pop();
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
                        stack.Push(newPath);
                    }
                }
            }
        }

        return new List<VoronoiSite>(); // Return an empty list if no path is found
    }
    
    public List<VoronoiSite> FindBorderSites(List<VoronoiSite> sites, PointBorderLocation borderLocation)
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

    public VoronoiSite FindRandomSiteFromList(List<VoronoiSite> voronoiSites)
    {
        var randomIndex = GD.RandRange(0, voronoiSites.Count - 1);
        return voronoiSites[randomIndex];
    }
}