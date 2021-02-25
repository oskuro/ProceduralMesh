using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorInspector : Editor
{
   public override void OnInspectorGUI() {
        TerrainGenerator tg = (TerrainGenerator) target;
        // base.OnInspectorGUI();


        DrawDefaultInspector();

        // if (DrawDefaultInspector())
        // {
        //     tg.UpdateMesh();
        // }

        if (GUILayout.Button("Generate Mesh")) {
            tg.DestroyOldTerrains();
            tg.CreateTerrain();
        }

        if (GUILayout.Button("Update Mesh")) {
            tg.UpdateMesh();
        }

        
   }
    
}
