using System.Collections.Generic;

using Utils;

namespace DCLBattle.Battle
{
    /// <summary>
    /// The different states in which the battle can be.
    /// /!\/!\/!\ WARNING: DO NOT update the index, as it will break Unity's Serialization.
    /// </summary>
    public enum BattleStateID
    {
        OnGoing = 0,
        Paused = 1,
        Ended = 2
    }

    public sealed class BattleFSM : FSM<BattleState, BattleStateID>
    {
        public BattleFSM(BattleState defaultState, List<BattleState> states) :
            base(defaultState, states)
        {
        }
    }
}