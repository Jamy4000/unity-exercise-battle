using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    [CreateAssetMenu(menuName = "DCLBattle/Battles/FSM/Battle Setup State Data", fileName = "BattleSetupStateData", order = 0)]
    public class BattleSetupStateData : BattleStateData
    {
        public override BattleStateID StateID => BattleStateID.Setup;

        public override BattleState CreateStateInstance(IServiceLocator serviceLocator)
        {
            return new BattleSetupState(this, serviceLocator);
        }
    }

}