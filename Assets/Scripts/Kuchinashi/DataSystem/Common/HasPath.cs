namespace Kuchinashi.DataSystem
{
    public interface IHasPath
    {
        public abstract string Path { get; set; }
        public abstract void Init(string path);
    }
}