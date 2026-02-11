namespace SlimeRpgEvolution2D.Data
{
    public interface IIdentifiable<out TKey>
    {
        TKey ID { get; }
    }
}