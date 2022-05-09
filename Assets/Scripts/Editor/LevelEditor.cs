using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

/// <summary>
/// Responsible for customising the inspector window to allow for easy level generation via the inspector.
/// </summary>
[CustomEditor(typeof(LevelManager))]
public class LevelEditor : Editor
{
    //EditorUtility.DisplayDialog("This is a test", "Test message body", "Close");
    GUIStyle headingStyle;
    int terrainSize;
    int terrainType;
    bool exactHeight;
    bool heightRange;
    int exactHeightValue;
    int minHeight;
    int maxHeight;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        LevelManager levelManager = (LevelManager)target;

        headingStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

        addHeading("Level Generation Options", true);

        addHeading("Terrain Options");

        terrainSize = EditorGUILayout.IntSlider("Terrain Size", terrainSize, TerrainGenerator.terrainMinSize, TerrainGenerator.terrainMaxSize);
        
        string[] terraintypes = Enum.GetNames(typeof(TerrainGenerator.TerrainType));
        terrainType = EditorGUILayout.Popup("Terrain type", terrainType, terraintypes);

        EditorGUILayout.BeginHorizontal();
        exactHeight = GUILayout.Toggle(!heightRange, "Exact Height");
        heightRange = GUILayout.Toggle(!exactHeight, "Height Range");
        EditorGUILayout.EndHorizontal();

        if (exactHeight)
        {
            exactHeightValue = EditorGUILayout.IntSlider("Exact Height", exactHeightValue, TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);
        }
        else
        {
            minHeight = EditorGUILayout.IntSlider("Minimum Height", minHeight, TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);
            maxHeight = EditorGUILayout.IntSlider("Maxiumum Height", maxHeight, TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Randomise Level"))
        {

        }

        if (GUILayout.Button("Generate Level"))
        {
            //levelManager.generate();
        }
        EditorGUILayout.EndHorizontal();

    }

    private void addHeading(string heading, bool mainHeading = false)
    {
        EditorGUILayout.Space();
        headingStyle.fontSize = mainHeading ? 15 : 13;
        EditorGUILayout.LabelField(heading, headingStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    }


}
