using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helper;
using System.Linq;
using System;

public class BitBlockBuilder : MonoBehaviour
{
    [SerializeField] [Range(3, 20)] int blockWidth = 3;
    [SerializeField] [Range(3, 20)] int blockHeight = 5;
    [SerializeField] [Range(3, 20)] int blockDepth = 3;
    [SerializeField] GameObject blockSets = null;
    /// TODO : replace the way to resize the block cell
    [SerializeField] GameObject blockCell = null;
    
    public delegate void OnSizeChangeDelegate(int width, int height, int depth);    
    public delegate void OnCellChangeDelegate(int id, int bitValue);
    public delegate void OnDataChangeDelegate(int width, int height, int depth, in BitCube points);
    public delegate void OnRefreshDelegate();

    public event OnSizeChangeDelegate onSizeChange;
    public event OnCellChangeDelegate onCellChange;
    public event OnDataChangeDelegate onDataChange;
    public event OnRefreshDelegate onRefresh;

    #region property

    internal int width, height, depth;
  
    int indexFromCoord(int x, int y, int z) => z + depth * (x + width * y);
    int childIndexFromCoord(int x, int y, int z) => z + (depth + 1) * (x + (width + 1) * y);

    #endregion

    Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

    BitCube points;
    string objectName;       

    Mesh GetCornerMesh(int bitmask, int level)
    {
        Mesh result = null;
        if (!meshes.TryGetValue(bitmask.ToString(), out result) && bitmask > 0 && bitmask != 255)
        {
            Debug.LogError($"Failed to get corner mesh {bitmask}, level={level}");
        }
        return result;
    }

    void Start()
    {
        // cache root transform name
        objectName = this.name;

        // cache basic mesh sets
        foreach (Transform child in blockSets.transform)
        {
            meshes.Add(child.name, child.GetComponent<MeshFilter>().sharedMesh);
        }

        onSizeChange += OnBlockSizeChange;
        // onCellChange += OnBlockCellChange;
        onRefresh += OnBlockCellRefresh;

        ReCreate();
    }

    void OnDestroy()
    {
        onSizeChange -= OnBlockSizeChange;
        // onCellChange -= OnBlockCellChange;
        onRefresh -= OnBlockCellRefresh;
    }

    public void ChangeBlockValue(Vector3Int id, int newValue)
    {
        if (newValue != points[id])
        {
            points[id] = newValue;

            onCellChange?.Invoke(indexFromCoord(id.x, id.y, id.z), newValue);

            onRefresh?.Invoke();
        }
    }

    void OnBlockSizeChange(int width, int height, int depth)
    {
        // rebuild data model
        points = new BitCube(width, height, depth);

        // rebuild block cells
        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }
        this.transform.DetachChildren();

        foreach (var id in EnumerableHelper.ForEachIntEnumerable(width + 1, height + 1, depth + 1))
        {
            var newObj = Instantiate(blockCell);
            // TODO : replace with relative transform ,eg. basement offset
            newObj.transform.position = new Vector3(id.x, id.y - 1/* yPos*/, id.z);
            newObj.transform.parent = this.transform;
            newObj.gameObject.SetActive(false);
        }
    }

    void OnBlockCellRefresh()
    {
        foreach (var id in EnumerableHelper.ForEachIntEnumerable(width + 1, height + 1, depth + 1))
        {
            int cubeIndex = points.CalculateIsoSurface(id);

            var childObj = this.transform.GetChild(childIndexFromCoord(id.x, id.y, id.z));
            childObj.GetComponent<MeshFilter>().mesh = GetCornerMesh(cubeIndex, id.y);
            childObj.name = string.Format($"[{id.x}, {id.y}, {id.z}]={id.y}({cubeIndex})");
            childObj.gameObject.SetActive(cubeIndex > 0);
        }

        int visibleCount = GetComponentsInChildren<Transform>()
            .Where(t => t != this.transform) // exclude self
            .ToList()
            .Count(child => child.gameObject.activeInHierarchy);

        this.gameObject.name = string.Format($"{objectName}({visibleCount}/{this.transform.childCount})");
    }

    void ReCreate()
    {
        width = blockWidth;
        height = blockHeight;
        depth = blockDepth;
        
        if (points.Length != width * height * depth)
        {
            onSizeChange?.Invoke(width, height, depth);
        }

        // TODO : replcae randomized data sets with generate event
        foreach (var id in EnumerableHelper.ForEachIntEnumerable(width, height, depth))
        {
            points[id] = (id.y != 0 && UnityEngine.Random.Range(0, 1000) < 500) ? 0 : 1;
        }

        onDataChange?.Invoke(width, height, depth, in points);

        onRefresh?.Invoke();        
    }
}