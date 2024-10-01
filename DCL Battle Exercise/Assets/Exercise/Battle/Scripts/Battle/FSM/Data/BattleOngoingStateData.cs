using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Battles/FSM/Battle Ongoing State Data", fileName = "BattleOngoingStateData", order = 0)]
    public class BattleOngoingStateData : BattleStateData
    {
        public override BattleStateID StateID => BattleStateID.OnGoing;

        public override BattleState CreateStateInstance(IArmiesHolder armiesHolder)
        {
            return new BattleOngoingState(this, armiesHolder);
        }
    }

}