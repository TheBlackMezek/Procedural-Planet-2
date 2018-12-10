using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;

public class FlycamSystem : ComponentSystem
{
    
    protected override void OnCreateManager()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected override void OnUpdate()
    {
        ComponentGroup nodeGroup = GetComponentGroup(typeof(Flycam), typeof(PrecisePosition), typeof(OctantPosition), typeof(Rotation));

        EntityArray tempEntityArray = nodeGroup.GetEntityArray();
        Entity[] entityArray = new Entity[tempEntityArray.Length];
        for (int i = 0; i < entityArray.Length; ++i)
            entityArray[i] = tempEntityArray[i];

        ComponentDataArray<Flycam> tempFlycamArray = nodeGroup.GetComponentDataArray<Flycam>();
        Flycam[] flycamArray = new Flycam[tempFlycamArray.Length];
        for (int i = 0; i < flycamArray.Length; ++i)
            flycamArray[i] = tempFlycamArray[i];

        ComponentDataArray<PrecisePosition> tempPosArray = nodeGroup.GetComponentDataArray<PrecisePosition>();
        PrecisePosition[] posArray = new PrecisePosition[tempPosArray.Length];
        for (int i = 0; i < tempPosArray.Length; ++i)
            posArray[i] = tempPosArray[i];

        ComponentDataArray<OctantPosition> tempOPosArray = nodeGroup.GetComponentDataArray<OctantPosition>();
        OctantPosition[] oposArray = new OctantPosition[tempOPosArray.Length];
        for (int i = 0; i < tempOPosArray.Length; ++i)
            oposArray[i] = tempOPosArray[i];

        ComponentDataArray<Rotation> tempRotArray = nodeGroup.GetComponentDataArray<Rotation>();
        Rotation[] rotArray = new Rotation[tempRotArray.Length];
        for (int i = 0; i < tempRotArray.Length; ++i)
            rotArray[i] = tempRotArray[i];



        

        quaternion pitchChange = quaternion.RotateX(Input.GetAxis("Mouse Y") * -flycamArray[0].mouseSensitivity);
        quaternion yawChange = quaternion.RotateY(Input.GetAxis("Mouse X") * flycamArray[0].mouseSensitivity);
        quaternion rollChange = quaternion.RotateZ(Input.GetAxis("Roll") * -flycamArray[0].rollSensitivity);

        quaternion rotChange = math.mul(pitchChange, yawChange);
        rotChange = math.mul(rollChange, rotChange);

        rotArray[0] = new Rotation { Value = math.mul(rotArray[0].Value, rotChange) };



        Flycam flyCam = flycamArray[0];
        float axisScrollWheel = Input.GetAxis("Mouse ScrollWheel");
        float octantSize = HyperposStaticReferences.OctantSize;

        float ospeed = flyCam.octMoveSpeed;
        float ospeedChange = flyCam.octMoveSpeed * flycamArray[0].moveSpeedChangeMultiplier * axisScrollWheel;
        ospeed += ospeedChange;
        float pspeedChange = ospeed % 1f;
        ospeed -= pspeedChange;
        pspeedChange *= octantSize;
        pspeedChange += flyCam.moveSpeed * flycamArray[0].moveSpeedChangeMultiplier * axisScrollWheel;

        flyCam.moveSpeed += pspeedChange;
        int overSpeed = (int)(flyCam.moveSpeed / octantSize);
        flyCam.moveSpeed -= overSpeed * octantSize;
        flyCam.octMoveSpeed = (int)ospeed + overSpeed;

        EntityManager.SetComponentData(entityArray[0], flyCam);

        SystemStaticReferences.SpeedText.text = flyCam.moveSpeed.ToString() + " m/s  "
                                              + flyCam.octMoveSpeed.ToString() + " oct/s";



        float3 forward = math.forward(rotArray[0].Value);
        float3 right = MathUtils.right(rotArray[0].Value);
        float3 up = MathUtils.up(rotArray[0].Value);

        float dt = Time.deltaTime;

        float axisForeBack = Input.GetAxis("ForeBack");
        float axisHorizontal = Input.GetAxis("Horizontal");
        float axisVertical = Input.GetAxis("Vertical");

        float3 octChangeInitial = forward * axisForeBack * dt * flyCam.octMoveSpeed
                                + right * axisHorizontal * dt * flyCam.octMoveSpeed
                                + up * axisVertical * dt * flyCam.octMoveSpeed;

        float3 posChange = octChangeInitial % 1f;

        int3 octChange = (int3)(octChangeInitial - posChange);

        posChange *= octantSize;
        posChange += forward * axisForeBack * dt * flyCam.moveSpeed
                   + right * axisHorizontal * dt * flyCam.moveSpeed
                   + up * axisVertical * dt * flyCam.moveSpeed;

        int3 overPos = (int3)(posChange / octantSize);
        posChange -= (float3)overPos * octantSize;
        octChange += overPos;

        posArray[0] = new PrecisePosition() { pos = posArray[0].pos + posChange };
        oposArray[0] = new OctantPosition() { pos = oposArray[0].pos + octChange };

        EntityManager.SetComponentData(entityArray[0], posArray[0]);
        EntityManager.SetComponentData(entityArray[0], rotArray[0]);
        EntityManager.SetComponentData(entityArray[0], oposArray[0]);
    }

}













