using System;
using Unity.Entities;
using UnityEngine;

namespace ECS_Experiments
{

    [Serializable]
    public struct TestSpawn : ISharedComponentData
    {
        public GameObject prefab;
        public int spawnCount;
        public float spawnRange;
    }

    public class TestSpawnComponent : SharedComponentDataWrapper<TestSpawn> { }

}
