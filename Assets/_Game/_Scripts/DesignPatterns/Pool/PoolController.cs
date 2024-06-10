using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using VinhLB;

namespace _Game.DesignPattern
{
    public class PoolController : Singleton<PoolController>
    {
        [Title("Amounts")]
        [SerializeField]
        private PoolAmount[] _poolAmounts;
        [SerializeField]
        private ParticleAmount[] _particleAmounts;
        
        [Title("Spawners")]
        [SerializeField]
        private FishSpawner _fishSpawner;

        public FishSpawner FishSpawner => _fishSpawner;

        public void Awake()
        {
            for (int i = 0; i < _particleAmounts.Length; i++)
                ParticlePool.Preload(_particleAmounts[i].prefab, _particleAmounts[i].amount, _particleAmounts[i].root);

            for (int i = 0; i < _poolAmounts.Length; i++)
                SimplePool.Preload(_poolAmounts[i].prefab, _poolAmounts[i].amount, _poolAmounts[i].root, _poolAmounts[i].collect, _poolAmounts[i].clamp);

        }
    }
}
