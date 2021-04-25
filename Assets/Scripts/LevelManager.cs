using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public List<TileMaterial> materials;

    private LevelData data;
    private int currentTopLayer;
    private List<TileType[]> currentlyDrawnTiles;

    private int width = 8;
    private int length = 8;

    public static UnityEvent<Vector3Int> onTileDestroyed = new UnityEvent<Vector3Int>();
    public static UnityEvent<int> onMapMoved = new UnityEvent<int>();
    public static UnityEvent<int, int> onMapDraw = new UnityEvent<int, int>();

    private void OnEnable()
    {
        SetUpLevel();
    }

    private void Start()
    {
        //InputManager.onTileClick.AddListener(OnTileClick);
        InputManager.onMouseScroll.AddListener(OnMouseScroll);
        PlayerController.onTryToDestroy.AddListener(OnDamage);
        DinamiteBehaviour.onExplosion.AddListener(OnExplosionDamage);
        PlayerController.onFinishedFalling.AddListener(OnPlayerFinishedFalling);
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
        currentlyDrawnTiles = data.GetVisible(currentTopLayer);
        for (int i = 0; i < currentlyDrawnTiles.Count; i++)
        {
            var layer = currentlyDrawnTiles[i];
            var y = -i;
            for (int j = 0; j < layer.Length; j++)
            {
                if (layer[j] == TileType.Deleted || (i != 0 && layer[j] == TileType.Invisible)) //skip deleted tiles! Also don't render invisible tiles that aren't on top layer
                    continue;
                var go = GetTileInstance(layer[j]);
                if (layer[j] == TileType.Invisible)
                    go.layer = LayerMask.NameToLayer("InvisibleTile");
                else
                    go.layer = LayerMask.NameToLayer("Tile");
                int x = j % length;
                int z = j / width;
                go.transform.position = new Vector3(x, y, z);
            }
        }

        onMapDraw?.Invoke(data.maxDepth, currentTopLayer);
    }

    //private void OnTileClick(GameObject go)
    //{
    //    var pos = go.transform.position;
    //    data.RecordModification(currentTopLayer + (int)-pos.y, (int)pos.z * width + (int)pos.x);
    //    ReturnTileInstance(go);
    //    DrawLevel();
    //}

    private List<GameObject> tileInstances;
    private int instancesUsed;
    private GameObject GetTileInstance(TileType type)
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
        var mat = materials.Find(x => { return x.type == type; }).material;
        if (mat != null)
            go.GetComponent<MeshRenderer>().material = mat;
        else
            go.GetComponent<MeshRenderer>().material = materials.Find(x => { return x.type == TileType.Unchanged; }).material;
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

    private void OnMouseScroll(float amount)
    {
        var prev = currentTopLayer;
        currentTopLayer += (int)Mathf.Sign(amount) * -1;
        if (currentTopLayer < 0)
            currentTopLayer = 0;

        if (currentTopLayer > (data.maxDepth-1))
            currentTopLayer = prev;

        if (prev != currentTopLayer)
        {
            DrawLevel();
            onMapMoved?.Invoke(currentTopLayer - prev);
        }
    }

    private void OnPlayerFinishedFalling(int position)
    {
        var prev = currentTopLayer;
        currentTopLayer += -position;
        if (currentTopLayer < 0)
            currentTopLayer = 0;

        if (currentTopLayer > (data.maxDepth - 1))
            currentTopLayer = prev;

        if (prev != currentTopLayer)
        {
            DrawLevel();
            onMapMoved?.Invoke(currentTopLayer - prev);
        }
    }

    //private void OnTryToDestroy(Vector3Int position)
    //{
    //    var targetPosition = position;
    //    data.RecordModification(currentTopLayer + -position.y, Mathf.RoundToInt(targetPosition.z) * width + Mathf.RoundToInt(targetPosition.x)); //need same thing as in TileIsVisible as now it destroys tile on another side TODO
    //    onTileDestroyed?.Invoke(position);
    //    DrawLevel();
    //}

    private void OnDamage(Vector3Int position, DamageType type)
    {
        var range = 0;
        if (type == DamageType.Dinamite)
            range = 1;
        for (var y = (currentTopLayer + -(position.y + range)) < 0 ? 0 : (position.y + range); y >= position.y - range; y-- )
        {
            for (var x = position.x - range; x <= position.x + range; x++)
            {
                if (x < 0 || x >= width)
                    continue;
                for (var z = position.z - range; z <= position.z + range; z++)
                {
                    if (z < 0 || z >= length)
                        continue;
                    var wasDestroyed = data.RecordDamage(currentTopLayer + -y, z * width + x, type); //need same thing as in TileIsVisible as now it destroys tile on another side TODO
                    if (wasDestroyed)
                        onTileDestroyed?.Invoke(new Vector3Int(x, y, z));
                }
            }
        }

        DrawLevel();
    }

    private void OnExplosionDamage(Vector3Int position)
    {
        OnDamage(position, DamageType.Dinamite);
    }

    private void OnDisable()
    {
        InputManager.onMouseScroll.RemoveListener(OnMouseScroll);
        PlayerController.onTryToDestroy.RemoveListener(OnDamage);
        PlayerController.onFinishedFalling.RemoveListener(OnPlayerFinishedFalling);
    }
}

[System.Serializable]
public struct TileMaterial
{
    public TileType type;
    public Material material;
}

public enum DamageType : byte
{
    Pickaxe = 0,
    Dinamite = 1
}
