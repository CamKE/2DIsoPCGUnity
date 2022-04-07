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
    
    float myFloat = 1.23f;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // for square and rectange levels, just round terrain size to nearest square or 2:1 ratio
        myFloat = EditorGUILayout.Slider("Terrain Size", myFloat, 10, 2000);

        LevelManager levelManager = (LevelManager)target;

        if (GUILayout.Button("Generate Level"))
        {
            levelManager.generate();
        }
    }
}
