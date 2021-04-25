using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScoreManager
{
    public static int TotalScore { get; private set; } = 0;

    public static UnityEvent<int> onScoreChanged = new UnityEvent<int>();


    public void Reset()
    {
        TotalScore = 0;
    }

    public static void TileDestroyed(TileType type, int tileDepth)
    {
        int tileScore = 1;
        if (type == TileType.Diamond)
            tileScore = 100;
        else if (type == TileType.Gold)
            tileScore = 50;
        else if (type == TileType.Silver)
            tileScore = 20;
        else if (type == TileType.DirtMinus2Thrds)
            tileScore = 7;
        else if (type == TileType.Rock)
            tileScore = 5;

        tileScore += tileDepth;

        TotalScore += tileScore;

        onScoreChanged?.Invoke(tileScore);
    }

    public static void DepthReached(int newDepth)
    {
        var score = newDepth * 10;
        TotalScore += score;
        onScoreChanged?.Invoke(score);
    }


}
