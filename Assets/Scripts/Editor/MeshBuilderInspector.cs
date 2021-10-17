using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Bitset;
using System.IO;

[CustomEditor(typeof(MeshBuilder))]
public class MeshBuilderInspector : Editor
{
    bool showProgress = false;
    string progressMessage;
    float progress = 0F;
    string outputDir = "Outputs";

    SerializedProperty propertyInput;

    Rect position;

    void Header(string title)
        => EditorGUILayout.PrefixLabel(title, new GUIStyle(), new GUIStyle() { fontStyle = FontStyle.Bold });

    void OnEnable()
    {                
        propertyInput = serializedObject.FindProperty("basicSets");
    }

    void OnUpdateProgress(string message, float progress)
    {
        this.progressMessage = message;
        this.progress = progress;

        Repaint();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Header("Property Settings");        
        EditorGUILayout.PropertyField(propertyInput);

        position = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);        
        outputDir = EditorGUI.TextField(position, new GUIContent() {text="Output Dir"}, outputDir);

        EditorGUILayout.Space();

        Header("Build");        
        if (GUILayout.Button("Build"))
        {
            // run background thread in Editor mode
            var task = ConvertMeshsetTask(new CancellationTokenSource().Token, this);
        }

        if (showProgress)
        {
            position = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            EditorGUI.ProgressBar(position, progress, progressMessage);
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
    
    static readonly object Mutex = new System.Object();
    static readonly int taskAwaitTimeInMilliSeconds = 10;
    static async Task<int> ConvertMeshsetTask(CancellationToken token, UnityEngine.Object inspector)
    {      
        token.ThrowIfCancellationRequested();

        await UnityThreadDispatcher.ThreadSwitcher.ResumeUnityAsync();

        var targetObject = (MeshBuilderInspector)inspector;

        string outputPath = $"Assets/{targetObject.outputDir}";

        lock (Mutex)
        {
            if (!AssetDatabase.IsValidFolder(outputPath)) { Directory.CreateDirectory(outputPath); }
        }

        DestroyImmediate(GameObject.Find("TEMPORARY_CHUNK_ROOT"));
        GameObject cs = new GameObject("TEMPORARY_CHUNK_ROOT");        

        lock (Mutex)
        {
            targetObject.showProgress = true;
            targetObject.OnUpdateProgress("Prepare", 0);        
        }

        var inputMesh = targetObject.propertyInput.objectReferenceValue as GameObject;
        if (!inputMesh) {
            return -1;
        }

        int numOfBaseMesh = inputMesh.transform.childCount;       
        foreach (Transform t in inputMesh.transform)
        {                        
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();           

            var bitValue = -1;
            if (!int.TryParse(t.name, out bitValue))
                continue;

            var mesh = t.gameObject;            
            if (!BitsetTable.ContainsKey(bitValue))
                continue;

            for (int i = 0; i < 4; ++i)
            {
                int bvalue = BitsetTable.Table[bitValue][i];
                if (bvalue == 0)
                    continue;

                var newMesh = Instantiate(mesh, Vector3.zero, Quaternion.Euler(0, BitsetTable.Orientation[i], 0));

                GameObject variant = new GameObject($"{bvalue}", new Type[] { typeof(MeshFilter), typeof(MeshRenderer) });
                variant.transform.SetParent(cs.transform);

                variant.GetComponent<MeshFilter>().sharedMesh = MeshBuilderCommandLine.CombineMesh(newMesh.transform);
                // variant.GetComponent<MeshRenderer>().sharedMaterial = materialRef; 
                
                DestroyImmediate(newMesh);
            }

            float percentage = (float)(t.GetSiblingIndex() + 1) / numOfBaseMesh;
            lock (Mutex)
            {
                targetObject.OnUpdateProgress($"Process varientID : {bitValue}({100F * percentage:F2}%)", percentage);                
            }

            await Task.Delay(taskAwaitTimeInMilliSeconds, token);
        }
        
        // TODO : output path setting
        int numOfChunk = cs.transform.childCount;
        foreach (Transform chunk in cs.transform)
        {            
            var ckmf = chunk.GetComponent<MeshFilter>();
            var name = chunk.name;
            string assetPath = $"{outputPath}/{name}.asset";            

            AssetDatabase.CreateAsset(ckmf.sharedMesh, assetPath);

            float percentage = (float)(chunk.GetSiblingIndex() + 1) / numOfChunk;
            lock (Mutex)
            {                
                targetObject.OnUpdateProgress($"Process meshID : {name}({100F * percentage:F2}%)", percentage);
            }

            await Task.Delay(taskAwaitTimeInMilliSeconds, token);
        }

        string prefabPath = $"{outputPath}/house.prefab";
        PrefabUtility.SaveAsPrefabAsset(cs, prefabPath);

        DestroyImmediate(GameObject.Find("TEMPORARY_CHUNK_ROOT"));

        lock (Mutex)
        {
            targetObject.OnUpdateProgress($"DONE", 1F);
            targetObject.showProgress = false;
        }
        
        return 0;
    }
}
