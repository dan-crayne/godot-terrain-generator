using System.Collections.Generic;
using System.Linq;
using Godot;
using SharpVoronoiLib;

namespace TerrainGenerator.Scripts.Libraries;

public class VoronoiEdgeHelper
{
    private readonly VoronoiPlane _voronoiPlane;
    
    public VoronoiEdgeHelper(VoronoiPlane voronoiPlane)
    {
        _voronoiPlane = voronoiPlane;
    }
    
    public List<VoronoiEdge> FindEdgePathBetweenSites(VoronoiSite startSite, VoronoiSite endSite)
    {
        var pathEdges = new List<VoronoiEdge>();
        var currentSite = startSite;

        while (currentSite != null && currentSite != endSite)
        {
            VoronoiEdge nextEdge = null;
            float bestDotProduct = float.NegativeInfinity;

            foreach (var edge in currentSite.Cell)
            {
                var neighbor = edge.Left == currentSite ? edge.Right : edge.Left;
                if (neighbor == null || pathEdges.Contains(edge))
                {
                    continue; // Skip if no neighbor or already in path
                }

                var toNeighbor = new Vector2((float)(neighbor.X - currentSite.X), (float)(neighbor.Y - currentSite.Y)).Normalized();
                var toEnd = new Vector2((float)(endSite.X - currentSite.X), (float)(endSite.Y - currentSite.Y)).Normalized();
                var dotProduct = toNeighbor.Dot(toEnd);

                if (dotProduct > bestDotProduct)
                {
                    bestDotProduct = dotProduct;
                    nextEdge = edge;
                }
            }

            if (nextEdge == null)
            {
                break; // No valid next edge found
            }

            pathEdges.Add(nextEdge);
            currentSite = nextEdge.Left == currentSite ? nextEdge.Right : nextEdge.Left;
        }

        return pathEdges;
    }
    
    public List<VoronoiEdge> FindEdgePathAlongSitePath(List<VoronoiSite> sitePath)
    {
        var edgePath = new List<VoronoiEdge>();

        for (int i = 0; i < sitePath.Count - 1; i++)
        {
            var currentSite = sitePath[i];
            var nextSite = sitePath[i + 1];

            // Find the edge that connects the current site to the next site
            var connectingEdge = currentSite.Cell.FirstOrDefault(edge => edge.Left == nextSite || edge.Right == nextSite);

            if (connectingEdge != null)
            {
                edgePath.Add(connectingEdge);
            }
            else
            {
                // If no connecting edge is found, return an empty path
                return new List<VoronoiEdge>();
            }
        }

        return edgePath;
    }
    
    public int FindLengthOfEdgePath(List<VoronoiEdge> path)
    {
        int totalLength = 0;
        foreach (var edge in path)
        {
            totalLength += (int)edge.Length;
        }
        
        return totalLength;
    }
    
    public VoronoiEdgePath FindBfsEdgePath(VoronoiEdge startEdge, VoronoiEdge endEdge)
    {
        var visited = new HashSet<VoronoiEdge>();
        var queue = new Queue<List<VoronoiEdge>>();
        queue.Enqueue(new List<VoronoiEdge> { startEdge });

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var currentEdge = path.Last();

            if (currentEdge == endEdge)
            {
                return new VoronoiEdgePath(path); // Return the path if the end edge is reached
            }

            if (!visited.Contains(currentEdge))
            {
                visited.Add(currentEdge);

                // Get neighboring edges connected by sites
                var neighbors = currentEdge.Neighbours;

                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        var newPath = new List<VoronoiEdge>(path) { neighbor };
                        queue.Enqueue(newPath);
                    }
                }
            }
        }

        return new VoronoiEdgePath(); // Return an empty list if no path is found
    }
    
    public VoronoiEdgePath FindDfsEdgePath(VoronoiEdge startEdge, VoronoiEdge endEdge)
    {
        var visited = new HashSet<VoronoiEdge>();
        var stack = new Stack<List<VoronoiEdge>>();
        stack.Push(new List<VoronoiEdge> { startEdge });

        while (stack.Count > 0)
        {
            var path = stack.Pop();
            var currentEdge = path.Last();

            if (currentEdge == endEdge)
            {
                return new VoronoiEdgePath(path); // Return the path if the end edge is reached
            }

            if (!visited.Contains(currentEdge))
            {
                visited.Add(currentEdge);

                // Get neighboring edges connected by sites
                var neighbors = currentEdge.Neighbours;

                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        var newPath = new List<VoronoiEdge>(path) { neighbor };
                        stack.Push(newPath);
                    }
                }
            }
        }

        return new VoronoiEdgePath(); // Return an empty list if no path is found
    }
    
    public VoronoiEdge FindRandomEdgeFromSite(VoronoiSite site)
    {
        var edges = site.Cell.ToList();
        var randomIndex = GD.RandRange(0, edges.Count - 1);
        return edges[randomIndex];
    }

    public VoronoiEdge FindRandomEdgeFromList(List<VoronoiEdge> voronoiEdges)
    {
        var randomIndex = GD.RandRange(0, voronoiEdges.Count - 1);
        return voronoiEdges[randomIndex];
    }
    
}