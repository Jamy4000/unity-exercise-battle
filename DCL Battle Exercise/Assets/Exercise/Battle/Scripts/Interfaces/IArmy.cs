using UnityEngine;

namespace DCLBattle.Battle
{
    public interface IArmy
    {
        IArmyModel Model { get; }
        int RemainingUnitsCount { get; }

        void Update();
        Vector3 CalculateCenterPoint();

        void AddUnit(IUnit unit);
        void RemoveUnit(IUnit unit);
    }
}