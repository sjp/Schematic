namespace SJP.Schema.Modelled.Reflection
{
    public interface IDbType<T>
    {
    }

    public struct nvarchar2000 : IDbType<string> { }
}
