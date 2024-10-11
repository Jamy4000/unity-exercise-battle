using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Battles/FSM/Battle Ended State Data", fileName = "BattleEndedStateData", order = 0)]
    public class BattleEndedStateData : BattleStateData
    {
        public override BattleStateID StateID => BattleStateID.Ended;

        public override BattleState CreateStateInstance(IServiceLocator serviceLocator)
        {
            return new BattleEndedState(this, serviceLocator);
        }
    }
}