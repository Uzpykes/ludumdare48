using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    private Vector2Int minPosition = Vector2Int.zero;
    private Vector2Int maxPosition = new Vector2Int(7, 7);

    public static UnityEvent<Vector3Int> onTryToDestroy = new UnityEvent<Vector3Int>();
    public static UnityEvent<int> onFinishedFalling = new UnityEvent<int>();

    [SerializeField]
    private bool canMove = true;

    [SerializeField]
    private Vector3Int gridPosition; //y == 1 when player stands on top
    private Vector3 verticalOffset = Vector3.down * 0.5f;

    private KeyCode queuedKey = KeyCode.Escape;

    private bool isDestroying = false;
    private bool isMoving = false;

    private void OnEnable()
    {
        transform.position = gridPosition + verticalOffset;
    }

    private void Update()
    {
        if (canMove)
            Move();
        if (canMove)
            SyncTransformToGrid();
    }

    private void Move()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || queuedKey == KeyCode.W)
        {
            queuedKey = KeyCode.W;
            if (canMove)
            {
                queuedKey = KeyCode.Escape;
                if (up != null)
                    DoDestroyWrapper(Vector3Int.forward);
                else
                    DoMoveWrapper(Vector3Int.forward, 5f);
            }
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || queuedKey == KeyCode.A)
        {
            queuedKey = KeyCode.A;
            if (canMove)
            {
                queuedKey = KeyCode.Escape;
                if (left != null)
                    DoDestroyWrapper(Vector3Int.left);
                else
                    DoMoveWrapper(Vector3Int.left, 5f);
            }
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || queuedKey == KeyCode.S)
        {
            queuedKey = KeyCode.S;
            if (canMove)
            {
                queuedKey = KeyCode.Escape;
                if (down != null)
                    DoDestroyWrapper(Vector3Int.back);
                else
                    DoMoveWrapper(Vector3Int.back, 5f);
            }
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || queuedKey == KeyCode.D)
        {
            queuedKey = KeyCode.D;
            if (canMove)
            {
                queuedKey = KeyCode.Escape;
                if (right != null)
                    DoDestroyWrapper(Vector3Int.right);
                else
                    DoMoveWrapper(Vector3Int.right, 5f);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space) || queuedKey == KeyCode.Space)
        {
            if (canMove)
            {
                if (below != null)
                    DoDestroyWrapper(Vector3Int.down);
            }
        }
    }

    private void DoDestroyWrapper(Vector3Int position)
    {
        if (isDestroying)
            return;
        isDestroying = true;
        StartCoroutine(DoDestroy(position));
    }

    private IEnumerator DoDestroy(Vector3Int position)
    {
        canMove = false;
        //transform.LookAt(position + verticalOffset);
        yield return new WaitForSeconds(0.05f);
        onTryToDestroy?.Invoke(gridPosition + position);
        yield return new WaitForSeconds(0.1f);
        canMove = true;
        isDestroying = false;
    }

    private void FakeFall()
    {
        DoMoveWrapper(Vector3Int.down, 5f);
    }

    private void DoMoveWrapper(Vector3Int amount, float speed = 1f)
    {
        if (isMoving)
            return;
        isMoving = true;
        StartCoroutine(DoMove(amount, speed));
    }

    private IEnumerator DoMove(Vector3Int amount, float speed = 1f)
    {
        canMove = false;
        var start = gridPosition + verticalOffset;
        var target = gridPosition + amount + verticalOffset;
        bool move = true;
        float t = 0;
        while (move)
        {
            yield return null;
            t += (speed * Time.deltaTime);
            if (t > 1f)
            {
                t = 1f;
                move = false;
            }
            transform.position = Vector3.Slerp(start, target, t);
        }
        gridPosition = gridPosition + amount;
        SyncTransformToGrid();
        canMove = true;
        isMoving = false;
        if (amount.y < 0)
            onFinishedFalling?.Invoke(gridPosition.y);
    }

    private void Start()
    {
        LevelManager.onTileDestroyed.AddListener(OnTileRemoved);
        LevelManager.onMapMoved.AddListener(OnMapMoved);
    }

    private void OnTileRemoved(Vector3Int position)
    {
        if (gridPosition.x == position.x && gridPosition.z == position.z)
        {
            FakeFall();
        }
    }

    private void OnMapMoved(int diff)
    {
        gridPosition += Vector3Int.up * diff;
    }


    private void OnDestroy()
    {
        onTryToDestroy?.RemoveAllListeners();
        onFinishedFalling?.RemoveAllListeners();
    }


    private GameObject up;
    private GameObject left;
    private GameObject down;
    private GameObject right;
    private GameObject below;

    private void FixedUpdate()
    {
        up = DoRaycast(Vector3.forward);
        left = DoRaycast(Vector3.left);
        down = DoRaycast(Vector3.back);
        right = DoRaycast(Vector3.right);
        below = DoRaycast(Vector3.down);
    }

    private GameObject DoRaycast(Vector3 dir)
    {
        Ray ray = new Ray(transform.position + (Vector3.up / 2f), dir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1f))
        {
            return hit.transform.gameObject;
        }
        return null;
    }

    private void SyncTransformToGrid()
    {
        transform.position = gridPosition + verticalOffset;
    }
}
