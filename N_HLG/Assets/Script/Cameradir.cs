using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cameradir : MonoBehaviour
{

    private const float CELL_SIZE = 0.025f;

    public int gridSize = 108;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public string Cameradirection()
    {
        Vector3 cd = Camera.main.transform.forward;

        Vector3 cp = Camera.main.transform.position;

        Vector3 center;

        float xPos = ((gridSize + (float)0.5) * CELL_SIZE) / 2;
        float yPos = ((gridSize + (float)0.5) * CELL_SIZE) / 2;
        float zPos = ((gridSize + (float)0.5) * CELL_SIZE) / 2;
        center = new Vector3(xPos, yPos, zPos);

        Vector3 dir = center - cp;

        float max = Math.Abs(cd.x);
        if (max < Math.Abs(cd.y))
        {
            max = Math.Abs(cd.y);
            if (max < Math.Abs(cd.z))
            {
                if (cd.z >= 0)
                {
                    return "z+";
                }
                else
                {
                    return "z-";
                }
            }
            else
            {
                if (cd.y >= 0)
                {
                    return "y+";
                }
                else
                {
                    return "y-";
                }
            }
        }
        else
        {
            if (max < Math.Abs(cd.z))
            {
                if (cd.z >= 0)
                {
                    return "z+";
                }
                else
                {
                    return "z-";
                }
            }
            else
            {
                if (cd.x >= 0)
                {
                    return "x+";
                }
                else
                {
                    return "x-";
                }
            }
        }
    }

}
