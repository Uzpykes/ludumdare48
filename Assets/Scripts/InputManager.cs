using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    public static UnityEvent<GameObject> onTileClick = new UnityEvent<GameObject>();
    public static UnityEvent<float> onMouseScroll = new UnityEvent<float>();

    public static UnityEvent<KeyCode> onMovementKeyDown = new UnityEvent<KeyCode>();

    private GameObject hitObject;

    private void OnEnable()
    {
    
    }

    private void Update()
    {
        if (hitObject != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (hitObject.layer == LayerMask.NameToLayer("Tile"))
                {
                    onTileClick?.Invoke(hitObject);
                    hitObject = null;
                }
            }
        }
        var mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(mouseScroll) > 0)
        {
            onMouseScroll?.Invoke(mouseScroll);
        }
    }

    private void FixedUpdate()
    {
        DoRaycast();
    }

    private void OnDestroy()
    {
        onTileClick.RemoveAllListeners();
        onMouseScroll.RemoveAllListeners();
        onMovementKeyDown.RemoveAllListeners();
    }

    private RaycastHit hit;

    private void DoRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            hitObject = hit.transform.gameObject;
        }
        else
            hitObject = null;
    }

}
