using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ResolutionData : ScriptableObject {
    public LODInfo[] resolutionLevels;

    [System.Serializable]
    public struct LODInfo {
        public int resolution;
        public float visibleChunksDstThreshold;
    }
}
