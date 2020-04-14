using UnityEngine;
using Unity.Mathematics;
public class Physarum1: GenericPhysarumSim<PhysParticle1, PhysUniforms1> { }

[System.Serializable]
public struct PhysParticle1 {
    public float2 position;
    public float heading;
}

[System.Serializable]
public struct PhysUniforms1: IComputeUniforms {
    public float sensorAngle;
    public float turnRate;
    public float sensorDist;
    public float colorDistThreshold;
    public float moveSpeed;
    public void Bind(ComputeShader shader) {
        shader.SetFloat("_SensorAngle", sensorAngle);
        shader.SetFloat("_TurnRate", turnRate * Time.deltaTime);
        shader.SetFloat("_SensorDist", sensorDist);
        shader.SetFloat("_ColorDistThreshold", colorDistThreshold * Time.deltaTime);
        shader.SetFloat("_MoveSpeed", moveSpeed * Time.deltaTime);
    }
}
