using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    public abstract class BattleStateData : ScriptableObject
    {
        [field: SerializeField]
        public List<BattleStateID> ExitStates { get; private set; } = new();

        public abstract BattleStateID StateID { get; }

        // This is an alternative to the factory pattern boilerplate code;
        // the data provides the implementation of the logic while injecting itself in the state.
        public abstract BattleState CreateStateInstance(IServiceLocator serviceLocator);
    }
}