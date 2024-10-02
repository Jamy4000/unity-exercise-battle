using UnityEngine;

namespace DCLBattle.Battle
{
    public interface IStrategyUpdater
    {
        public static readonly int StrategyCount = System.Enum.GetValues(typeof(ArmyStrategy)).Length;

        public static readonly UnityEngine.Vector3 FlatScale = new UnityEngine.Vector3(1f, 0f, 1f);

        ArmyStrategy ArmyStrategy { get; }

        /// <summary>
        /// Check for a specific strategy what the unit should do.
        /// </summary>
        /// <param name="unitToUpdate">The unit we want to update</param>
        /// <returns>returns the direction in which we want the unit to move</returns>
        //UnityEngine.Vector3 UpdateStrategy(UnitBase unitToUpdate);
        TargetInfo UpdateStrategy(UnitData dataSet, out Vector3 strategyMovement);
    }
}