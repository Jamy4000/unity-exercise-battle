
using System;

public interface IUnitModel
{
    int GetUnitsCount();
    void SetUnitsCount(int newUnitCount);

    string GetUnitsName();

    UnitType GetUnitsType();

    UnitBase GetUnitsPrefab();

    Action<IUnitModel> OnUnitsChanged { get; set; }
}
