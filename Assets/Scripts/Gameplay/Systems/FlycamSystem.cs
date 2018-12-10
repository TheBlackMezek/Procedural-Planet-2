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
        ComponentGroup nodeGroup = GetComponentGroup(typeof(Flycam), typeof(Position), typeof(Rotation));

        EntityArray entityArray = nodeGroup.GetEntityArray();
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



        Flycam flyCam = flycamArray[0];
        flyCam.moveSpeed += flyCam.moveSpeed * flycamArray[0].moveSpeedChangeMultiplier * Input.GetAxis("Mouse ScrollWheel");
        EntityManager.SetComponentData(entityArray[0], flyCam);

        SystemStaticReferences.SpeedText.text = flyCam.moveSpeed.ToString() + " m/s";

        float3 forward = math.forward(rotArray[0].Value);
        float3 right = MathUtils.right(rotArray[0].Value);
        float3 up = MathUtils.up(rotArray[0].Value);

        float dt = Time.deltaTime;

        float3 posChange = forward * Input.GetAxis("ForeBack") * dt * flyCam.moveSpeed
                         + right * Input.GetAxis("Horizontal") * dt * flyCam.moveSpeed
                         + up * Input.GetAxis("Vertical") * dt * flyCam.moveSpeed;

        posArray[0] = new Position() { Value = posArray[0].Value + posChange };
    }

}
