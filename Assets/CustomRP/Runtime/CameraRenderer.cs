using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
{
    private ScriptableRenderContext context;
    private Camera camera;

    private const string bufferName = "Camera Render";

    private CommandBuffer buffer = new CommandBuffer()
    {
        name = bufferName
    };

    private CullingResults cullingResults;

    //private ShaderTagId unlitTagId = new ShaderTagId("SRPDefaultUnli");
    private ShaderTagId[] shaderTagIds = {
        new ShaderTagId("UniversalForward"),
        new ShaderTagId("SRPDefaultUnlit"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("ForwardAdd"),
    };

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.camera = camera;
        this.context = context;

        if (!this.Cull()) return;

        this.Setup();
        this.DrawVisibleGeometry();

        this.Submit();
    }

    private void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(this.shaderTagIds[0], sortingSettings);
        for (int i = 1; i < this.shaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, this.shaderTagIds[i]);
        }
        var filterSettings = new FilteringSettings(RenderQueueRange.all);

        //context.DrawRenderers(this.cullingResults, ref drawingSettings, ref filterSettings);
        this.DrawStaticGeometryIndirect();
        this.context.DrawSkybox(this.camera);
    }

    private void DrawStaticGeometryIndirect()
    {
        if (GPURenderManager.IsInitialized)
        {
            GPURenderManager.Instance.InitializeAllGroupBuffers();
            GPURenderManager.Instance.DrawAllGroups(this.buffer);
        }
        else
        {
            GPURenderManager.Instance.GatherAllRenderers();
            GPURenderManager.Instance.InitializeAllGroupBuffers();
        }
    }

    private void Submit()
    {
        buffer.EndSample(bufferName);
        this.ExecuteBuffer();
        this.context.Submit();
    }
    private void Setup()
    {
        this.context.SetupCameraProperties(this.camera);
        buffer.ClearRenderTarget(true, true, Color.clear);
        buffer.BeginSample(bufferName);
        this.ExecuteBuffer();
    }

    private void ExecuteBuffer()
    {
        this.context.ExecuteCommandBuffer(this.buffer);
        buffer.Clear();
    }

    private bool Cull()
    {
        if (camera.TryGetCullingParameters(out var cullParams))
        {
            this.cullingResults = this.context.Cull(ref cullParams);
            return true;
        }
        return false;
    }
}
