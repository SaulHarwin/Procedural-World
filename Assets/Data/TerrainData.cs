using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : ScriptableObject {
    public bool waterVisible;
    public LODInfo[] resolutionLevels;
    public Gradient gradient;
    public int scale = 1;
    public int chunkSize = 240;
    public int maxViewDst = 5;
    public AnimationCurve meshHeightCurve;
    public AnimationCurve landMassHeightCurve;

    [System.Serializable]
    public struct LODInfo {
        public int resolution;
        public float visibleChunksDstThreshold;
    }
}
