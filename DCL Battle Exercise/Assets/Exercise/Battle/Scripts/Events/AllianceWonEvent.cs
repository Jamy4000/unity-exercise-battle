namespace DCLBattle.Battle
{
    public sealed class AllianceWonEvent
    {
        public readonly int AllianceID;

        public AllianceWonEvent(int allianceID)
        {
            AllianceID = allianceID;
        }
    }
}