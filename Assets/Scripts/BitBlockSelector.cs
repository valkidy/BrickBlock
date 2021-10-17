using Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BitBlockBuilder))]
public class BitBlockSelector : MonoBehaviour
{
    public GameObject selectionPrefab;

    LayerMask layerMask = 1 << 0;
    
    BoxCursor cursor;
    BitBlockBuilder builder;
    Transform placeHolder;

    Vector3Int size; 
    Bounds bounds = new Bounds();

    Ray ray;

    // Start is called before the first frame update
    void Start()
    {        
        cursor = new BoxCursor(selectionPrefab);
        placeHolder = new GameObject("PlaceHolder").transform;

        builder = GetComponent<BitBlockBuilder>();
        if (builder)
        {
            builder.onSizeChange += OnSizeChange;
            builder.onDataChange += OnUpdateAllCollider;
            builder.onCellChange += OnUpdateCollider;
        }
    }

    void OnDestroy()
    {
        if (builder)
        {
            builder.onSizeChange -= OnSizeChange;
            builder.onDataChange -= OnUpdateAllCollider;
            builder.onCellChange -= OnUpdateCollider;
        }

        Destroy(GameObject.Find("PlaceHolder"));
    }

    void OnSizeChange(int width, int height, int depth)
    {
        size = new Vector3Int(width, height, depth);

        foreach (Transform child in placeHolder)
        {
            Destroy(child.gameObject);
        }
        placeHolder.DetachChildren();

        foreach (var id in EnumerableHelper.ForEachIntEnumerable(width, height, depth))
        {
            var newObj = new GameObject($"[{id.x}, {id.y}, {id.z}]", new System.Type[] { typeof(BoxCollider) });
            var scale = newObj.transform.localScale;
            newObj.transform.position = new Vector3(id.x, id.y - 1, id.z) + scale / 2;
            newObj.transform.parent = placeHolder;
        }
        bounds.SetMinMax(Vector3.zero, new Vector3(width - 1, height - 1, depth - 1));
    }

    void OnUpdateCollider(Vector3Int id, int oldValue, int newValue)
    {
        placeHolder.GetChild(id.z + size.z * (id.x + size.x * id.y)).gameObject.SetActive(newValue > 0);
    }

    void OnUpdateAllCollider(in BitCube points)
    {        
        foreach (var id in EnumerableHelper.ForEachIntEnumerable(size.x, size.y, size.z))
        {            
            OnUpdateCollider(id, points[id], points[id]);
        }
    }

    void Update()
    {
        cursor.enable = false;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // TODO : add collision layer filter        
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, layerMask))
        {
            var baseID = (hit.transform.position + new Vector3(0, 1, 0)).ToVector3Int();

            cursor.SetPositionAndHighLight(hit.transform.position, hit.normal);
            cursor.enable = true;

            var hasUserInput = (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1));
            if (hasUserInput)
            {
                var adjacentID = hit.normal.ToVector3Int();

                (Vector3Int, int) chunkInfo = (-Vector3Int.one, -1);
                if (Input.GetMouseButtonDown(0)) { chunkInfo = (baseID + adjacentID, 1); }
                if (Input.GetMouseButtonDown(1)) { chunkInfo = (baseID, 0); }

                (var id, var bitValue) = chunkInfo;
                if (bounds.Contains(id))
                {
                    builder?.ChangeBlockValue(id, bitValue);
                }

                /// avoid UnityTemplateProjects.SimpleCameraController to lock mouse cursor.
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;                
            }
        }
    }
}
