using System;

namespace DCLBattle.LaunchMenu
{
    public interface IUnitModel
    {
        int GetUnitsCount();
        void SetUnitsCount(int newUnitCount);
        string GetUnitsName();
        UnitType GetUnitsType();

        Action<IUnitModel> OnUnitsCountChanged { get; set;  }
    }

    public class UnitModel : IUnitModel
    {
        private int _unitCount;
        // TODO Could add the ability to change the name of the army, as well as the color?
        private readonly string _unitName;
        private readonly UnitType _unitType;

        public Action<IUnitModel> OnUnitsCountChanged { get; set; }

        public UnitModel(int unitCount, string unitName, UnitType unitType)
        {
            _unitCount = unitCount;
            _unitName = unitName;
            _unitType = unitType;
        }


        public int GetUnitsCount()
        {
            return _unitCount;
        }

        public void SetUnitsCount(int newUnitCount)
        {
            _unitCount = newUnitCount;
            OnUnitsCountChanged?.Invoke(this);
        }

        public string GetUnitsName()
        {
            return _unitName;
        }

        public UnitType GetUnitsType()
        {
            return _unitType;
        }
    }
}