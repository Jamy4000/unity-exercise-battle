using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class Army : IArmy
    {
        private readonly Color _color;
        private readonly ArmyStrategy _strategy;
        private readonly List<UnitBase> _units = new();

        public Color ArmyColor => _color;
        public ArmyStrategy Strategy => _strategy;
        public int RemainingUnitsCount => _units.Count;


        public Army(Color color, ArmyStrategy strategy)
        {
            _color = color;
            _strategy = strategy;
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
            return Utils.GetCenter(_units);
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