using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject tilePrefab;

    private LevelData data;
    private int currentTopLayer;

    private int width = 8;
    private int length = 8;

    private void OnEnable()
    {
        SetUpLevel();
    }

    private void Start()
    {
        InputManager.onTileClick.AddListener(OnTileClick);
    }

    private void SetUpLevel()
    {
        data = new LevelData(length, width);
        tileInstances = new List<GameObject>();
        currentTopLayer = 0;
        DrawLevel();
    }

    private void DrawLevel()
    {
        ReturnAllInstances();
        var layers = data.GetVisible(currentTopLayer);
        for (int i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            var y = -i;
            for (int j = 0; j < layer.Length; j++)
            {
                if (layer[j] == TileType.Deleted) //skip deleted tiles!
                    continue;
                var go = GetTileInstance();
                int x = j % length;
                int z = j / width;
                go.transform.position = new Vector3(x, y, z);
            }
        }
    }

    private void OnTileClick(GameObject go)
    {
        var pos = go.transform.position;
        data.RecordModification((int)-pos.y, (int)pos.z * width + (int)pos.x);
        ReturnTileInstance(go);
        DrawLevel();
    }

    private List<GameObject> tileInstances;
    private int instancesUsed;
    private GameObject GetTileInstance()
    {
        GameObject go;
        if (tileInstances.Count > instancesUsed)
        {
            go = tileInstances[instancesUsed];
            go.SetActive(true);
        }
        else
        {
            go = Instantiate(tilePrefab);
            tileInstances.Add(go);
        }
        instancesUsed++;
        return go;
    }

    private void ReturnTileInstance(GameObject go)
    {
        var index = tileInstances.IndexOf(go);
        tileInstances[index] = tileInstances[instancesUsed-1]; //swap last used with instance we want to remove
        tileInstances[instancesUsed-1] = go;
        instancesUsed--;
        go.SetActive(false);
    }

    private void ReturnAllInstances()
    {
        foreach (var go in tileInstances)
            go.SetActive(false);
        instancesUsed = 0;
    }
}
