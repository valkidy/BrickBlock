using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxCursor
{
    Dictionary<Vector3, int> faceIDMap = new Dictionary<Vector3, int>()
    {
        { -Vector3.forward, 1}, // front
        {  Vector3.up,      2}, // top
        {  Vector3.right,   3}, // right
        { -Vector3.right,   4}, // left
        {  Vector3.forward, 5}, // back
        { -Vector3.up,      6}, // bottom
    };

    readonly int propertyFaceID = Shader.PropertyToID("_FaceID");

    GameObject obj;
    Material material;

    public BoxCursor(GameObject boxCursorPrefab)
    {
        obj = GameObject.Instantiate(boxCursorPrefab) as GameObject;
        material = obj.GetComponent<Renderer>().sharedMaterial;
    }

    public bool enable
    {
        set => obj.SetActive(value);
        get => obj.activeSelf;
    }

    public void SetPositionAndHighLight(Vector3 position, Vector3 faceNormal)
    {        
        material.SetInt(propertyFaceID, faceIDMap[faceNormal]);
        obj.transform.position = position;        
    }
}
