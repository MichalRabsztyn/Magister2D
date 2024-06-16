using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField]
    private Academy _academy;

    [SerializeField]
    private float _zoom = 1;

    private void Start()
    {
        if (_academy == null)
        {
            Debug.Log("Invalid Academy");
            return;
        }

        if (_zoom == 0.0f)
        {
            Debug.Log("Zoom can't be x0. Setting Zoom x1");
            _zoom = 1.0f;
        }

        FitOnScreen();
    }

    private Bounds GetBounds()
    {
        Bounds bound = new Bounds(_academy.transform.position, Vector3.zero);
        var rList = _academy.GetComponentsInChildren(typeof(Renderer));
        foreach (Renderer r in rList)
        {
            bound.Encapsulate(r.bounds);
        }

        return bound;
    }

    private void FitOnScreen()
    {
        Bounds bounds = GetBounds();
        Vector3 boundSize = bounds.size;
        float diagonal = Mathf.Sqrt((boundSize.x * boundSize.x) + (boundSize.y * boundSize.y) + (boundSize.z * boundSize.z));
        Camera.main.orthographicSize = diagonal / _zoom / 4.0f;
        Camera.main.transform.position = bounds.center + new Vector3(0.0f, 0.0f, -1.0f);
    }
}
