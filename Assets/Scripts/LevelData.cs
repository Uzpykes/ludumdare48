using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType : byte
{
    Unchanged = 0,
    Dirt = 1,
    Sand = 2,
    Rock = 3,

    Grass = 16, // special layer found on top
    Deleted = 99 //Tile that was deleted
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
        var layerIndex = startingLayer > offset ? startingLayer : offset;
        var endLayer = Mathf.Max(depthData) + 1;
        CalculateLayerBetween(layerIndex, endLayer);
        return visibleLayerData.GetRange(layerIndex, endLayer);
    }

    private void CalculateLayerBetween(int topLayer, int bottomLayer)
    {
        for (int i = 0; i < bottomLayer - topLayer; i++) //take a look here if it doesn't work!
        {
            if (visibleLayerData.Count <= i) //if there are less layers than we need then add new one
            { 
                visibleLayerData.Add(FillLayerData(new TileType[layerLength * layerWidth], null, i + topLayer));
                layerModificationData.Add(new TileType[layerLength * layerWidth]);
            }
            else //otherwise - override on top
                FillLayerData(visibleLayerData[i], layerModificationData[i], i + topLayer);
        }
    }

    private TileType[] FillLayerData(TileType[] layer, TileType[] modificationLayer, int depth)
    {
        var finalSeed = globalSeed + depth;
        Random.InitState(finalSeed);
        for (int i = 0; i < layer.Length; i++)
        {
            if (modificationLayer == null || modificationLayer[i] == TileType.Unchanged)
                layer[i] = GetTile(depth);
            else
                layer[i] = modificationLayer[i];
        }
        return layer;
    }

    private TileType GetTile(int depth) //rand should 
    {
        if (depth == 0)
            return TileType.Grass; //first layer should always be grass
        var random = Random.Range(0, 100);

        if (depth > 0 && depth < 10)
        {
            if (random > 70)
                return TileType.Rock;
            if (random > 40)
                return TileType.Dirt;
            if (random > 0)
                return TileType.Sand;
        }

        return TileType.Rock;
    }

    public void RecordModification(int layerIndex, int tileIndex)
    {
        layerModificationData[layerIndex][tileIndex] = TileType.Deleted;
        Debug.Log(tileIndex);
        Debug.Log(depthData.Length);
        depthData[tileIndex] = layerIndex + 1;
    }
        
}
