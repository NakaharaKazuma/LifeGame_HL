using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;



public class Hololens_LifeGame_3D : MonoBehaviour {

    public UnityEngine.UI.Text scoreLabel;

    public UnityEngine.UI.Text Target_X;
    public UnityEngine.UI.Text Target_Y;
    public UnityEngine.UI.Text Target_Z;

    public UnityEngine.UI.Button button_LG;
    public UnityEngine.UI.Button button_CP;

    private const float CELL_SIZE = 0.025f;

    public int gridSize = 100;
    public GameObject cellPrefab;

    private Cell[,,] cells;

    int time = 0;

    int add_time = 0;

    int mode = 0;

    int[,,] state;

    public Cell Target;

    string cd;

    public Cameradir cameradir;

    // Use this for initialization
    void Start () {
        cells = new Cell[gridSize, gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {

                    GameObject obj = Instantiate(cellPrefab) as GameObject;
                    obj.transform.SetParent(transform);

                    float xPos = (x - gridSize * 0.0125f) * CELL_SIZE;
                    float yPos = (y - gridSize * 0.0125f) * CELL_SIZE;
                    float zPos = (z - gridSize * 0.0125f) * CELL_SIZE;
                    obj.transform.localPosition = new Vector3(xPos, yPos, zPos);

                    cells[x, y, z] = obj.GetComponent<Cell>();
                    cells[x, y, z] = obj.GetComponent<Cell>();
                    cells[x, y, z].x = x;
                    cells[x, y, z].y = y;
                    cells[x, y, z].z = z;
                    cells[x, y, z].gridSize = gridSize;
                }
            }
        }

        state = new int[gridSize, gridSize, gridSize];

        cameradir = new Cameradir();

    }
	
	// Update is called once per frame
	void Update () {

        if (mode == 0)
        {
            Mode_LifeGame();
        }

    }

    /// <summary>
    /// ライフゲームモード
    /// </summary>

    private void Mode_LifeGame()
    {

        cd = cameradir.Cameradirection();

        if (Input.GetMouseButtonDown(0))
        {
            Cell cell = searchtarget();

            if (cell.Life)
            {
                cell.Die();
            }
            else
            {
                cell.Birth();
            }
        }


        if (Input.GetKeyDown(KeyCode.H))
        {
            StartCoroutine(LifeGameCoroutine());
            add_time = 1;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            StopAllCoroutines();
            add_time = 0;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Load();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {

            Update_allCell();

            time = time + 1;
            scoreLabel.text = time.ToString();

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            time = 0;
            scoreLabel.text = time.ToString();
        }

        Target = searchtarget();

        Target_X.text = "X:" + Target.x.ToString();
        Target_Y.text = "Y:" + Target.y.ToString();
        Target_Z.text = "Z:" + Target.z.ToString() + " " + cd;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Target.Target = false;
            if (scroll >=0)
            {
                DeepUp(cd);
            } else
            {
                DeepDown(cd);
            }
            Target.Target = true;
        }

    }



    /// <summary>
    /// ライフゲームの更新コルーチン
    /// </summary>
    /// <returns>The game coroutine.</returns>

    private IEnumerator LifeGameCoroutine()
    {
        while (true)
        {

            Update_allCell();

            time = time + add_time;
            scoreLabel.text = time.ToString();

            yield return new WaitForSeconds(0.03f);
        }
    }

    /// <summary>
    /// 全てのセルを更新
    /// </summary>

    private void Update_allCell()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    UpdateCell(x, y, z);
                }
            }
        }
    }



    /// <summary>
    /// セルの状態を更新
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="z">The z coordinate.</param>

    private void UpdateCell(int cellX, int cellY, int cellZ)
    {

        int count = 0;
        for (int x = cellX - 1; x <= cellX + 1; x++)
        {
            for (int y = cellY - 1; y <= cellY + 1; y++)
            {
                for (int z = cellZ - 1; z <= cellZ + 1; z++)
                {
                    if ((x == cellX && y == cellY) && z == cellZ)
                    {
                        continue;
                    }

                    if (cells[(x + gridSize) % gridSize, (y + gridSize) % gridSize, (z + gridSize) % gridSize].Life)
                    {
                        count++;
                    }
                }
            }
        }
        Cell Cell = cells[cellX, cellY, cellZ];
        if (Cell.Life)
        {

            if (count <= 1 || count >= 4)
            {
                Cell.Die();
            }
        }
        else
        {

            if (count == 3)
            {
                Cell.Birth();
            }
        }
    }

    private void Load()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    if (state[x, y, z] == 1)
                    {
                        cells[x, y, z].Birth();
                    }
                    else
                    {
                        cells[x, y, z].Die();
                    }
                }
            }
        }
    }


    private void Save()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    if (cells[x, y, z].Life)
                    {
                        state[x, y, z] = 1;
                    }
                    else
                    {
                        state[x, y, z] = 0;
                    }
                }
            }
        }
    }

    private Cell searchCell(int a, int b, int c)
    {
        return cells[a, b, c];
    }

    private Cell searchtarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit))
        {
            Cell cell = hit.collider.gameObject.transform.parent.GetComponent<Cell>();
            return cell;
        }
        return cells[0, 0, 0];
    }

    public void DeepUp(string a)
    {
        switch (a)
        {
            case "z-":
                if (Target.z > 0)
                {
                    Target = cells[Target.x, Target.y, Target.z - 1];
                }
                break;
            case "z+":
                if (Target.z < gridSize - 1)
                {
                    Target = cells[Target.x, Target.y, Target.z + 1];
                }
                break;
            case "y+":
                if (Target.y < gridSize - 1)
                {
                    Target = cells[Target.x, Target.y + 1, Target.z];
                }
                break;
            case "y-":
                if (Target.y > 0)
                {
                    Target = cells[Target.x, Target.y - 1, Target.z];
                }
                break;
            case "x+":
                if (Target.x < gridSize - 1)
                {
                    Target = cells[Target.x + 1, Target.y, Target.z];
                }
                break;
            case "x-":
                if (Target.x > 0)
                {
                    Target = cells[Target.x - 1, Target.y, Target.z];
                }
                break;
        }
    }

    public void DeepDown(string b)
    {
        switch (b)
        {
            case "z+":
                if (Target.z > 0)
                {
                    Target = cells[Target.x, Target.y, Target.z - 1];
                }
                break;
            case "z-":
                if (Target.z < gridSize - 1)
                {
                    Target = cells[Target.x, Target.y, Target.z + 1];
                }
                break;
            case "y-":
                if (Target.y < gridSize - 1)
                {
                    Target = cells[Target.x, Target.y + 1, Target.z];
                }
                break;
            case "y+":
                if (Target.y > 0)
                {
                    Target = cells[Target.x, Target.y - 1, Target.z];
                }
                break;
            case "x-":
                if (Target.x < gridSize - 1)
                {
                    Target = cells[Target.x + 1, Target.y, Target.z];
                }
                break;
            case "x+":
                if (Target.x > 0)
                {
                    Target = cells[Target.x - 1, Target.y, Target.z];
                }
                break;
        }
    }

}
