using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBuilderController : MonoBehaviour
{
    [SerializeField]
    Terrain terrain;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    float[,] GenerateNoise(int wid, int hei, float scale, Vector2 offset) {

        float[,] noiseMap = new float[wid, hei];

        for (int x =0; x < wid; x++) {
            for (int y = 0; y< hei;y++) {
                float xpos = (float)x * scale + offset.x;
                float ypos = (float)y * scale + offset.y;

                float randomhei = Random.Range(0.00f,1.01f);

            }
        }
        return null;
    }
}
