using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ParticleSimulation : MonoBehaviour {
    public ComputeShader particleShader;
    public ComputeShader indirectArgsShader;
    public ComputeShader colorDiffuseShader;

    public int particleSeedKernel = 0;
    public int particleSimKernel = 1;


    public Vector2Int textureDims;
    public new Renderer renderer;


    public ComputeBuffer pBufB;
    public ComputeBuffer pBufA;

    public ComputeBuffer counterBuf;
    public ComputeBuffer indirectArgsBuf;

    DoubleBufferTexture _doubleBuffer;

    public int maxParticles = 8;

    void OnEnable() {
        pBufB = new ComputeBuffer(maxParticles, PARTICLE_SIZE, ComputeBufferType.Append);
        pBufA = new ComputeBuffer(maxParticles, PARTICLE_SIZE, ComputeBufferType.Append);
        pBufB.SetCounterValue(0);
        pBufA.SetCounterValue(0);

        counterBuf = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Counter);

        indirectArgsBuf = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        var args = new int[] {0, 0, 0, 0 };
        indirectArgsBuf.SetData(args);

        _doubleBuffer.Create(textureDims.x, textureDims.y);
    }

    void Update() {
        Vector2 mousePos = new Vector2 {
            x = Input.mousePosition.x / (float)Screen.width,
            y = Input.mousePosition.y / (float)Screen.height
        };
        Vector2 resolution = new Vector2 {
            x = _doubleBuffer.width,
            y = _doubleBuffer.height
        };

        Shader.SetGlobalVector("_Resolution", new Vector4 {
            x = resolution.x,
            y = resolution.y,
            z = 1f / resolution.x,
            w = 1f / resolution.y
        });
        Shader.SetGlobalVector("_Mouse", mousePos);
        Shader.SetGlobalFloat("_Time", Time.time);
        Shader.SetGlobalFloat("_DeltaTime", Time.deltaTime);

        if (Input.GetMouseButton(0)) {
            particleShader.SetInt("_MaxParticles", maxParticles);
            particleShader.SetBuffer(particleSeedKernel, "_CounterBuf", indirectArgsBuf);
            particleShader.SetBuffer(particleSeedKernel, "_WriteParticles", pBufA);
            particleShader.Dispatch(particleSeedKernel, 1, 1, 1);
        }

        ComputeBuffer.CopyCount(pBufA, indirectArgsBuf, 0);
        indirectArgsShader.SetBuffer(0, "_IndirectArgsBuf", indirectArgsBuf);
        indirectArgsShader.Dispatch(0, 1, 1, 1);

        particleShader.SetBuffer(particleSimKernel, "_CounterBuf", indirectArgsBuf);
        particleShader.SetBuffer(particleSimKernel, "_ReadParticles", pBufA);
        particleShader.SetBuffer(particleSimKernel, "_WriteParticles", pBufB);
        particleShader.SetTexture(particleSimKernel, "_ReadTexture", _doubleBuffer.read);
        particleShader.SetTexture(particleSimKernel, "_WriteTexture", _doubleBuffer.write);
        particleShader.DispatchIndirect(particleSimKernel, indirectArgsBuf, sizeof(int));

        ComputeBuffer.CopyCount(pBufB, indirectArgsBuf, 0);

        // flip buffers
        var tmp = pBufA;
        pBufA = pBufB;
        pBufB = tmp;

        _doubleBuffer.Flip();
        {
            colorDiffuseShader.SetTexture(0, "_ReadTexture", _doubleBuffer.read);
            colorDiffuseShader.SetTexture(0, "_WriteTexture", _doubleBuffer.write);
            colorDiffuseShader.GetKernelThreadGroupSizes(0, out var threadsX, out var threadsY, out var threadsZ);
            var threadGroupsX = _doubleBuffer.width / (int)threadsX;
            var threadGroupsY = _doubleBuffer.height / (int)threadsY;
            colorDiffuseShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        }

        renderer.material.SetTexture("_MainTex", _doubleBuffer.write);
    }

    const int PARTICLE_SIZE = sizeof(float) * 7;
    [System.Serializable]
    public struct Particle {
        public float4 posVel;
        public float2 position;
        public float heading;
    }
}

