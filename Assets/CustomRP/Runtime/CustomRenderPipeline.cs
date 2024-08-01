using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer cameraRenderer = new CameraRenderer();
    public CustomRenderPipeline(){
        Debug.Log("pipeline created");
    }
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach(var camera in cameras){
            cameraRenderer.Render(context, camera);
        }
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
    }
}
