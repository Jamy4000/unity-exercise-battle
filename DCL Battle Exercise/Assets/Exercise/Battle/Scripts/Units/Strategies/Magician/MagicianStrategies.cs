using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    public sealed class MagicianBasicStrategyUpdater : IStrategyUpdater, I_Startable
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Basic;

        public bool HasStarted { get; set; }

        private IArmiesHolder _armiesHolder;

        public MagicianBasicStrategyUpdater()
        {
            GameUpdater.Register(this);
        }

        public void Start()
        {
            _armiesHolder = UnityServiceLocator.ServiceLocator.Global.Get<IArmiesHolder>();
            GameUpdater.Unregister(this);
        }

        public TargetInfo UpdateStrategy(UnitData unitData, out Vector3 strategyMovement)
        {
            var unitArmy = _armiesHolder.GetArmy(unitData.ArmyID);

            TargetInfo target = unitArmy.GetClosestEnemy(unitData.Position, out float distance);

            // normalizing
            strategyMovement = (target.Position - unitData.Position) / distance;
            strategyMovement.Scale(IStrategyUpdater.FlatScale);
            return target;
        }
    }

    public sealed class MagicianDefensiveStrategyUpdater : IStrategyUpdater
    {
        public ArmyStrategy ArmyStrategy => ArmyStrategy.Defensive;

        private static readonly Quaternion _flankRotation = Quaternion.Euler(0f, 90f, 0f);

        public TargetInfo UpdateStrategy(UnitData unitData, out Vector3 strategyMovement)
        {
            strategyMovement = Vector3.zero;
            return default;
            /*
            Vector3 moveDirection = Vector3.zero;

            // We get the enemies' center point
            List<Army> opponentsArmies = unitToUpdate.Army.GetEnemyArmies();
            Vector3 opponentsArmiesCenter = Vector3.zero;
            for (int i = 0; i < opponentsArmies.Count; i++)
            {
                opponentsArmiesCenter += opponentsArmies[i].Center;
            }
            opponentsArmiesCenter /= opponentsArmies.Count;

            // If we are further away than attack range; we move toward the enemies' center
            // TODO why are we only using X here ?
            float unitPositionX = unitToUpdate.Position.x;
            float distToEnemyX = Mathf.Abs(opponentsArmiesCenter.x - unitPositionX);

            if (distToEnemyX > unitToUpdate.AttackRange)
            {
                if (opponentsArmiesCenter.x < unitPositionX)
                    moveDirection += Vector3.left;
                
                if (opponentsArmiesCenter.x > unitPositionX)
                    moveDirection += Vector3.right;
            }

            // We check who the closest enemy is
            UnitBase closestEnemy = unitToUpdate.Army.GetClosestEnemy(unitToUpdate.Position, out float distance);

            Vector3 toNearest = Vector3.Normalize(closestEnemy.Position - unitToUpdate.Position);
            toNearest.Scale(IStrategyUpdater.FlatScale);

            // if the unit is within attack range
            if (distance < unitToUpdate.AttackRange)
            {
                Vector3 flank = _flankRotation * toNearest;
                moveDirection += Vector3.Normalize(-(toNearest + flank));

                // no need to call attack if the unit is further than attack range
                unitToUpdate.Target = closestEnemy;
            }
            else
            {
                moveDirection += Vector3.Normalize(toNearest);
            }

            return Vector3.Normalize(moveDirection);
            */
        }
    }
}