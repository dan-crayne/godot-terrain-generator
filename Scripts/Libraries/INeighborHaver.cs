using System.Collections.Generic;

namespace TerrainGenerator.Scripts.Libraries;

public interface INeighborHaver<T>
{
   IEnumerable<T> Neighbours { get; }
}