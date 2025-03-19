using Microsoft.Extensions.VectorData;

public class Player
{
    [VectorStoreRecordKey]
    public int Key { get; set; }

    [VectorStoreRecordData]
    public string? Name { get; set; }

    [VectorStoreRecordData]
    public string? Position { get; set; }

    [VectorStoreRecordData]
    public string? Team { get; set; }

    [VectorStoreRecordVector(384, DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}