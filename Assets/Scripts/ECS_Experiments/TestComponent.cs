using System;
using Unity.Entities;
using UnityEngine;

namespace ECS_Experiments
{

    [Serializable]
    public struct Test : ISharedComponentData
    {
        public float moveSpeed;
    }

    public class TestComponent : SharedComponentDataWrapper<Test> { }

}
