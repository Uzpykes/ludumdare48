using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    public static UnityEvent<GameObject> onTileClick = new UnityEvent<GameObject>();

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
                }
            }
        }
    }

    private void FixedUpdate()
    {
        DoRaycast();
    }

    private void OnDestroy()
    {
        onTileClick.RemoveAllListeners();
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
