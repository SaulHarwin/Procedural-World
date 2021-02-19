using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : ScriptableObject {    
    public int seed;
    public int seedLandMass;
    public int seedHeat;
    public int seedMoisture;
    public int octaves;
    public int mapScale;

    public float amplitude = 480f;
    public float frequency = 0.001f;
    public float lacinarity = 2f;
    public float persistance = 0.42f;
    
    public float landMassFrequency = 0.0001f;
    public float landMassAmplitude = 500f;

    public float heatMapFrequency = 0.001f;
    public float moistureMapFrequency = 0.001f;
}
