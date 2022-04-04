using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
