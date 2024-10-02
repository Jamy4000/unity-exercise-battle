using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    public sealed class BattleInstantiator : MonoBehaviour, IArmiesHolder, I_UpdateOnly, I_LateUpdateOnly,
        ISubscriber<BattleStartEvent>, ISubscriber<AllianceWonEvent>
    {
        /// <summary>
        /// Simple serialized struct to link an army to its spawn point
        /// </summary>
        [System.Serializable]
        private struct ArmySpawnParameters
        {
            [SerializeField]
            private Object _armyModel;
            public readonly IArmyModel ArmyModel => _armyModel as IArmyModel;

            // TODO We may want to let the user chose where the armies spawn
            [SerializeField]
            private BoxCollider _armySpawnBounds;
            public readonly Bounds GetSpawnBounds() => _armySpawnBounds.bounds;
        }

        // TODO We may want to get that from a scriptable object instead, in case we want to let player add/remove armies in the launch menu
        [SerializeField]
        private ArmySpawnParameters[] _armiesToSpawn;

        [Header("FSM Data"), SerializeField]
        private BattleStateData[] _battleStatesData;

        [SerializeField]
        private BattleStateID _defaultState = BattleStateID.OnGoing;

        private Army[] _armies;

        public Army GetArmy(int index) => _armies[index];
        public int ArmiesCount => _armies.Length;
        public Vector3 BattleCenter { get; private set; }

        private static readonly IStrategyUpdater[,] _strategyUpdaters = new IStrategyUpdater[IArmyModel.UnitLength, IStrategyUpdater.StrategyCount];

        private BattleFSM _battleFSM;

        void Awake()
        {
            _armies = new Army[_armiesToSpawn.Length];

            // For each army that should spawn on the map
            for (int armyIndex = 0; armyIndex < _armiesToSpawn.Length; armyIndex++)
            {
                ArmySpawnParameters armySpawnParam = _armiesToSpawn[armyIndex];
                _armies[armyIndex] = CreateArmy(armySpawnParam.ArmyModel, armySpawnParam.GetSpawnBounds());
            }

            // Second pass to feed the enemy armies inside each army
            for (int armyIndex = 0; armyIndex < _armies.Length; armyIndex++)
            {
                int allianceID = _armies[armyIndex].Model.AllianceID;

                for (int secondArmyIndex = 0; secondArmyIndex < _armies.Length; secondArmyIndex++)
                {
                    // If we are checking the army against itself, skip
                    if (armyIndex == secondArmyIndex)
                        continue;

                    // If the two armies have different alliance id, we mark them as enemies
                    if (_armies[secondArmyIndex].Model.AllianceID != allianceID)
                        _armies[armyIndex].AddEnemyArmy(_armies[secondArmyIndex]);
                }
            }

            UnityServiceLocator.ServiceLocator.ForSceneOf(this).Register(this);

            _battleFSM = CreateFSM();

            MessagingSystem<BattleStartEvent>.Subscribe(this);
            MessagingSystem<AllianceWonEvent>.Subscribe(this);

            GameUpdater.Register(this);
        }

        private void Start()
        {
            foreach (var army in _armies)
            {
                army.Start();
            }
        }

        private BattleFSM CreateFSM()
        {
            List<BattleState> states = new(_battleStatesData.Length);
            BattleState defaultState = null;

            for (int i = 0; i < _battleStatesData.Length; i++)
            {
                BattleState state = _battleStatesData[i].CreateStateInstance(this);
                states.Add(state);
                if (state.StateEnum == _defaultState)
                    defaultState = state;
            }

            return new BattleFSM(defaultState, states);
        }

        public void ManualUpdate()
        {
            List<Task> tasks = new List<Task>();

            int remainingArmies = 0;
            foreach (var army in _armies)
            {
                if (army.RemainingUnitsCount > 0)
                    remainingArmies++;

                tasks.Add(Task.Factory.StartNew(() => army.UpdateArmyData()));
            }
            Task.WaitAll(tasks.ToArray());

            BattleCenter = Vector3.zero;
            foreach (var army in _armies)
            {
                BattleCenter += army.Center;
            }
            BattleCenter /= remainingArmies;

            foreach (var army in _armies)
            {
                if (army.RemainingUnitsCount == 0)
                    continue;

                army.UpdateUnits();
            }

            _battleFSM.ManualUpdate();

            foreach (var army in _armies)
            {
                if (army.RemainingUnitsCount == 0)
                    continue;

                army.LateUpdate();
            }
        }

        public void ManualLateUpdate()
        {
            _battleFSM.ManualLateUpdate();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            foreach (var army in _armies)
            {
                if (army.RemainingUnitsCount == 0)
                    continue;

                Gizmos.color = army.Model.ArmyColor;
                Gizmos.DrawSphere(army.Center, 2f);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(BattleCenter, 4f);
        }

        private void OnDestroy()
        {
            GameUpdater.Unregister(this);

            MessagingSystem<BattleStartEvent>.Unsubscribe(this);
            MessagingSystem<AllianceWonEvent>.Unsubscribe(this);
        }

        private Army CreateArmy(IArmyModel armyModel, Bounds spawnBounds)
        {
            // TODO remove hard implementation
            Army army = new Army(armyModel, this);

            // For each type of unit in the game
            for (int unitTypeIndex = 0; unitTypeIndex < IArmyModel.UnitLength; unitTypeIndex++)
            {
                // If the current army has this type of unit in its rank
                if (armyModel.TryGetUnitModel((UnitType)unitTypeIndex, out IUnitModel unitModel))
                {
                    // We check if the strategy updaters were generated for this unit type
                    CheckForStrategyUpdaterCreation(unitModel);

                    // We create the amount of units provided in the Launch Menu
                    CreateUnitsForArmy(armyModel, army, unitModel, spawnBounds);
                }
            }

            return army;
        }

        private void CreateUnitsForArmy(IArmyModel armyModel, Army army, IUnitModel unitModel, Bounds spawnBounds)
        {
            // We spawn the amount of units of this type provided in the Launch Menu
            int maxUnitCount = armyModel.GetUnitCount(unitModel.UnitType);
            for (int unitIndex = 0; unitIndex < maxUnitCount; unitIndex++)
            {
                // We generate the parameters to create the Unit
                Vector3 position = DCLBattleUtils.GetRandomPosInBounds(spawnBounds);
                // Could randomize that or make them face a certain direction
                Quaternion rotation = Quaternion.identity;
                // Provides the reference to the strategy Updater that this unit will be using at the start of the battle, based on its army configuration
                IStrategyUpdater strategyUpdater = _strategyUpdaters[(int)unitModel.UnitType, (int)armyModel.Strategy];

                UnitCreationParameters parameters = new(position, rotation, army, unitModel, strategyUpdater);

                UnitBase newUnit = unitModel.InstantiateUnit(parameters);

                army.AddUnit(newUnit);
            }
        }

        private void CheckForStrategyUpdaterCreation(IUnitModel unitModel)
        {
            int unitTypeIndex = (int)unitModel.UnitType;

            // No need to recreate the strategy updaters multiple time, one object for all units of the same type using the same strategy is enough
            if (_strategyUpdaters[unitTypeIndex, 0] == null)
            {
                for (int strategyIndex = 0; strategyIndex < IStrategyUpdater.StrategyCount; strategyIndex++)
                {
                    // This will create extra stratyUpdaters that may not be necessary, though we may change design to have a dynamic strategy instead while the battle is ongoing
                    _strategyUpdaters[unitTypeIndex, strategyIndex] = unitModel.CreateStrategyUpdater((ArmyStrategy)strategyIndex);
                }
            }
        }

        public void OnEvent(AllianceWonEvent evt)
        {
            GameUpdater.Unregister(this);
        }

        public void OnEvent(BattleStartEvent evt)
        {
            GameUpdater.Register(this);
        }
    }
}