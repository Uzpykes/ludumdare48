using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType : byte
{
    Unchanged = 0,
    Sand = 1,
    Rock = 10,
    DepletedRock = 11, //left after mining silver, gold or diamond. Cannot be destroyed with pick
    Dirt = 20,
    DirtMinusThrd = 21,
    DirtMinus2Thrds = 22,
    Gold = 30,
    Silver = 35,
    Diamond = 40,
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
                {
                    layer[i] = tile;
                }
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
        var random = Random.Range(0f, 100f); //Perlin3D(x, y, depth) * 100;

        float diamondThreshold = Mathf.Max(101f - (depth / 20f), 96f); //Chance is increased every 20 layers. Minimum 20 depth is required
        float goldThreshold = Mathf.Max(100f - (depth / 20f), 95f); //Chance for gold
        float silverThreshold = Mathf.Max(98f - (depth / 20f), 93f); //chance for silver
        float rockThreshold = Mathf.Max(70 - (depth / 30f), 60f); //No bigger then 50% chance
        float dirtThreshold = Mathf.Max(40 - (depth / 10f), 30f);
        float sandThreshold = 2f;

        if (random > diamondThreshold)
            return TileType.Diamond;
        if (random > goldThreshold)
            return TileType.Gold;
        if (random > silverThreshold)
            return TileType.Silver;
        if (random > rockThreshold)
            return TileType.Rock;
        if (random > dirtThreshold)
            return TileType.Dirt;
        if (random > sandThreshold)
            return TileType.Sand;

        return TileType.Rock; // Might be cool to have empty tile here
    }

    private void RecordModification(int layerIndex, int tileIndex, TileType newType, TileType previousType)
    {
        if (previousType == TileType.Gold)
            ResourceManager.ChangeCount(ResourceType.Gold, 1);
        else if (previousType == TileType.Silver)
            ResourceManager.ChangeCount(ResourceType.Silver, 1);
        else if (previousType == TileType.Diamond)
            ResourceManager.ChangeCount(ResourceType.Diamond, 1);
        else if (previousType != TileType.Deleted && newType == TileType.Deleted)
            ResourceManager.ChangeCount(ResourceType.Blocks, 1);


        layerModificationData[layerIndex][tileIndex] = newType;
        if (newType == TileType.Deleted)
        {
            depthData[tileIndex] = layerIndex + 1;
            maxDepth = Mathf.Max(depthData);
        }
    }

    public bool RecordDamage(int layerIndex, int tileIndex, DamageType type = DamageType.Pickaxe)
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
                if (type == DamageType.Dinamite)
                {
                    RecordModification(layerIndex, tileIndex, TileType.Deleted, currentTile);
                    return true;
                }
                else
                {
                    RecordModification(layerIndex, tileIndex, TileType.DirtMinusThrd, currentTile);
                    return false;
                }
            case TileType.DirtMinusThrd:
                if (type == DamageType.Dinamite)
                {
                    RecordModification(layerIndex, tileIndex, TileType.Deleted, currentTile);
                    return true;
                }
                else
                {
                    RecordModification(layerIndex, tileIndex, TileType.DirtMinus2Thrds, currentTile);
                    return false;
                }
            case TileType.Rock:
            case TileType.DepletedRock:
                if (type == DamageType.Dinamite)
                {
                    RecordModification(layerIndex, tileIndex, TileType.Deleted, currentTile);
                    return true;
                }
                else
                    return false;
            case TileType.Diamond:
            case TileType.Gold:
            case TileType.Silver:
                if (type == DamageType.Dinamite)
                {
                    RecordModification(layerIndex, tileIndex, TileType.Deleted, currentTile);
                    return true;
                }
                else
                {
                    RecordModification(layerIndex, tileIndex, TileType.DepletedRock, currentTile);
                    return false;
                }
            case TileType.Deleted: //probably not needed
                return false;
            default:
                RecordModification(layerIndex, tileIndex, TileType.Deleted, currentTile);
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
