using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshBuilderCommandLine
{
    const string OutputPath = "Assets/Outputs/";

    /// <summary>
    /// To combine all children of the select transform to a mesh.
    /// </summary>
    [MenuItem("Tools/Create CombineMesh")]
    static void CreateCombineMesh()
    {
        var selection = (Selection.activeObject as GameObject).transform;
        if (selection && selection.childCount > 0)
        {
            var combineMesh = CombineMesh(selection);

            WriteMeshAsAsset(combineMesh, selection.name);
        }
    }
    
    /// <summary>
    /// To create a unit right triangle.
    ///  v4 __  
    ///    |\ \__
    ///    | v3__\_ v2
    ///  v7\ |  v6 |
    ///     \|___\_| 
    ///      v0     v1    
    /// </summary>
    [MenuItem("Tools/Create TriangleMesh")]
    static void CreateTriangleMesh()
    {       
        Vector3[] vertices;
        Vector2[] uv;
        int[] indices;

        
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
        
        WriteMeshAsAsset(mesh, "right-triangle");        
    }

    /// <summary>
    /// To create a non combined vertex index cube, and
    /// each face uses non-overlapping texcoords.
    /// 
    /// uv0 : uvmap with non-overlapping texcoords.
    /// uv1 : integer as face ID [1, 6]
    /// 
    ///  v4 ______ v5     
    ///    |\    |\       
    ///    |v3___|_\ v2   
    ///  v7\ |  v6 |      
    ///     \|____\|      
    ///     v0       v1       
    /// </summary>
    [MenuItem("Tools/Create Non-Combine Cube")]
    static void CreateNonCombineCube()
    {
        Vector3[] vertices;
        Vector2[] uv0, uv1;
        int[] indices;
        
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

        uv0 = new Vector2[] {
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

        var uv1_ = new Vector2[] {
            new Vector2(1,1),
            new Vector2(2,2),
            new Vector2(3,3),
            new Vector2(4,4),
            new Vector2(5,5),
            new Vector2(6,6)
        };

        uv1 = new Vector2[] {
            uv1_[0],  uv1_[0],  uv1_[0],  uv1_[0], uv1_[0],  uv1_[0],  //face front
	        uv1_[1],  uv1_[1],  uv1_[1],  uv1_[1], uv1_[1],  uv1_[1],  //face top
            uv1_[2],  uv1_[2],  uv1_[2],  uv1_[2], uv1_[2],  uv1_[2],  //face right
            uv1_[3],  uv1_[3],  uv1_[3],  uv1_[3], uv1_[3],  uv1_[3],  //face left
            uv1_[4],  uv1_[4],  uv1_[4],  uv1_[4], uv1_[4],  uv1_[4],  //face back
            uv1_[5],  uv1_[5],  uv1_[5],  uv1_[5], uv1_[5],  uv1_[5]   //face bottom          
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
        mesh.uv = uv0;
        mesh.uv2 = uv1;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.Optimize();

        WriteMeshAsAsset(mesh, "cube-non-combine");        
    }
    
    /// Internal method
        
    /// TODO : build combine rules, eg. meshes with same name such like "wall", "ground", "roof"
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

    public static void WriteMeshAsAsset(Mesh mesh, string name)
    {
        if (!AssetDatabase.IsValidFolder(OutputPath)) Directory.CreateDirectory(OutputPath);

        AssetDatabase.DeleteAsset($"{name}");
        AssetDatabase.Refresh();

        string outputPath = $"{OutputPath}{name}.asset";
        AssetDatabase.CreateAsset(mesh, outputPath);
    }        
}
