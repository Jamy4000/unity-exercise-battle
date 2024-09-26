using System.Collections.Generic;
using UnityEngine;

public sealed class Army
{
    private Color _color;
    private readonly List<UnitBase> _units = new();

    public Army(Color color, List<UnitBase> units)
    {
        _color = color;
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