namespace QFramework
{
    public class MusicPlayer : AbstractAudioPlayer
    {
        public MusicPlayer(BindableProperty<float> volume,bool isLoop = true)
        {
            OnInit(volume);
            IsLoop = isLoop;
        }

        protected override void OnPlayStarted()
        {
            
        }

        internal override bool CanPlayAudio() => true;

        protected override void OnBeforeStop()
        {
            
        }

        protected override void OnStop()
        {
            
        }

        public void Deinit()
        {
            OnDeinit();
        }
    }
}