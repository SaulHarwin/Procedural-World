using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : ScriptableObject {    
    public int seed;
    public int octaves;

    public float amplitude = 480f;
    public float frequency = 0.001f;
    public float lacinarity = 2f;
    public float persistance = 0.42f;
    
    public float landMassFrequency = 0.0001f;
    public float landMassAmplitude = 500f;
}
