using System;
using System.Collections.Generic;
using System.Linq;

namespace TerrainGenerator.Scripts.Libraries;

public class Searcher<T> where T : INeighborHaver<T>, IEquatable<T>
{
    
    private List<T> GetBreadthFirstSearchPath(T start, T end)
    { var visited = new HashSet<T>();
        var queue = new Queue<List<T>>();
        queue.Enqueue([start]);

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var current = path.Last();

            if (current.Equals(end))
            {
                return path; // Return the path if the end site is reached
            }

            if (!visited.Contains(current))
            {
                visited.Add(current);

                // Get neighboring sites connected by edges
                var neighbors = current.Neighbours;

                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        var newPath = new List<T>(path) { neighbor };
                        queue.Enqueue(newPath);
                    }
                }
            }
        }

        return new List<T>(); // Return an empty list if no path is found
    }
}