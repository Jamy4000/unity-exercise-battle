using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    public class UnitController : MonoBehaviour
    {
        [SerializeField]
        private UnitStateData[] _unitStatesData;

        [SerializeField]
        private UnitStateID _defaultState = UnitStateID.Idle;

        protected UnitFSM Fsm { get; private set; } = null;

        private void Awake()
        {
            InitializeFsm();
        }

        private void InitializeFsm()
        {
            List<UnitState> states = new List<UnitState>(_unitStatesData.Length);
            UnitState defaultState = null;

            for (int i = 0; i < _unitStatesData.Length; i++)
            {
                UnitState state = _unitStatesData[i].CreateStateInstance();
                states.Add(state);
                if ((UnitStateID)i == _defaultState)
                    defaultState = state;
            }

            Fsm = new UnitFSM(defaultState, states);
        }
    }
}