using System;
using Unity.Entities;
using UnityEngine;

namespace ECS_Experiments
{

    [Serializable]
    public struct TestSpawn : ISharedComponentData
    {
        public GameObject prefab;
    }

    public class TestSpawnComponent : SharedComponentDataWrapper<TestSpawn> { }

}
