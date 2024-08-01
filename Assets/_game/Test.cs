using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GPURenderManager.Instance.InitializeAllGroupBuffers();
        GPURenderManager.Instance.Log();
    }

    // Update is called once per frame
    void Update()
    {
        //GPURenderManager.Instance.DrawAllGroups(); 
    }
}
