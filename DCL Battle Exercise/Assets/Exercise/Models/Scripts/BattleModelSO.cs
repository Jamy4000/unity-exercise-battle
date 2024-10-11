using UnityEngine;
using Utils.SpatialPartitioning;

/// <summary>
/// ScriptableObject containing the data of the Battle
/// </summary>
[CreateAssetMenu(menuName = "DCLBattle/Battle/Create Battle Model", fileName = "BattleModel", order = 0)]
public class BattleModelSO : ScriptableObject, IBattleModel
{
    [field: SerializeField, ReadOnly]
    public SearchAlgorithm Algorithm { get; set; }

    public ISpatialPartitioner<TDimension> CreateSpatialPartitioner<TDimension>()
    {
        // This is breaking Open Close principle, but shouldn't be much of an issue tbh
        switch (Algorithm)
        {
            case SearchAlgorithm.KDTree2D:
                return (ISpatialPartitioner<TDimension>)(object)new KDTree<Vector2>();

            case SearchAlgorithm.Quadtree:
                return (ISpatialPartitioner<TDimension>)(object)new Quadtree(Vector2.zero, Vector2.one * 1000f);

            default:
                throw new System.NotImplementedException($"The implementation of the Spatial Partitioner for {Algorithm} was not provided.");
        }
    }
}