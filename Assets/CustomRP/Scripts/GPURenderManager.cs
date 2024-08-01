using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class GPURenderManager
{
    private static GPURenderManager instance;
    public static GPURenderManager Instance{
        get {
            if (instance == null) instance = new GPURenderManager();
            return instance;
        }
    }
    public static bool IsInitialized => instance != null;

    private Dictionary<(Mesh, Material), GPUStaticRenderGroup> groups;
    public GPURenderManager()
    {
        groups = new Dictionary<(Mesh, Material), GPUStaticRenderGroup>();
    }

    public void GatherAllRenderers(){
        var renderers = GameObject.FindObjectsOfType<GPUStaticRenderer>();
        foreach(var renderer in renderers){
            renderer.Initialize();
        }
    }

    public void RegisterRenderer(GPUStaticRenderer renderer, Mesh mesh, Material material){
        if(groups.ContainsKey((mesh, material))){
            groups[(mesh, material)].RegisterRenderer(renderer);
        }
        else{
            var group = new GPUStaticRenderGroup();
            group.SetMeshAndMaterial(mesh, material); 
            group.RegisterRenderer(renderer);
            groups.Add((mesh, material), group);          
        }
    }

    public void Log(){
        Debug.Log(this.groups.ElementAt(0).Value.count);
    }

    public void InitializeAllGroupBuffers(){
        foreach(var key in this.groups.Keys){
            //groups[key].Log();
#if UNITY_EDITOR
            groups[key].BindBuffers();
#endif
            if(groups[key].IsBufferSynchronized) continue;
            groups[key].InitBuffers();
        }
    }

    public void TrySyncAllGroupBuffers(){
        
    }

    public void DrawAllGroups(CommandBuffer cmdBuffer){
        foreach(var key in groups.Keys){
            groups[key].Draw(cmdBuffer);
        }
    }
    public void DrawAllGroups(){
        foreach(var key in groups.Keys){ 
            groups[key].Draw();
        }
    }
}
