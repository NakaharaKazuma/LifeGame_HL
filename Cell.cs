using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {

    public bool Living { get; private set; }

    public bool Target;

    public bool Life;

    public bool lock_target = false;

    public bool run;

 //   private GameObject cube1;
    private GameObject cube2;
 //   private GameObject cube3;
    private GameObject cube4;

    public int x;
    public int y;
    public int z;

    public int target_d;

    public float scroll;

    public string cd;
    public Cameradir cameradir;

    public int gridSize;

    // Use this for initialization
    void Awake()
    {
//        cube1 = transform.Find("Cube01").gameObject;
        cube2 = transform.Find("Cube02").gameObject;
//        cube3 = transform.Find("Cube03").gameObject;
        cube4 = transform.Find("Cube04").gameObject;

//        cube1.SetActive(true);
        cube2.SetActive(false);
//        cube3.SetActive(false);
        cube4.SetActive(true);
        Living = false;
        Target = false;
        Life = false;
        run = false;

        cameradir = new Cameradir();

    }

    // Update is called once per frame
    void Update()
    {

        cd = cameradir.Cameradirection();

        if (Input.GetMouseButtonDown(1))
        {
            lock_target = !lock_target;
        }

        if (Input.GetMouseButtonDown(2))
        {
            cube4.SetActive(!lock_target);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            cube2.SetActive(false);
            Life = false;
        }

        if (lock_target == false)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(ray, out hit))
            {
                Cell cell = hit.collider.gameObject.transform.parent.GetComponent<Cell>();
                if(cell == this)
                {
                    Target = true;
                } else
                {
                    Target = false;
                }
                switch (cd)
                {
                    case "z-":
                        target_d = cell.z;
                        break;
                    case "z+":
                        target_d = cell.z;
                        break;
                    case "y+":
                        target_d = cell.y;
                        break;
                    case "y-":
                        target_d = cell.y;
                        break;
                    case "x+":
                        target_d = cell.x;
                        break;
                    case "x-":
                        target_d = cell.x;
                        break;
                }
                CheckTarget(cell);
            }
        } else 
        {
            scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                if (scroll >= 0)
                {
                    if (target_d < gridSize - 1)
                    {
                        switch (cd)
                        {                           
                            case "z+":
                                target_d = target_d + 1;
                                break;
                            case "y+":
                                target_d = target_d + 1;
                                break;                            
                            case "x+":
                                target_d = target_d + 1;
                                break;                            
                        }
                    }
                    if (target_d > 0)
                    {
                        switch (cd)
                        {
                            case "z-":
                                target_d = target_d - 1;
                                break;
                            case "y-":
                                target_d = target_d - 1;
                                break;
                            case "x-":
                                target_d = target_d - 1;
                                break;
                        }
                    }
                    switch (cd)
                    {
                        case "z-":
                            if (z >= target_d)
                            {
                                ChangeLayer(2);
                                cube2.SetActive(false);
                                cube4.SetActive(false);
                            }
                            break;
                        case "z+":
                            if (z <= target_d)
                            {
                                ChangeLayer(2);
                                cube2.SetActive(false);
                                cube4.SetActive(false);
                            }
                            break;
                        case "y+":
                            if (y <= target_d)
                            {
                                ChangeLayer(2);
                                cube2.SetActive(false);
                                cube4.SetActive(false);
                            }
                            break;
                        case "y-":
                            if (y >= target_d)
                            {
                                ChangeLayer(2);
                                cube2.SetActive(false);
                                cube4.SetActive(false);
                            }
                            break;
                        case "x+":
                            if (x <= target_d)
                            {
                                ChangeLayer(2);
                                cube2.SetActive(false);
                                cube4.SetActive(false);
                            }
                            break;
                        case "x-":
                            if (x >= target_d)
                            {
                                ChangeLayer(2);
                                cube2.SetActive(false);
                                cube4.SetActive(false);
                            }
                            break;
                    }
                    
                }
                else
                {
                    if (target_d > 0)
                    {
                        switch (cd)
                        {                            
                            case "z+":
                                target_d = target_d - 1;
                                break;
                            case "y+":
                                target_d = target_d - 1;
                                break;                            
                            case "x+":
                                target_d = target_d - 1;
                                break;                            
                        }
                    }
                    if (target_d < gridSize - 1)
                    {
                        switch (cd)
                        {
                            case "z-":
                                target_d = target_d + 1;
                                break;
                            case "y-":
                                target_d = target_d + 1;
                                break;
                            case "x-":
                                target_d = target_d + 1;
                                break;
                        }
                    }
                    switch (cd)
                    {
                        case "z-":
                            if (z <= target_d)
                            {
                                ChangeLayer(0);
                                if (!run)
                                {
                                    cube4.SetActive(true);
                                }
                                if (Life)
                                {
                                    cube2.SetActive(true);
                                }
                            }
                            break;
                        case "z+":
                            if (z >= target_d)
                            {
                                ChangeLayer(0);
                                if (!run)
                                {
                                    cube4.SetActive(true);
                                }
                                if (Life)
                                {
                                    cube2.SetActive(true);
                                }
                            }
                            break;
                        case "y+":
                            if (y >= target_d)
                            {
                                ChangeLayer(0);
                                if (!run)
                                {
                                    cube4.SetActive(true);
                                }
                                if (Life)
                                {
                                    cube2.SetActive(true);
                                }
                            }
                            break;
                        case "y-":
                            if (y <= target_d)
                            {
                                ChangeLayer(0);
                                if (!run)
                                {
                                    cube4.SetActive(true);
                                }
                                if (Life)
                                {
                                    cube2.SetActive(true);
                                }
                            }
                            break;
                        case "x+":
                            if (x >= target_d)
                            {
                                ChangeLayer(0);
                                if (!run)
                                {
                                    cube4.SetActive(true);
                                }
                                if (Life)
                                {
                                    cube2.SetActive(true);
                                }
                            }
                            break;
                        case "x-":
                            if (x <= target_d)
                            {
                                ChangeLayer(0);
                                if (!run)
                                {
                                    cube4.SetActive(true);
                                }
                                
                                if (Life)
                                {
                                    cube2.SetActive(true);
                                }
                            }
                            break;
                    }
                }
            }
        }

