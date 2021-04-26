using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{

    public static bool InputIsBlocked = false;

    public static UnityEvent<float> onMouseScroll = new UnityEvent<float>();

    private GameObject hitObject;

    private void OnEnable()
    {
    
    }

    private void Update()
    {
        if (InputIsBlocked)
            return;
        if (hitObject != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (hitObject.layer == LayerMask.NameToLayer("Tile"))
                {
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
