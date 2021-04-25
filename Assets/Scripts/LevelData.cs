using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType : byte
{
    Unchanged = 0,
    Sand = 1,
    Rock = 10,
    Dirt = 20,
    DirtMinusThrd = 21,
    DirtMinus2Thrds = 22,
    Grass = 56, // special layer found on top
    Deleted = 99, //Tile that was deleted
    Invisible = 100, //tile cannot be interacted with
//    Player = 200
}


//Holds data about level
public class LevelData
{
    public int layerWidth { get; private set; }
    public int layerLength { get; private set; }

    public int globalSeed { get; private set; }


    //Needed layers are generated on the fly
    //Then modification data is overlayed on top
    //Once layer is fully cleared then modification data is removed
    //Offset shows first layer that's not cleared
    private List<TileType[]> layerModificationData; //stores tiles that were destroyed or that were moved (for example, sand falls down)
    public int offset { get; private set; }
    public List<TileType[]> visibleLayerData;
    private int[] depthData; //Highest depth of each position;
    public int maxDepth { get; private set; }

    public LevelData(int width, int length)
    {
        offset = 0;
        layerWidth = width;
        layerLength = length;
        visibleLayerData = new List<TileType[]>();
        layerModificationData = new List<TileType[]>();
        depthData = new int[layerLength * layerWidth];
        globalSeed = unchecked((int)System.DateTime.UtcNow.Ticks); //grab part of time in ticks
    }

    public List<TileType[]> GetVisible(int startingLayer)
    {
        var endLayer = maxDepth + 1;
        var layerIndex = startingLayer >= endLayer ? endLayer-1 : startingLayer;
        CalculateLayerBetween(layerIndex, endLayer);
        return visibleLayerData.GetRange(0, endLayer-offset-layerIndex); //0 - always starts from top!
    }

    private void CalculateLayerBetween(int topLayer, int bottomLayer)
    {
        for (int i = 0; i < bottomLayer - topLayer; i++) //take a look here if it doesn't work!
        {
            if(layerModificationData.Count <= i + topLayer)
                layerModificationData.Add(new TileType[layerLength * layerWidth]);

            if (visibleLayerData.Count <= i) //if there are less layers than we need then add new one
            { 
                visibleLayerData.Add(FillLayerData(new TileType[layerLength * layerWidth], null, i + topLayer));
            }
            else //otherwise - override on top
                FillLayerData(visibleLayerData[i], layerModificationData[i + topLayer], i + topLayer);
        }
    }

    private TileType[] FillLayerData(TileType[] layer, TileType[] modificationLayer, int depth)
    {
        var finalSeed = globalSeed + depth;
        Random.InitState(finalSeed);
        for (int i = 0; i < layer.Length; i++)
        {
            var tile = GetTile(i % layerWidth, i / layerLength, depth); //need to call this here as random.range inside fails when seed it reset;
            if (modificationLayer == null || modificationLayer[i] == TileType.Unchanged)
                if (!TileIsVisible(i, depth, layer))
                    layer[i] = TileType.Invisible;
                else
                    layer[i] = tile;
            else
                layer[i] = modificationLayer[i];
        }
       
        return layer;
    }

    private bool TileIsVisible(int index, int depth, TileType[] layer)
    {
        if (depth == depthData[index])
            return true;

        if (index + layerWidth < depthData.Length && depth < depthData[index + layerWidth])
            return true;

        if (index - layerWidth >= 0 && depth < depthData[index - layerWidth])
            return true;

        if (((index + 1) % layerLength != 0) && index + 1 < depthData.Length && depth < depthData[index + 1])
            return true;

        if ((index % layerLength != 0) && index - 1 >= 0 && depth < depthData[index - 1])
            return true;

        return false;
    }

    private TileType GetTile(int x, int y, int depth) //rand should 
    {
        if (depth == 0)
            return TileType.Grass; //first layer should always be grass
        var random = Random.Range(0, 100); //Perlin3D(x, y, depth) * 100;
        if (depth > 0 && depth < 10)
        {
            if (random > 70)
                return TileType.Rock;
            if (random > 40)
                return TileType.Dirt;
            if (random > 0)
                return TileType.Sand;
        }
        if (depth >= 10 && depth <= 30)
        {
            if (random > 60)
                return TileType.Rock;
            if (random > 30)
                return TileType.Dirt;
            if (random > 0)
                return TileType.Sand;
        }

        return TileType.Rock;
    }

    private void RecordModification(int layerIndex, int tileIndex, TileType newType)
    {
        layerModificationData[layerIndex][tileIndex] = newType;
        if (newType == TileType.Deleted)
        {
            depthData[tileIndex] = layerIndex + 1;
            maxDepth = Mathf.Max(depthData);
        }
    }

    public bool RecordDamage(int layerIndex, int tileIndex, bool isExplosion = false, int damageRange = 1)
    {
        var tiles = GetVisible(0);
        var currentTile = layerModificationData[layerIndex][tileIndex];
        if (currentTile == TileType.Unchanged)
        {
            currentTile = tiles[layerIndex][tileIndex];
        }
        switch(currentTile)
        {
            case TileType.Dirt:
                RecordModification(layerIndex, tileIndex, TileType.DirtMinusThrd);
                return false;
            case TileType.DirtMinusThrd:
                RecordModification(layerIndex, tileIndex, TileType.DirtMinus2Thrds);
                return false;
            case TileType.Rock:
                if (isExplosion)
                {
                    RecordModification(layerIndex, tileIndex, TileType.Deleted);
                    return true;
                }
                else
                    return false;
            default:
                RecordModification(layerIndex, tileIndex, TileType.Deleted);
                return true;
        }
    }

    //private static float Perlin3D(float x, float y, float z)
    //{
    //    var xf = x / 10f * 256f;
    //    var yf = y / 10f * 256f;
    //    var zf = z / 10f * 256f;

    //    float xy = Mathf.PerlinNoise(xf, yf);
    //    float yz = Mathf.PerlinNoise(yf, zf);
    //    float xz = Mathf.PerlinNoise(xf, zf);

    //    float yx = Mathf.PerlinNoise(yf, xf);
    //    float zy = Mathf.PerlinNoise(zf, yf);
    //    float zx = Mathf.PerlinNoise(zf, xf);

    //    float xyz = xy + yz + xz + yx + zy + zx;
    //    return xyz / 6f;;
    //}
        
}
