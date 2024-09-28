using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class Army : IArmy
    {
        public IArmyModel Model { get; }
        public int RemainingUnitsCount => _units.Count;

        private readonly List<IUnit> _units;


        public Army(IArmyModel model)
        {
            Model = model;

            // pre-allocate the list
            int armySize = 0;
            for (int i = 0; i < IArmyModel.UnitLength; i++)
            {
                armySize += model.GetUnitCount((UnitType)i);
            }
            _units = new(armySize);
        }

        public void Update()
        {
            /*
            foreach (IUnit unit in _units)
            {
                unit.ManualUpdate();
            }
            */
        }

        public Vector3 CalculateCenterPoint()
        {
            return DCLBattleUtils.GetCenter(_units);
        }

        public void RemoveUnit(IUnit unit)
        {
            _units.Remove(unit);
        }

        public void AddUnit(IUnit unit)
        {
            _units.Add(unit);
        }
    }
}