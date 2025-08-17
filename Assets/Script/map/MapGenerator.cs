using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public Tilemap groundTileMap;

    public int width;
    public int height;

    public int seed;
    public bool useRandomSeed;

    public float lacunarity;

    [Range(0,1f)]
    public float waterProbability;

    public TileBase groundTile;
    public TileBase waterTile;

    private float[,] mapData;//True:ground,False:water


   public void GenerateMap()
    {
        GenerateMapData();
        //TODO:地图处理
        GenerateTileMap();
    }
   private void GenerateMapData()
    {
        //对于种子的应用
        if (!useRandomSeed)
        {
            seed = Time.time.GetHashCode();
        }
        UnityEngine.Random.InitState(seed);

        mapData = new float[width,height];

        float randomOffset = UnityEngine.Random.Range(-10000,10000);
        //0~1
        //0~0.5
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;
        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                float noiseValue =  Mathf.PerlinNoise(x* lacunarity + randomOffset, y* lacunarity + randomOffset);
                mapData[x, y] = noiseValue;

                if (noiseValue < minValue) minValue = noiseValue;
                if (noiseValue > maxValue) maxValue = noiseValue;
            }
        }
        //平滑到0~1
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mapData[x, y] = Mathf.InverseLerp(minValue, maxValue, mapData[x,y]);
            }
        }

    }

    private void GenerateTileMap()
    {
        CleanTileMap();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileBase tile = mapData[x,y]>waterProbability ? groundTile : waterTile;
                groundTileMap.SetTile(new Vector3Int(x,y),tile);
            }
        }
    }
    public void CleanTileMap()
    {
        groundTileMap.ClearAllTiles();
    }

}
