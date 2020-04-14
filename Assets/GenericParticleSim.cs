using UnityEngine;
using Unity.Mathematics;
public class GenericPhysarumSim<TParticle, TUniforms> : MonoBehaviour where TUniforms: IComputeUniforms {
    public TUniforms uniforms;
    public ComputeShader particleShader;
    public int particleSeedKernel = 0;
    public int particleSimKernel = 1;
    public ComputeShader indirectArgsShader;
    public ComputeShader colorDiffuseShader;
    public Vector2Int textureDims;
    public new Renderer renderer;
    
    ComputeBuffer pBufB;
    ComputeBuffer pBufA;
    ComputeBuffer counterBuf;
    ComputeBuffer indirectArgsBuf;

    DoubleBufferTexture _doubleBuffer;

    public int maxParticles = 8;
    public float decayRate = 0.04f;

    int _tParticleSize;
    int _tUniformsSize;


    void CacheStructSizes() {
        _tParticleSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(TParticle));
        _tUniformsSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(TUniforms));
    }

    void OnEnable() {
        CacheStructSizes();
        pBufB = new ComputeBuffer(maxParticles, _tParticleSize, ComputeBufferType.Append);
        pBufA = new ComputeBuffer(maxParticles, _tParticleSize, ComputeBufferType.Append);
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
        uniforms.Bind(particleShader);

        particleShader.DispatchIndirect(particleSimKernel, indirectArgsBuf, sizeof(int));

        ComputeBuffer.CopyCount(pBufB, indirectArgsBuf, 0);

        // flip buffers
        var tmp = pBufA;
        pBufA = pBufB;
        pBufB = tmp;

        _doubleBuffer.Flip();
        {
            colorDiffuseShader.SetFloat("_DecayRate", decayRate);
            colorDiffuseShader.SetTexture(0, "_ReadTexture", _doubleBuffer.read);
            colorDiffuseShader.SetTexture(0, "_WriteTexture", _doubleBuffer.write);
            colorDiffuseShader.GetKernelThreadGroupSizes(0, out var threadsX, out var threadsY, out var threadsZ);
            var threadGroupsX = _doubleBuffer.width / (int)threadsX;
            var threadGroupsY = _doubleBuffer.height / (int)threadsY;
            colorDiffuseShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        }
        renderer.material.SetTexture("_MainTex", _doubleBuffer.write);
    }
}

public interface IComputeUniforms {
    void Bind(ComputeShader shader);
}
