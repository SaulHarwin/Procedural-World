using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TreeData : ScriptableObject {
    public GameObject tree;

    public float radius;
    public float displayRadius;
    public Vector2 regionSize = Vector2.one;
    public int rejectionSamples;
}
