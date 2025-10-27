namespace Mission
{
    public struct OnMissionAccomplishedEvent
    {
        public MissionBase mission;
        public OnMissionAccomplishedEvent(MissionBase mission)
        {
            this.mission = mission;
        }
    }
}