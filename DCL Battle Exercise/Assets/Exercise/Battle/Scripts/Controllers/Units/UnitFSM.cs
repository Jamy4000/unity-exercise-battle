using System.Collections.Generic;

using Utils;

namespace DCLBattle.Battle
{
    /// <summary>
    /// The type of ways the units can behave in the world.
    /// /!\/!\/!\ WARNING: DO NOT update the index, as it will break Unity's Serialization.
    /// </summary>
    public enum UnitStateID
    {
        Idle = 0,
        Walk = 1,
        Attacking = 2,
        Blocked = 3
    }

    public sealed class UnitFSM : FSM<UnitState, UnitStateID>
    {
        public UnitFSM(UnitState defaultState, List<UnitState> states) :
            base(defaultState, states)
        {
        }

        // TODO On state starts; send event
    }
}