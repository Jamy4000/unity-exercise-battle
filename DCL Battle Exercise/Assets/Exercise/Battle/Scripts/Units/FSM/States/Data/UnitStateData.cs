using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public abstract class UnitStateData : ScriptableObject
    {
        [field: SerializeField]
        public List<UnitStateID> ExitStates { get; private set; } = new();

        public abstract UnitStateID StateID { get; }

        // Thid is an alternative to the factory pattern boilerplate code;
        // the data provides the implementation of the logic while injecting itself in the state.
        public abstract UnitState CreateStateInstance(UnitBase unit);
    }
}