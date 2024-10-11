using Utils.SpatialPartitioning;

public enum SearchAlgorithm
{
    KDTree2D = 0,
    Quadtree = 1,
    //KDTree3D = 2,
    //Octree = 3
}

public interface IBattleModel
{
    SearchAlgorithm Algorithm { get; set; }

    ISpatialPartitioner<TDimension> CreateSpatialPartitioner<TDimension>();
}