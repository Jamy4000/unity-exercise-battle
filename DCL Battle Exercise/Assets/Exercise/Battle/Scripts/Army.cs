using System.Collections.Generic;
using UnityEngine;

public sealed class Army
{
    private readonly Color _color;
    private readonly ArmyStrategy _strategy;
    private readonly List<UnitBase> _units = new();

    public Army(Color color, ArmyStrategy strategy, List<UnitBase> units)
    {
        _color = color;
        _strategy = strategy;
        _units = units;
    }

    public void Update()
    {
        foreach (UnitBase unit in _units)
        {
            unit.ManualUpdate();
        }
    }

    public Color GetColor()
    {
        return _color;
    }

    public ArmyStrategy GetStrategy()
    {
        return _strategy;
    }

    public Army GetEnemyArmy()
    {
        return null;
    }
    
    public List<UnitBase> GetUnits()
    {
        return _units;
    }

    public void RemoveUnit(UnitBase unit)
    {
        _units.Remove(unit);
    }
}