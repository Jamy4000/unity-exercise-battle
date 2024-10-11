using System.Collections.Generic;
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

        public Army GetArmy(int armyIndex) => _armies[armyIndex];


        // TODO FSM parameters should be injected differently
        public BattleUpdater(Army[] armies, IServiceLocator serviceLocator, BattleStateData[] battleStatesData, BattleStateID defaultStateValue)
        {
            _armies = armies;
            serviceLocator.AddService(this as IArmiesHolder);

            MessagingSystem<BattleStartEvent>.Subscribe(this);
            MessagingSystem<AllianceWonEvent>.Subscribe(this);

            // the FSM throws the BattleStartEvent, so we need to create it at the end
            _battleFSM = CreateFSM(battleStatesData, defaultStateValue, serviceLocator);
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
            foreach (var army in _armies)
            {
                if (army.RemainingUnitsCount == 0)
                    continue;

                army.LateUpdate();
            }

            _battleFSM.LateUpdate();
        }

        public void Dispose()
        {
            foreach (var army in _armies)
            {
                army.Dispose();
            }

            GameUpdater.Unregister(this);

            MessagingSystem<BattleStartEvent>.Unsubscribe(this);
            MessagingSystem<AllianceWonEvent>.Unsubscribe(this);
        }

        private BattleFSM CreateFSM(BattleStateData[] battleStatesData, BattleStateID defaultStateEnum, IServiceLocator serviceLocator)
        {
            List<BattleState> states = new(battleStatesData.Length);
            BattleState defaultState = null;

            for (int i = 0; i < battleStatesData.Length; i++)
            {
                BattleState state = battleStatesData[i].CreateStateInstance(serviceLocator);
                states.Add(state);
                if (state.StateEnum == defaultStateEnum)
                    defaultState = state;
            }

            return new BattleFSM(defaultState, states);
        }

        private void UpdateArmies()
        {
            // we generate the tasks to update the armies (mainly to rebuild the KD-trees)
            List<Task> tasks = new(ArmiesCount);
            int remainingArmies = 0;
            foreach (var army in _armies)
            {
                if (army.RemainingUnitsCount > 0)
                {
                    remainingArmies++;
                    tasks.Add(Task.Factory.StartNew(() => army.UpdateArmyData()));
                }
            }
            Task.WaitAll(tasks.ToArray());

            // We calculate the battle center based on armies centers
            BattleCenter = Vector3.zero;
            foreach (var army in _armies)
            {
                BattleCenter += army.Center;
            }
            BattleCenter /= remainingArmies;
        }

        private void UpdateUnits()
        {
            foreach (var army in _armies)
            {
                if (army.RemainingUnitsCount == 0)
                    continue;

                army.UpdateUnits();
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

        public void Initialize(IServiceLocator serviceLocator)
        {
        }
    }
}