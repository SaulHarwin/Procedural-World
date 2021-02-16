using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : ScriptableObject {
    public LODInfo[] resolutionLevels;
    public Gradient gradient;
    public int scale = 1;
    public int chunkSize = 240;
    public float maxViewDst = 500f;
    public AnimationCurve meshHeightCurve;
    public AnimationCurve landMassHeightCurve;

    [System.Serializable]
    public struct LODInfo {
        public int resolution;
        public float visibleChunksDstThreshold;
    }
}
