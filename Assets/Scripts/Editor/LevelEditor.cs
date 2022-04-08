using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Responsible for customising the inspector window to allow for easy level generation via the inspector.
/// </summary>
[CustomEditor(typeof(LevelManager))]
public class LevelEditor : Editor
{
    //EditorUtility.DisplayDialog("This is a test", "Test message body", "Close");
    float levelSize = 1.23f;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelManager levelManager = (LevelManager)target;
        // for square and rectange levels, just round terrain size to nearest square or 2:1 ratio
        levelSize = EditorGUILayout.Slider("Terrain Size", levelSize, TerrainGenerator.terrainMinSize, TerrainGenerator.terrainMaxSize);
       
        if (GUILayout.Button("Generate Level"))
        {
            levelManager.generate();
        }
    }
}
