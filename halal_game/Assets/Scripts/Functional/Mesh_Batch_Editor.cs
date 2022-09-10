using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Mesh_Batcher))]
public class Mesh_Batch_Editor : Editor
{
    private void OnSceneGUI()
    {
        Mesh_Batcher batcher = target as Mesh_Batcher;

        if (Handles.Button(batcher.transform.position + Vector3.up * 2, Quaternion.LookRotation(Vector3.up), 1, 1, Handles.CubeHandleCap))
        {
            batcher.BatchMeshes();
        }
    }
}
