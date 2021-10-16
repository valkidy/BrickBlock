using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshBuilderCommandLineTool
{
    const string OutputPath = "Assets/Outputs/";

    // [MenuItem("MeshBuilder/Combine Mesh")]
    static void CombineMesh()
    {
        var selection = Selection.activeObject;
        if (selection is GameObject)
        {
            var combineMesh = CombineMesh((selection as GameObject).transform);

            if (!AssetDatabase.IsValidFolder(OutputPath)) Directory.CreateDirectory(OutputPath);

            AssetDatabase.DeleteAsset($"{selection.name}");
            AssetDatabase.Refresh();

            string outputPath = $"{OutputPath}{selection.name}.asset";
            AssetDatabase.CreateAsset(combineMesh, outputPath);
        }
    }

    // [MenuItem("MeshBuilder/Create Mesh")]
    static void CreateMesh()
    {       
        Vector3[] vertices;
        Vector2[] uv;
        int[] indices;

        /*  v4 ______ v5       v4 __  
         *    |\    |\           |\ \__  
         *    |v3___|_\ v2       | v3__\_ v2
         *  v7\ |  v6 |        v7\ |  v6 |
         *     \|____\|           \|___\_| 
         *     v0       v1        v0      v1
         */
        var v = new Vector3[] {
                   new Vector3 (-0.5f, -0.5f, -0.5f),
                   new Vector3 (+0.5f, -0.5f, -0.5f),
                   new Vector3 (+0.5f, +0.5f, -0.5f),
                   new Vector3 (-0.5f, +0.5f, -0.5f),
                   new Vector3 (-0.5f, +0.5f, +0.5f),
                   new Vector3 (+0.5f, +0.5f, +0.5f),
                   new Vector3 (+0.5f, -0.5f, +0.5f),
                   new Vector3 (-0.5f, -0.5f, +0.5f),
                };

        vertices = new Vector3[] {
                    v[0], v[2], v[1], //face front
	                v[0], v[3], v[2],
                    v[0], v[7], v[4], //face left
	                v[0], v[4], v[3],
                    v[2], v[3], v[4], //face top
                    v[7], v[1], v[2], //face back
	                v[7], v[2], v[4],
                    v[0], v[1], v[7]  //face bottom	 
                };

        var uv0 = new Vector2[] {
                   new Vector2(0, 0), new Vector2(1, 0),
                   new Vector2(0, 1), new Vector2(1, 1)                        
                };

        uv = new Vector2[] {
                   uv0[3], uv0[0], uv0[2],
                   uv0[3], uv0[1], uv0[0],
                   uv0[1], uv0[0], uv0[2],
                   uv0[1], uv0[2], uv0[3],
                   uv0[2], uv0[3], uv0[1],
                   uv0[1], uv0[0], uv0[2],
                   uv0[1], uv0[2], uv0[3],
                   uv0[1], uv0[0], uv0[3]
                };

        indices = new int[] {
            0, 1, 2,   //face front
	        3, 4, 5,
            6, 7, 8,   //face left
	        9, 10, 11,
           12, 13, 14, //face top
           15, 16, 17, //face back
	       18, 19, 20,
           21, 22, 23  //face bottom	        
        };

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;        
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);        
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.Optimize();

        if (!AssetDatabase.IsValidFolder(OutputPath)) Directory.CreateDirectory(OutputPath);

        string name = "triangle";
        AssetDatabase.DeleteAsset($"{name}");
        AssetDatabase.Refresh();

        string outputPath = $"{OutputPath}{name}.asset";
        AssetDatabase.CreateAsset(mesh, outputPath);
    }

    // [MenuItem("MeshBuilder/Create Side Cube")]
    static void CreateSideCube()
    {
        Vector3[] vertices;
        Vector2[] uv, uv2;
        int[] indices;

        /*  v4 ______ v5     
         *    |\    |\       
         *    |v3___|_\ v2   
         *  v7\ |  v6 |      
         *     \|____\|      
         *     v0       v1   
         */
        var v = new Vector3[] {
            new Vector3 (-0.5f, -0.5f, -0.5f),
            new Vector3 (+0.5f, -0.5f, -0.5f),
            new Vector3 (+0.5f, +0.5f, -0.5f),
            new Vector3 (-0.5f, +0.5f, -0.5f),
            new Vector3 (-0.5f, +0.5f, +0.5f),
            new Vector3 (+0.5f, +0.5f, +0.5f),
            new Vector3 (+0.5f, -0.5f, +0.5f),
            new Vector3 (-0.5f, -0.5f, +0.5f),
        };      
        
        vertices = new Vector3[] {
            v[0], v[2], v[1], //face front
	        v[0], v[3], v[2],
            v[2], v[3], v[4], //face top
            v[2], v[4], v[5],
            v[1], v[2], v[5], //face right
            v[1], v[5], v[6],
            v[0], v[7], v[4], //face left
	        v[0], v[4], v[3], 
            v[5], v[4], v[7], //face back
	        v[5], v[7], v[6],
            v[0], v[6], v[7], //face bottom	 
            v[0], v[1], v[6]
        };
              
        var uv_ = new Vector2[] {
            new Vector2(0, 0),         new Vector2(0.333f, 0),    new Vector2(0.333f, 0.5f), new Vector2(0, 0.5f),       // Front
            new Vector2(0.666f, 0),    new Vector2(0.333f, 0),    new Vector2(0.333f, 0.5f), new Vector2(0.666f, 0.5f),  // Top
            new Vector2(0.666f, 0),    new Vector2(0.666f, 0.5f), new Vector2(1, 0.5f),      new Vector2(1, 0),          // Right
            new Vector2(0.333f, 0.5f), new Vector2(0.333f, 1),    new Vector2(0, 1),         new Vector2(0, 0.5f),       // Left
            new Vector2(0.666f, 1),    new Vector2(0.333f, 1),    new Vector2(0.333f, 0.5f), new Vector2(0.666f, 0.5f),  // Back
            new Vector2(1, 0.5f),      new Vector2(0.666f, 0.5f), new Vector2(0.666f, 1),    new Vector2(1, 1)           // Bottom
        };

        uv = new Vector2[] {
            uv_[0],  uv_[2],  uv_[1],  //face front
	        uv_[0],  uv_[3],  uv_[2],
            uv_[4],  uv_[5],  uv_[6],  //face top
            uv_[4],  uv_[6],  uv_[7],
            uv_[8],  uv_[9],  uv_[10], //face right
            uv_[8],  uv_[10], uv_[11],
            uv_[12], uv_[15], uv_[14], //face left
	        uv_[12], uv_[14], uv_[13],
            uv_[17], uv_[16], uv_[19], //face back
	        uv_[17], uv_[19], uv_[18],
            uv_[20], uv_[22], uv_[23], //face bottom	 
            uv_[20], uv_[21], uv_[22]
        };
        var uv2_ = new Vector2[] {
            new Vector2(1,1),
            new Vector2(2,2),
            new Vector2(3,3),
            new Vector2(4,4),
            new Vector2(5,5),
            new Vector2(6,6)
        };

        uv2 = new Vector2[] {
            uv2_[0],  uv2_[0],  uv2_[0],  uv2_[0], uv2_[0],  uv2_[0],  //face front
	        uv2_[1],  uv2_[1],  uv2_[1],  uv2_[1], uv2_[1],  uv2_[1],  //face top
            uv2_[2],  uv2_[2],  uv2_[2],  uv2_[2], uv2_[2],  uv2_[2],  //face right
            uv2_[3],  uv2_[3],  uv2_[3],  uv2_[3], uv2_[3],  uv2_[3],  //face left
            uv2_[4],  uv2_[4],  uv2_[4],  uv2_[4], uv2_[4],  uv2_[4],  //face back
            uv2_[5],  uv2_[5],  uv2_[5],  uv2_[5], uv2_[5],  uv2_[5]   //face bottom          
        };

        indices = new int[] {
            0, 1, 2,   //face front
	        3, 4, 5,
            6, 7, 8,   //face top
	        9, 10, 11,
           12, 13, 14, //face right
           15, 16, 17, 
	       18, 19, 20, //face left
           21, 22, 23,
           24, 25, 26, //face back	        
           27, 28, 29,         
           30, 31, 32, //face bottom	        
           33, 34, 35  	        
        };

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.uv2 = uv2;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.Optimize();

        if (!AssetDatabase.IsValidFolder(OutputPath)) Directory.CreateDirectory(OutputPath);

        string name = "sidecube";
        AssetDatabase.DeleteAsset($"{name}");
        AssetDatabase.Refresh();

        string outputPath = $"{OutputPath}{name}.asset";
        AssetDatabase.CreateAsset(mesh, outputPath);
    }

    // TODO : combine meshes with same name, eg. "wall", "ground", "roof"     
    public static Mesh CombineMesh(Transform t)
    {
        // Combine meshes
        var pivot = t.GetChild(0);
        MeshFilter[] meshFilters = pivot.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combineMeshFilters = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; ++i)
        {
            combineMeshFilters[i].mesh = meshFilters[i].sharedMesh;
            combineMeshFilters[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        var combineMesh = new Mesh();
        combineMesh.CombineMeshes(combineMeshFilters, true, true);
        return combineMesh;
    }

}
