using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public interface IArmy
    {
        IArmyModel Model { get; }
        int RemainingUnitsCount { get; }

        void Update();
        Vector3 CalculateCenterPoint();

        IUnit GetClosestUnit(Vector3 source, out float distance);
        int GetUnitsInRadius_NoAlloc(Vector3 source, float radius, IUnit[] result);

        void AddUnit(IUnit unit);
        void RemoveUnit(IUnit unit);

        List<IArmy> GetEnemyArmies();
        void AddEnemyArmy(IArmy army);
    }
}