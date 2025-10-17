namespace Kuchinashi.DataSystem
{
    public interface IReadableData
    {
        public abstract IReadableData DeSerialize();

        public abstract T DeSerialize<T>() where T : IReadableData, new();

        public abstract bool Validate<T>(out T value) where T : IReadableData, new();
    }
}