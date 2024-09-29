using UnityEngine;

namespace DCLBattle.Battle
{
    // Data Transfer Object; this avoid modifying the signature of CreateUnit if we want to add more data later on.
    // If some units have specific data that need to be injected, they can extend this class to do so
    public class UnitCreationParameters
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly UnitType UnitType;
        public readonly IArmy ParentArmy;
        public readonly IStrategyUpdater StrategyUpdater;

        public UnitCreationParameters(Vector3 position, Quaternion rotation, IArmy parentArmy, UnitType unitType, IStrategyUpdater strategyUpdater)
        {
            Position = position;
            Rotation = rotation;
            UnitType = unitType;
            ParentArmy = parentArmy;
            StrategyUpdater = strategyUpdater;
        }
    }

    public interface IUnitFactory
    {
        IUnit CreateUnit(UnitCreationParameters parameters);
    }

    /// <summary>
    /// A simple Scriptable Object handling the instantiation of a unit prefab.
    /// Note: This feels to me overkill and unnecessary; this could simply be placed in the UnitModelSO.
    /// However, in order to follow the Single Responsibility Principle, I will keep it as is.
    /// </summary>
    public class UnitFactorySO : ScriptableObject, IUnitFactory
    {
        [SerializeField]
        private GameObject _unitPrefab;

        public virtual IUnit CreateUnit(UnitCreationParameters parameters)
        {
            // TODO Pooling
            IUnit unitGameobject = Instantiate(_unitPrefab).GetComponent<IUnit>();
            unitGameobject.Initialize(parameters);
            return unitGameobject;
        }
    }
}