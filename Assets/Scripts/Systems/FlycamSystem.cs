using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

public class FlycamSystem : ComponentSystem
{

    protected override void OnCreateManager()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected override void OnUpdate()
    {
        ComponentGroup nodeGroup = GetComponentGroup(typeof(Flycam), typeof(Position), typeof(Rotation));

        ComponentDataArray<Flycam> flycamArray = nodeGroup.GetComponentDataArray<Flycam>();
        ComponentDataArray<Position> posArray = nodeGroup.GetComponentDataArray<Position>();
        ComponentDataArray<Rotation> rotArray = nodeGroup.GetComponentDataArray<Rotation>();
        //Position[] nodeArray = new Position[posArray.Length];
        //for (int i = 0; i < posArray.Length; ++i)
        //    nodeArray[i] = posArray[i];

        quaternion pitchChange = quaternion.RotateX(Input.GetAxis("Mouse Y") * -flycamArray[0].mouseSensitivity);
        quaternion yawChange = quaternion.RotateY(Input.GetAxis("Mouse X") * flycamArray[0].mouseSensitivity);
        quaternion rollChange = quaternion.RotateZ(Input.GetAxis("Roll") * -flycamArray[0].rollSensitivity);

        quaternion rotChange = math.mul(pitchChange, yawChange);
        rotChange = math.mul(rollChange, rotChange);

        rotArray[0] = new Rotation { Value = math.mul(rotArray[0].Value, rotChange) };


        
        float3 forward = math.forward(rotArray[0].Value);
        float3 right = MathUtils.right(rotArray[0].Value);
        float3 up = MathUtils.up(rotArray[0].Value);

        float3 posChange = forward * Input.GetAxis("ForeBack")
                         + right * Input.GetAxis("Horizontal")
                         + up * Input.GetAxis("Vertical");

        posArray[0] = new Position() { Value = posArray[0].Value + posChange };
    }

}
