using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CountChildren : MonoBehaviour
{
    void OnValidate()
    {
        // this.gameObject.name += $"({this.transform.childCount})";   
    }    
}
