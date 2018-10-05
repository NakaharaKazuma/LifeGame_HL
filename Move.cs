using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{

    Vector3 screenPoint;

    void Awake()
    {
        transform.position = new Vector3((float)0.125, (float)0.125, 0);
    }

    // Update is called once per frame
    void Update()
    {
        this.screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 a = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        transform.position = Camera.main.ScreenToWorldPoint(a);
    }
}
