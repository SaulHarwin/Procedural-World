using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : ScriptableObject {
    public bool waterVisible;
    public Gradient gradient;
    public int scale = 1;
    public int chunkSize = 240;
    public int maxViewDst = 5;
    public AnimationCurve meshHeightCurve;
    public AnimationCurve landMassHeightCurve;


    [SerializeField] public GraphicQuality graphicQuality;
    public enum GraphicQuality {Low, Medium, High}

    public ResolutionData lowResolution;
    public ResolutionData mediumResolution;
    public ResolutionData highResolution;
}
