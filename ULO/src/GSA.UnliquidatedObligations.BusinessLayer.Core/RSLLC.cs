namespace RevolutionaryStuff.Core.ApplicationParts
{
    public interface IPrimaryKey
    {
        object Key { get; }
    }

    public interface IPrimaryKey<TKey> : IPrimaryKey
    {
        new TKey Key { get; }
    }
    public interface IDataEntity
    { }
    public interface IRdbDataEntity : IDataEntity
    {
    }
}
