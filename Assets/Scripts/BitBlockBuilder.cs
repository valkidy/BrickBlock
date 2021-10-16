using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helper;
using System.Linq;

public class BitBlockBuilder : MonoBehaviour
{
    [SerializeField] [Range(3, 20)] int width = 3;
    [SerializeField] [Range(3, 20)] int height = 5;
    [SerializeField] [Range(3, 20)] int depth = 3;

    [SerializeField] GameObject blockSets = null;
    [SerializeField] GameObject blockCell = null;

    #region property

    public (int, int, int) Size => (width, height, depth);    
    public int this[Vector3Int id]
    {
        set => points[id] = value;
        get => points[id];
    }
    public void OnReCreate() => ReCreate();
    public int indexFromCoord(int x, int y, int z) => z + depth * (x + width * y);
    public int childIndexFromCoord(int x, int y, int z) => z + (depth + 1) * (x + (width + 1) * y);

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
        objectName = this.name;

        points = new BitCube(width, height, depth);

        foreach (var id in EnumerableHelper.ForEachIntEnumerable(width, height, depth))
        {
            points[id] = (id.y != 0 && UnityEngine.Random.Range(0, 1000) < 500) ? 0 : 1;
        }

        foreach (Transform child in blockSets.transform)
        {
            meshes.Add(child.name, child.GetComponent<MeshFilter>().sharedMesh);
        }

        foreach (var id in EnumerableHelper.ForEachIntEnumerable(width + 1, height + 1, depth + 1))
        {
            var newObj = Instantiate(blockCell);
            newObj.transform.position = new Vector3(id.x, id.y - 1/* yPos*/, id.z);
            newObj.transform.parent = this.transform;
            newObj.gameObject.SetActive(false);
        }

        ReCreate();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.V))
        {
            foreach (var id in EnumerableHelper.ForEachIntEnumerable(width, height, depth))
            {
                points[id] = (id.y != 0 && UnityEngine.Random.Range(0, 1000) < 500) ? 0 : 1;
            }

            ReCreate();
        }
    }

    void ReCreate()
    {
        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(false);            
        }
        
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

        this.gameObject.name = string.Format($"{objectName}({this.transform.childCount})({visibleCount})");
    }
}