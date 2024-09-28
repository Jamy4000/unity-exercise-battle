using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class Army : IArmy
    {
        private readonly IArmyModel _model;
        private readonly List<UnitBase> _units = new();

        public Color ArmyColor => _model.ArmyColor;
        public ArmyStrategy Strategy => _model.Strategy;
        public int RemainingUnitsCount => _units.Count;


        public Army(IArmyModel model)
        {
            _model = model;
        }

        public void Update()
        {
            foreach (UnitBase unit in _units)
            {
                unit.ManualUpdate();
            }
        }

        public Vector3 CalculateCenterPoint()
        {
            return DCLBattleUtils.GetCenter(_units);
        }

        public void RemoveUnit(UnitBase unit)
        {
            _units.Remove(unit);
        }

        public void AddUnit(UnitBase unit)
        {
            _units.Add(unit);
        }
    }
}