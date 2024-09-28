using UnityEngine;

namespace DCLBattle.Battle
{
    // Data Transfer Object; this avoid modifying the signature of CreateUnit if we want to add more data later on.
    // If some units have specific data that need to be injected, they can extend this class to do so
    public class UnitCreationParameters
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly IUnitModel Model;
        public readonly IArmy ParentArmy;

        public UnitCreationParameters(Vector3 position, Quaternion rotation, IArmy parentArmy, IUnitModel model)
        {
            Position = position;
            Rotation = rotation;
            ParentArmy = parentArmy;
            Model = model;
        }
    }

    public interface IUnitFactory
    {
        IUnit CreateUnit(UnitCreationParameters parameters);
    }

    // Could be extended if necessary, though i don't really see the point
    /// <summary>
    /// A simple Scriptable Object handling the instantiation of a unit prefab.
    /// Note: This feels to me overkill and unnecessary; this could simply be placed in the UnitModelSO.
    /// However, in order to follow the Single Responsibility Principle, I will keep it as is.
    /// </summary>
    [CreateAssetMenu(menuName = "Create Unit Factory", fileName = "ArmyModel", order = 0)]
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