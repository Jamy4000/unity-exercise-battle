using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    public sealed class BattleUpdater : IArmiesHolder, I_UpdateOnly, I_Startable, 
        I_LateUpdateOnly, ISubscriber<BattleStartEvent>, ISubscriber<AllianceWonEvent>
    {
        private readonly Army[] _armies;

        private readonly BattleFSM _battleFSM;

        // IArmiesHolder
        public int ArmiesCount => _armies.Length;
        public Vector3 BattleCenter { get; private set; } = Vector3.zero;
        public bool HasStarted { get; set; }
        public Action<Army> ArmyDefeatedEvent { get; set; }

        public Army GetArmy(int armyIndex) => _armies[armyIndex];


        // TODO FSM parameters should be injected differently
        public BattleUpdater(Army[] armies, BattleStateData[] battleStatesData, BattleStateID defaultStateValue)
        {
            _armies = armies;

            MessagingSystem<BattleStartEvent>.Subscribe(this);
            MessagingSystem<AllianceWonEvent>.Subscribe(this);

            UnityServiceLocator.ServiceLocator.Global.Register<IArmiesHolder>(this);

            // the FSM throws the BattleStartEvent, so we need to create it at the end
            _battleFSM = CreateFSM(battleStatesData, defaultStateValue);
        }

        public void Start()
        {
            foreach (var army in _armies)
            {
                army.Start();
            }
        }

        public void ManualUpdate()
        {
            UpdateArmies();
            UpdateUnits();

            _battleFSM.ManualUpdate();
        }

        public void ManualLateUpdate()
        {
            // for now there is no need to thread this, since this is just removing units marked for deletion
            foreach (var army in _armies)
            {
                if (army.RemainingUnitsCount == 0)
                    continue;

                army.LateUpdate();
            }

            foreach (var army in _armies)
            {
                if (army.RemainingUnitsCount == 0)
                {
                    ArmyDefeatedEvent?.Invoke(army);
                }
            }

            _battleFSM.ManualLateUpdate();
        }

        public void Dispose()
        {
            GameUpdater.Unregister(this);

            MessagingSystem<BattleStartEvent>.Unsubscribe(this);
            MessagingSystem<AllianceWonEvent>.Unsubscribe(this);
        }

        // TODO parameter should be injected differently
        private BattleFSM CreateFSM(BattleStateData[] battleStatesData, BattleStateID defaultStateEnum)
        {
            List<BattleState> states = new(battleStatesData.Length);
            BattleState defaultState = null;

            for (int i = 0; i < battleStatesData.Length; i++)
            {
                BattleState state = battleStatesData[i].CreateStateInstance(this);
                states.Add(state);
                if (state.StateEnum == defaultStateEnum)
                    defaultState = state;
            }

            return new BattleFSM(defaultState, states);
        }

        private void UpdateArmies()
        {
            // we generate the tasks to update the armies (mainly to rebuild the KD-trees)
            var parallelResult = Parallel.ForEach(_armies, army => army.UpdateArmyData());

            while (!parallelResult.IsCompleted)
                continue;

            // We calculate the battle center based on armies centers
            BattleCenter = Vector3.zero;
            int remainingArmies = 0;
            foreach (var army in _armies)
            {
                if (army.RemainingUnitsCount == 0)
                    continue;

                remainingArmies++;
                BattleCenter += army.Center;
            }
            BattleCenter /= remainingArmies;
        }

        private void UpdateUnits()
        {
            var parallelResult = Parallel.ForEach(_armies, army => army.CalculateNewData());

            while (!parallelResult.IsCompleted)
                continue;

                // We apply the data on the main thread
            foreach (var army in _armies)
            {
                if (army.RemainingUnitsCount == 0)
                    continue;

                army.ApplyCalculatedData();
            }
        }

        public void OnEvent(BattleStartEvent evt)
        {
            GameUpdater.Register(this);
        }

        public void OnEvent(AllianceWonEvent evt)
        {
            GameUpdater.Unregister(this);
        }
    }
}