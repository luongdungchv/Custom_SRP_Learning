using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode, InitializeOnLoad]
public class GPUStaticRenderer : MonoBehaviour
{
    private static bool IsInitialized;

    [SerializeField] protected Material material;
    [SerializeField] protected Mesh mesh;
    protected int indexInGroup = -1;
    private GPUStaticRenderGroup group;

#if UNITY_EDITOR
    static GPUStaticRenderer()
    {
        // var rendererList = GameObject.FindObjectsOfType<GPUStaticRenderer>();
        // foreach(var renderer in rendererList) renderer.Initialize();

    }
#endif

    private void Awake()
    { 
        this.Initialize();
    }
#if UNITY_EDITOR
    private Vector3 lastPosition;
    private Material lastMaterial;
    private void Update()
    {
        if(lastPosition != transform.position){
            lastPosition = transform.position;
            Debug.Log(this.indexInGroup);
            this.group.UpdateInstanceData(this.indexInGroup);
        }
    }

    public void Select(){

    }

    private void OnValidate() {
        if(lastMaterial != null && lastMaterial != this.material){
            this.group.RemoveInstance(this.indexInGroup);
            GPURenderManager.Instance.RegisterRenderer(this, this.mesh, this.material);
        }
        lastMaterial = this.material;
    }
#endif

    public void Initialize()
    {
        Debug.Log("init");
        GPURenderManager.Instance.RegisterRenderer(this, this.mesh, this.material);
    }

    public void SetIndex(int index, GPUStaticRenderGroup group)
    {
        Debug.Log("Set Index");
        this.indexInGroup = index;
        this.group = group;
    }

    private void OnDestroy()
    {

    }
    [Sirenix.OdinInspector.Button]
    private void Test(){
        this.group.Log();
        var data = group.GetData();
        foreach(var i in data) Debug.Log(i.AABBCenter);
    }

}
