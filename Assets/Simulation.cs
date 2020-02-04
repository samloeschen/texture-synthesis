using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Simulation : MonoBehaviour {

    public ComputeShader shader;
    public int seedKernel;
    public int simKernel;

    public Vector2Int textureDims;
    public new Renderer renderer;

    DoubleBuffer _doubleBuffer;


    void OnEnable() {
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
        shader.SetVector("_Resolution", resolution);
        shader.SetVector("_Mouse", mousePos);
        shader.SetFloat("_Time", Time.time);

        shader.SetTexture(seedKernel, "Read", _doubleBuffer.read);
        shader.SetTexture(seedKernel, "Write", _doubleBuffer.write);
        Dispatch(shader, seedKernel);

        _doubleBuffer.Flip();
        shader.SetTexture(simKernel, "Read", _doubleBuffer.read);
        shader.SetTexture(simKernel, "Write", _doubleBuffer.write);
        Dispatch(shader, simKernel);

        renderer.material.SetTexture("_MainTex", _doubleBuffer.write);
    }


    void Dispatch(ComputeShader shader, int kernel) {
        shader.GetKernelThreadGroupSizes(kernel, out var width, out var height, out var depth);
        var threadGroupsX = _doubleBuffer.width / (int)width;
        var threadGroupsY = _doubleBuffer.height / (int)height;
        shader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);
    }
}

public struct DoubleBuffer {
    public RenderTexture read;
    public RenderTexture write;

    public int width {
        get { return read.width; }
    }
    public int height {
        get { return read.height; }
    }

    public void Create(int width, int height) {
        read = MakeTexture(width, height);
        write = MakeTexture(width, height);
    }

    public void Bind(ComputeShader shader, int kernel) {

    }

    public void Flip() {
        var tmp = read;
        read = write;
        write = tmp;
    }

    RenderTexture MakeTexture(int width, int height) {
        var desc = new RenderTextureDescriptor();
        desc.colorFormat = RenderTextureFormat.BGRA32;
        desc.depthBufferBits = 0;
        desc.dimension = TextureDimension.Tex2D;
        desc.enableRandomWrite = true;
        desc.width = width;
        desc.height = height;
        desc.volumeDepth = 1;
        desc.msaaSamples = 1;

        var tex = new RenderTexture(desc);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        tex.Create();
        return tex;
    }
}
