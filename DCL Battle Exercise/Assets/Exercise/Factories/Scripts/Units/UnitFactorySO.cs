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
        
        public UnitCreationParameters(Vector3 position, Quaternion rotation, IUnitModel model)
        {
            Position = position;
            Rotation = rotation;
            Model = model;
        }
    }

    public interface IUnitFactory
    {
        // TODO IUnit, IArmy
        IUnit CreateUnit(IArmy parentArmy, UnitCreationParameters parameters);
    }

    public abstract class UnitFactorySO : ScriptableObject, IUnitFactory
    {
        [SerializeField]
        private GameObject _unitPrefab;

        public virtual IUnit CreateUnit(IArmy parentArmy, UnitCreationParameters parameters)
        {
            // TODO Pooling
            IUnit unitGameobject = Instantiate(_unitPrefab).GetComponent<IUnit>();
            unitGameobject.Initialize(parentArmy, parameters);
            return unitGameobject;
        }
    }
}