using System.Collections.Generic;
using System.Linq;
using SharpVoronoiLib;

namespace TerrainGenerator.Scripts.Libraries;

public class VoronoiSearcher(VoronoiPlane voronoiPlane)
{
    private readonly VoronoiPlane _voronoiPlane = voronoiPlane;

    public List<VoronoiEdge> FindEdgePathBetweenTwoEdges(VoronoiEdge startEdge, VoronoiEdge endEdge)
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
                return path; // Return the path if the end edge is reached
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

        return new List<VoronoiEdge>(); // Return an empty list if no path is found
    }

    public List<VoronoiSite> GetBreadthFirstSearchPath(VoronoiSite startSite, VoronoiSite endSite)
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
}