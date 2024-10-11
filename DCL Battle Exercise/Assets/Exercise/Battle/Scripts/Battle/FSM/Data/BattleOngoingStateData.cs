using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Battle/FSM/Battle Ongoing State Data", fileName = "BattleOngoingStateData", order = 0)]
    public class BattleOngoingStateData : BattleStateData
    {
        public override BattleStateID StateID => BattleStateID.OnGoing;

        public override BattleState CreateStateInstance(IServiceLocator serviceLocator)
        {
            return new BattleOngoingState(this, serviceLocator);
        }
    }

}