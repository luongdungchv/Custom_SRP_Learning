using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class GPUStaticRenderGroup
{
    public static int test;
    protected Material material;
    protected Mesh mesh;
    protected List<GPUStaticRenderer> listRenderer;
    protected List<StaticInstanceData> instancesData;

    private ComputeBuffer instanceBuffer, argsBuffer;

    public bool IsBufferSynchronized => this.listRenderer.Count == this.instancesData.Count && instanceBuffer.count > 0;

    public GPUStaticRenderGroup()
    {
        listRenderer = new List<GPUStaticRenderer>();
        instancesData = new List<StaticInstanceData>();
    }
    ~GPUStaticRenderGroup()
    {
        instanceBuffer?.Dispose();
        argsBuffer?.Dispose();
    }

    public void Log(){
        Debug.Log((instancesData.Count, listRenderer.Count, instanceBuffer.count, argsBuffer));
    }

    public StaticInstanceData[] GetData(){
        var result = new StaticInstanceData[5];
        this.instanceBuffer.GetData(result);
        return result;
    }

    public void RegisterRenderer(GPUStaticRenderer renderer, bool recreateBuffer = false)
    {
        this.listRenderer ??= new List<GPUStaticRenderer>();

        renderer.SetIndex(this.listRenderer.Count, this);
        this.listRenderer.Add(renderer);

        if (recreateBuffer)
        {
            var data = new StaticInstanceData()
            {
                AABBCenter = renderer.GetComponent<BoxCollider>().bounds.center,
                AABBExtents = renderer.GetComponent<BoxCollider>().bounds.extents,
                trs = Matrix4x4.TRS(renderer.transform.position, renderer.transform.rotation, renderer.transform.lossyScale)
            };
            this.instancesData.Add(data);

            this.instanceBuffer.Dispose();
            this.instanceBuffer = new ComputeBuffer(listRenderer.Count, StaticInstanceData.Size);
            this.instanceBuffer.SetData(this.instancesData);
        }
    }

    public void SetMeshAndMaterial(Mesh mesh, Material material)
    {
        this.mesh = mesh;
        this.material = material;
    }

    public void InitBuffers()
    {
        if (argsBuffer != null) argsBuffer.Dispose();
        this.argsBuffer = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
        uint[] args = {
            this.mesh.GetIndexCount(0),
            (uint)this.listRenderer.Count,
            this.mesh.GetIndexStart(0),
            this.mesh.GetBaseVertex(0),
            0
        };
        this.argsBuffer.SetData(args);

        this.instancesData = new List<StaticInstanceData>();
        foreach (var renderer in this.listRenderer)
        {
            var data = new StaticInstanceData()
            {
                AABBCenter = renderer.GetComponent<BoxCollider>().bounds.center,
                AABBExtents = renderer.GetComponent<BoxCollider>().bounds.extents,
                trs = Matrix4x4.TRS(renderer.transform.position, renderer.transform.rotation, renderer.transform.lossyScale)
            };
            this.instancesData.Add(data);
            Debug.Log(data.trs);
        }

        if (instanceBuffer != null) instanceBuffer.Dispose();
        this.instanceBuffer = new ComputeBuffer(listRenderer.Count, StaticInstanceData.Size);
        this.instanceBuffer.SetData(this.instancesData);

        this.BindBuffers();
    }

    public void BindBuffers(){
        this.material.SetBuffer("instanceBuffer", this.instanceBuffer);
    }

    public void UpdateInstanceData(int instanceIndex)
    {
        var renderer = this.listRenderer[instanceIndex];
        var data = new StaticInstanceData()
        {
            AABBCenter = renderer.GetComponent<BoxCollider>().bounds.center,
            AABBExtents = renderer.GetComponent<BoxCollider>().bounds.extents,
            trs = Matrix4x4.TRS(renderer.transform.position, renderer.transform.rotation, renderer.transform.lossyScale)
        };

        this.instancesData[instanceIndex] = data;
        this.instanceBuffer.SetData(this.instancesData);

    }

    public void RemoveInstance(int instanceIndex)
    {
        this.instancesData.RemoveAt(instanceIndex);
        this.listRenderer.RemoveAt(instanceIndex);
        instanceBuffer?.Dispose();
        this.instanceBuffer = new ComputeBuffer(listRenderer.Count, StaticInstanceData.Size);
        this.instanceBuffer.SetData(this.instancesData);
    }

    public int count => this.instancesData.Count;

    public void Draw(CommandBuffer cmdBuffer)
    {
        cmdBuffer.DrawMeshInstancedIndirect(this.mesh, 0, this.material, -1, this.argsBuffer, 0);
    }
    public void Draw()
    {
        Graphics.DrawMeshInstancedIndirect(this.mesh, 0, this.material, new Bounds(Vector3.zero, Vector3.one * 10000), this.argsBuffer, 0);
    }
}

public struct StaticInstanceData
{
    public Vector3 AABBCenter;
    public Vector3 AABBExtents;
    public Matrix4x4 trs;
    public static int Size => sizeof(float) * 22;
}
