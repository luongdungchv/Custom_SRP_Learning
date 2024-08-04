using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
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

    private SceneView sceneView;

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.camera = camera;
        this.context = context;

        if (!this.Cull()) return;

        this.Setup();
        buffer.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
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

        this.DrawStaticGeometryIndirect();
        context.DrawRenderers(this.cullingResults, ref drawingSettings, ref filterSettings);
        this.context.DrawSkybox(this.camera);
    }

    private void DrawStaticGeometryIndirect()
    {
        if(this.sceneView == null){
            sceneView = SceneView.lastActiveSceneView;
            SceneView.duringSceneGui  += this.SceneViewGUI;
        }
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

    private void SceneViewGUI(SceneView scene)
    {
        
        if(Event.current.type == EventType.MouseDown){
            var raycastResult = new List<RaycastResult>();
            var pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Event.current.mousePosition;
            Debug.Log(pointerEventData.position);
            Debug.Log(EventSystem.current);
            //EventSystem.current.RaycastAll(pointerEventData, raycastResult);
            //Debug.Log(raycastResult[0]);
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
