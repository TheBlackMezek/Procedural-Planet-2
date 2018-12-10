using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;

public class HyperpositionSystem : ComponentSystem
{

    private int3 localOctant;



    protected override void OnCreateManager()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected override void OnUpdate()
    {
        float octantSize = HyperposStaticReferences.OctantSize;



        ComponentGroup camGroup = GetComponentGroup(typeof(Flycam), typeof(PrecisePosition), typeof(OctantPosition));

        EntityArray entityArray = camGroup.GetEntityArray();
        Entity camEntity = entityArray[0];

        ComponentDataArray<PrecisePosition> tempcposArray = camGroup.GetComponentDataArray<PrecisePosition>();
        PrecisePosition camPos = tempcposArray[0];
        ComponentDataArray<OctantPosition> coctposArray = camGroup.GetComponentDataArray<OctantPosition>();

        float3 coverPosF = math.floor(camPos.pos / octantSize);
        int3 coverPos = (int3)coverPosF;
        OctantPosition coctpos = coctposArray[0];

        //Debug.Log("POS:" + camPos.pos + " OCTPOS:" + coctpos.pos);

        if (coverPos.x != 0 || coverPos.y != 0 || coverPos.z != 0)
        {
            coctpos.pos += coverPos;
            float3 offsetAmt = coverPosF * octantSize;
            camPos = new PrecisePosition { pos = camPos.pos - offsetAmt };

            EntityManager.SetComponentData(camEntity, coctpos);
            EntityManager.SetComponentData(camEntity, camPos);
        }

        localOctant = coctpos.pos;



        ComponentGroup preciseGroup = GetComponentGroup(typeof(PrecisePosition), typeof(Position), typeof(OctantPosition));
        EntityArray pposEntityArray = preciseGroup.GetEntityArray();
        ComponentDataArray<PrecisePosition> pposArray = preciseGroup.GetComponentDataArray<PrecisePosition>();
        ComponentDataArray<OctantPosition> oposArray = preciseGroup.GetComponentDataArray<OctantPosition>();
        ComponentDataArray<Position> posArray = preciseGroup.GetComponentDataArray<Position>();

        for(int i = 0; i < pposArray.Length; ++i)
        {
            PrecisePosition ppos = pposArray[i];
            OctantPosition octpos = oposArray[i];
            float3 overPosF = math.floor(ppos.pos / octantSize);
            int3 overPos = (int3)overPosF;

            Entity entity = pposEntityArray[i];

            if (overPos.x != 0 || overPos.y != 0 || overPos.z != 0)
            {
                octpos.pos += overPos;
                float3 offsetAmt = overPosF * octantSize;
                ppos = new PrecisePosition { pos = ppos.pos - offsetAmt };

                EntityManager.SetComponentData(entity, octpos);
                EntityManager.SetComponentData(entity, ppos);
            }

            if (EntityManager.HasComponent<HyperdistantMarker>(entity))
            {
                float3 octOffset = octpos.pos - localOctant;

                posArray[i] = new Position { Value = camPos.pos + octOffset + (ppos.pos - camPos.pos) / octantSize };
            }
            else
            {
                float3 octOffset = octpos.pos - localOctant;

                posArray[i] = new Position { Value = ppos.pos + octOffset * octantSize };
            }

        }

        //ComponentGroup camGroup = GetComponentGroup(typeof(Position), typeof(Flycam));
        //
        //EntityArray entityArray = camGroup.GetEntityArray();
        //Entity camEntity = entityArray[0];
        //
        //ComponentDataArray<Position> cposArray = camGroup.GetComponentDataArray<Position>();
        //Position camPos = cposArray[0];
        //
        //float3 overPosF = math.floor(camPos.Value / octantSize);
        //int3 overPos = (int3)overPosF;
        //
        //if(overPos.x != 0 || overPos.y != 0 || overPos.z != 0)
        //{
        //    OctantPosition octpos;
        //
        //    if (EntityManager.HasComponent<OctantPosition>(camEntity))
        //    {
        //       octpos = EntityManager.GetComponentData<OctantPosition>(camEntity);
        //    }
        //    else
        //    {
        //        octpos = new OctantPosition();
        //        EntityManager.AddComponentData(camEntity, octpos);
        //    }
        //
        //    octpos.pos += overPos;
        //    float3 offsetAmt = overPosF * octantSize;
        //    camPos = new Position { Value = camPos.Value - offsetAmt };
        //
        //    EntityManager.SetComponentData(camEntity, octpos);
        //    EntityManager.SetComponentData(camEntity, camPos);
        //
        //    Debug.Log("POS:" + camPos.Value + " OCTPOS:" + octpos.pos);
        //
        //
        //
        //    ComponentGroup hyperposGroup = GetComponentGroup(typeof(Position), typeof(PrecisePosition));
        //    ComponentDataArray<Position> posArray = hyperposGroup.GetComponentDataArray<Position>();
        //
        //    for(int i = 0; i < posArray.Length; ++i)
        //    {
        //        posArray[i] = new Position { Value = posArray[i].Value - offsetAmt };
        //    }
        //}
        //
        //
        //
        //ComponentGroup hyperdistGroup = GetComponentGroup(typeof(Position), typeof(HyperdistantMarker));
        //ComponentDataArray<Position> hdposArray = hyperdistGroup.GetComponentDataArray<Position>();
        //ComponentDataArray<HyperdistantMarker> hyperdistArray = hyperdistGroup.GetComponentDataArray<HyperdistantMarker>();
        //
        //for(int i = 0; i < hdposArray.Length; ++i)
        //{
        //    hdposArray[i] = new Position { Value = camPos.Value + (hyperdistArray[i].precisePos - camPos.Value) / octantSize };
        //}
    }

}
