using UnityEngine;
using Unity.Mathematics;
public class Contagion1: GenericPhysarumSim<ContagionParticle1, ContagionUniforms1> { }

[System.Serializable]
public struct ContagionParticle1 {
    public float2 position;
    public float heading;
    public float contagion;
}

[System.Serializable]
public struct ContagionUniforms1: IComputeUniforms {
    public float normalSensorAngle;
    public float infectedSensorAngle;
    [Space(10)]
    public float normalTurnRate;
    public float infectedTurnRate;
    [Space(10)]
    public float normalSensorDist;
    public float infectedSensorDist;
    [Space(10)]
    public float normalMoveSpeed;
    public float infectedMoveSpeed;
    public void Bind(ComputeShader shader) {
        shader.SetFloat("_NormalSensorAngle", normalSensorAngle);
        shader.SetFloat("_InfectedSensorAngle", infectedSensorAngle);

        shader.SetFloat("_NormalTurnRate", normalTurnRate * Time.deltaTime);
        shader.SetFloat("_InfectedTurnRate", infectedTurnRate * Time.deltaTime);

        shader.SetFloat("_NormalSensorDist", normalSensorDist);
        shader.SetFloat("_InfectedSensorDist", infectedSensorDist);

        shader.SetFloat("_NormalMoveSpeed", normalMoveSpeed * Time.deltaTime);
        shader.SetFloat("_InfectedMoveSpeed", infectedMoveSpeed * Time.deltaTime);
    }
}
