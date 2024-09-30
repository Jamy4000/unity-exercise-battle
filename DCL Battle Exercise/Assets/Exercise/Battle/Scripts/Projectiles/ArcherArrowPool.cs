using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    public sealed class ArcherArrowPool : GenericPoolHelper<IProjectile>
    {
        private readonly Object _arrowPrefab;

        public ArcherArrowPool(Object arrowPrefab, int minPoolSize, int maxPoolSize, bool shouldCheckCollection = false) : 
            base(minPoolSize, maxPoolSize, shouldCheckCollection)
        {
            _arrowPrefab = arrowPrefab;
        }

        protected override IProjectile CreatePooledItem()
        {
            return Object.Instantiate(_arrowPrefab) as IProjectile;
        }
    }
}