//        On_target();

        if (Input.GetKeyDown(KeyCode.H))
        {
            Target = false;
            run = true;
            cube4.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            run = false;
            cube4.SetActive(true);
        }
    }

 
    public void Birth()
    {
        /*        cube1.SetActive(false);
                cube2.SetActive(true);
                cube3.SetActive(false);*/

        switch (cd)
        {
            case "z-":
                if (z <= target_d)
                {
                    cube2.SetActive(true);
                }
                break;
            case "z+":
                if (z >= target_d)
                {
                    cube2.SetActive(true);
                }
                break;
            case "y+":
                if (y >= target_d)
                {
                    cube2.SetActive(true);
                }
                break;
            case "y-":
                if (y <= target_d)
                {
                    cube2.SetActive(true);
                }
                break;
            case "x+":
                if (x >= target_d)
                {
                    cube2.SetActive(true);
                }
                break;
            case "x-":
                if (x <= target_d)
                {
                    cube2.SetActive(true);
                }
                break;
        }

        Life = true;
    }

    public void Die()
    {
 //               cube1.SetActive(true);
                cube2.SetActive(false);
 //               cube3.SetActive(false);
/*
        switch (cd)
        {
            case "z-":
                if (z > target_d)
                {
                    cube2.SetActive(false);
                }
                break;
            case "z+":
                if (z < target_d)
                {
                    cube2.SetActive(false);
                }
                break;
            case "y+":
                if (y < target_d)
                {
                    cube2.SetActive(false);
                }
                break;
            case "y-":
                if (y > target_d)
                {
                    cube2.SetActive(false);
                }
                break;
            case "x+":
                if (x < target_d)
                {
                    cube2.SetActive(false);
                }
                break;
            case "x-":
                if (x > target_d)
                {
                    cube2.SetActive(false);
                }
                break;
        }
*/
        Life = false;
    }
/*
    public void On_target()
    {
        if (Target)
        {
            cube3.SetActive(true);
        }
        else
        {
            cube3.SetActive(false);
        }
    }
*/
    public void CheckTarget(Cell hit)
    {
        if (this == hit)
        {
            Target = true;
        }
        else
        {
            if (Target)
            {
                Target = false;
            }
        }
    }

    public void ChangeLayer(int layer)
    {
//        cube1.layer = layer;
        cube2.layer = layer;
//        cube3.layer = layer;
        cube4.layer = layer;
    }

}


