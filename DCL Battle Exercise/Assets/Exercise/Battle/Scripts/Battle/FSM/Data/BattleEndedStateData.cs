using UnityEngine;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Battles/FSM/Battle Ended State Data", fileName = "BattleEndedStateData", order = 0)]
    public class BattleEndedStateData : BattleStateData
    {
        public override BattleStateID StateID => BattleStateID.Ended;

        public override BattleState CreateStateInstance(IArmiesHolder armiesHolder)
        {
            return new BattleEndedState(this, armiesHolder);
        }
    }
}