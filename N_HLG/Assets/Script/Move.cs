using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{
    private Renderer _Renderer;
    Vector3 screenPoint;

    void Awake()
    {
        transform.position = new Vector3((float)0.125, (float)0.125, 0);
        _Renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsVisible())
        {
            this.screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 a = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            transform.position = Camera.main.ScreenToWorldPoint(a);
        }

        if (Input.GetMouseButtonDown(0))
        {
            this.screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 a = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            transform.position = Camera.main.ScreenToWorldPoint(a);
        }

    }

    public bool IsVisible()
    {
        return _Renderer.isVisible;
    }

}
