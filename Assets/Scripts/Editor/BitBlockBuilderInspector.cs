using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Bitset;
using System.IO;

[CustomEditor(typeof(BitBlockBuilder))]
public class BitBlockBuilderInspector : Editor
{
    SerializedProperty propertyBlockWidth;
    SerializedProperty propertyBlockHeight;
    SerializedProperty propertyBlockDepth;
    SerializedProperty propertyBlockSets;
    SerializedProperty propertyBlockCell;

    Rect position;

    void Header(string title)
        => EditorGUILayout.PrefixLabel(title, new GUIStyle(), new GUIStyle() { fontStyle = FontStyle.Bold });

    void OnEnable()
    {
        propertyBlockWidth = serializedObject.FindProperty("blockWidth");
        propertyBlockHeight = serializedObject.FindProperty("blockHeight");
        propertyBlockDepth = serializedObject.FindProperty("blockDepth");
        propertyBlockSets = serializedObject.FindProperty("blockSets");
        propertyBlockCell = serializedObject.FindProperty("blockCell");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(propertyBlockWidth, new GUIContent(){ text="Width" });
        EditorGUILayout.PropertyField(propertyBlockHeight, new GUIContent() { text = "Height" });
        EditorGUILayout.PropertyField(propertyBlockDepth, new GUIContent() { text = "Depth" });
        EditorGUILayout.PropertyField(propertyBlockSets);
        EditorGUILayout.PropertyField(propertyBlockCell);

        if (GUILayout.Button("Generate"))
        {
            ((BitBlockBuilder)serializedObject.targetObject).SendMessage("ReCreate");
        }

        serializedObject.ApplyModifiedProperties();
    }
       
}
