namespace QFramework
{
    internal class StopAllSoundCommand
    {
        internal static void Execute()
        {
            Architecture.PlayingSoundPoolModel.ForEachAllSound(player=>player.Stop());
            Architecture.PlayingSoundPoolModel.ClearAllPlayingSound();
        }
    }
}