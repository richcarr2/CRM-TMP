namespace VA.TMP.Integration.Core
{
    public interface IFilter<T>
    {
        void Execute(T input);
    }
}