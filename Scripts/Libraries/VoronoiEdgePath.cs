using System.Collections.Generic;
using SharpVoronoiLib;

namespace TerrainGenerator.Scripts.Libraries;

public class VoronoiEdgePath(List<VoronoiEdge> edges)
{
    public List<VoronoiEdge> Edges { get; } = edges;

    public VoronoiEdgePath() : this(new List<VoronoiEdge>())
    {
    }
}