//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.Jobs;
//using UnityEngine;
//
//public class FlycamSystem : ComponentSystem
//{
//
//    protected override void OnCreateManager()
//    {
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//    }
//
//    protected override void OnUpdate()
//    {
//        ComponentGroup nodeGroup = GetComponentGroup(typeof(Flycam), typeof(Position), typeof(Rotation));
//
//        EntityArray entityArray = nodeGroup.GetEntityArray();
//        ComponentDataArray<Flycam> flycamArray = nodeGroup.GetComponentDataArray<Flycam>();
//        ComponentDataArray<Position> posArray = nodeGroup.GetComponentDataArray<Position>();
//        ComponentDataArray<Rotation> rotArray = nodeGroup.GetComponentDataArray<Rotation>();
//        //Position[] nodeArray = new Position[posArray.Length];
//        //for (int i = 0; i < posArray.Length; ++i)
//        //    nodeArray[i] = posArray[i];
//
//        quaternion pitchChange = quaternion.RotateX(Input.GetAxis("Mouse Y") * -flycamArray[0].mouseSensitivity);
//        quaternion yawChange = quaternion.RotateY(Input.GetAxis("Mouse X") * flycamArray[0].mouseSensitivity);
//        quaternion rollChange = quaternion.RotateZ(Input.GetAxis("Roll") * -flycamArray[0].rollSensitivity);
//
//        quaternion rotChange = math.mul(pitchChange, yawChange);
//        rotChange = math.mul(rollChange, rotChange);
//
//        rotArray[0] = new Rotation { Value = math.mul(rotArray[0].Value, rotChange) };
//
//
//
//        Flycam flyCam = flycamArray[0];
//        flyCam.moveSpeed += flyCam.moveSpeed * flycamArray[0].moveSpeedChangeMultiplier * Input.GetAxis("Mouse ScrollWheel");
//        EntityManager.SetComponentData(entityArray[0], flyCam);
//
//        SystemStaticReferences.SpeedText.text = flyCam.moveSpeed.ToString() + " m/s";
//
//        float3 forward = math.forward(rotArray[0].Value);
//        float3 right = MathUtils.right(rotArray[0].Value);
//        float3 up = MathUtils.up(rotArray[0].Value);
//
//        float dt = Time.deltaTime;
//
//        float3 posChange = forward * Input.GetAxis("ForeBack") * dt * flyCam.moveSpeed
//                         + right * Input.GetAxis("Horizontal") * dt * flyCam.moveSpeed
//                         + up * Input.GetAxis("Vertical") * dt * flyCam.moveSpeed;
//
//        posArray[0] = new Position() { Value = posArray[0].Value + posChange };
//    }
//
//}
