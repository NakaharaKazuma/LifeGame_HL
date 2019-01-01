using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
//using CI.WSANative.Pickers;

#if UNITY_UWP
using Windows.Storage;
using System.Threading.Tasks;
#endif

public struct Cell
{
    public GameObject obj;

    public bool life;

    public Vector3 Pos;
}


public class LifeGame : MonoBehaviour
{

    Pattern bugs;

    public UnityEngine.UI.Text Tag;

    private const float CELL_SIZE = 0.025f;

    public GameObject CellPrefab;
    public Material Cell_01;
    public Material Cell_02;

    public GameObject TargetPrefab;
    public GameObject Target;

    public GameObject X;
    public GameObject Y;
    public GameObject Z;

    public GameObject Wall_XPrefab;
    public GameObject Wall_X;

    public GameObject Wall_YPrefab;
    public GameObject Wall_Y;

    public GameObject Wall_ZPrefab;
    public GameObject Wall_Z;

    //public GameObject op;

    public Cell[,,] cell;

    public Cell tg_cell;

    public int gridSize;

    public bool[,,] state;

    public bool[,,] state2;

    public bool[,,] able;

    public bool[,,,] hush;

    public bool chenge_state;

    public bool edit_state = true;
    public bool change_depth = false;

    public bool tapping = false;

    public Vector3Int tg;

    public Vector3 Wall_Xpos;
    public Vector3 Wall_Ypos;
    public Vector3 Wall_Zpos;

    public int block;

    public int time = 0;

    public int range;

    public int memory_time = 0;

    public int[] rule;

    public float h1;
    public float v1;

    public float scroll;

    public Cameradir cameradir;

    public Vector3 position;
    public Vector3 pre_position;

    string fileString;

    // Use this for initialization
    void Start()
    {
        /*
        InteractionManager.InteractionSourceUpdated += InteractionSourceUpdated;
        InteractionManager.InteractionSourcePressed += InteractionSourcePressed;
        InteractionManager.InteractionSourceReleased += InteractionSourceReleased;
        */
        
        rule = new int[] {4, 102, 133, 102, 142};

        range = rule[0] * 2 + 1;
        bugs = new Pattern();
        tg = new Vector3Int(0, 0, 0);
        block = gridSize / range;

        state = new bool[gridSize, gridSize, gridSize];

        state2 = new bool[block, block, block];

        able = new bool[block, block, block];

        hush = new bool[gridSize, gridSize, gridSize, 50];

        cell = new Cell[gridSize, gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    cell[x, y, z].life = false;

                    cell[x, y, z].Pos = new Vector3((x + (float)0.5) * CELL_SIZE, (y + (float)0.5) * CELL_SIZE, (z + (float)0.5) * CELL_SIZE);
                }

            }
        }

        Target = Instantiate(TargetPrefab) as GameObject;
        Target.transform.localPosition = cell[0, 0, 0].Pos;

        Wall_X = Instantiate(Wall_XPrefab) as GameObject;
        Wall_Y = Instantiate(Wall_YPrefab) as GameObject;
        Wall_Z = Instantiate(Wall_ZPrefab) as GameObject;

        X.transform.localScale = new Vector3(CELL_SIZE * gridSize, CELL_SIZE, CELL_SIZE);
        Y.transform.localScale = new Vector3(CELL_SIZE, CELL_SIZE * gridSize, CELL_SIZE);
        Z.transform.localScale = new Vector3(CELL_SIZE, CELL_SIZE, CELL_SIZE * gridSize);
        Wall_X.transform.localScale = new Vector3(CELL_SIZE * gridSize, CELL_SIZE * gridSize, 0.0001f);
        Wall_Y.transform.localScale = new Vector3(CELL_SIZE * gridSize, CELL_SIZE * gridSize, 0.0001f);
        Wall_Z.transform.localScale = new Vector3(CELL_SIZE * gridSize, CELL_SIZE * gridSize, 0.0001f);

        X.transform.localPosition = new Vector3(cell[gridSize - 1, 0, 0].Pos.x / 2, cell[0, 0, 0].Pos.y, cell[0, 0, 0].Pos.z - CELL_SIZE);
        Y.transform.localPosition = new Vector3(cell[0, 0, 0].Pos.x - CELL_SIZE, cell[0, gridSize - 1, 0].Pos.y / 2, cell[0, 0, 0].Pos.z - CELL_SIZE);
        Z.transform.localPosition = new Vector3(cell[0, 0, 0].Pos.x - CELL_SIZE, cell[0, 0, 0].Pos.y, cell[0, 0, gridSize - 1].Pos.z / 2);
        Wall_X.transform.localPosition = new Vector3(cell[0, 0, 0].Pos.x, cell[0, gridSize - 1, 0].Pos.y / 2, cell[0, 0, gridSize - 1].Pos.z / 2);
        Wall_Y.transform.localPosition = new Vector3(cell[gridSize - 1, 0, 0].Pos.x / 2, cell[0, 0, 0].Pos.y, cell[0, 0, gridSize - 1].Pos.z / 2);
        Wall_Z.transform.localPosition = new Vector3(cell[gridSize - 1, 0, 0].Pos.x / 2, cell[0, gridSize - 1, 0].Pos.y / 2, cell[0, 0, 0].Pos.z);
        Wall_Xpos = Wall_X.transform.localPosition;
        Wall_Ypos = Wall_Y.transform.localPosition;
        Wall_Zpos = Wall_Z.transform.localPosition;

        Instantiate(X);
        Instantiate(Y);
        Instantiate(Z);
        //Wall.SetActive(false);
                
        cameradir = new Cameradir();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //cell_up();
            Scan();
            Upcell();
            time += 1;
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
             chenge_state = !chenge_state;
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button6))
        {
            switch (cameradir.Cameradirection())
            {
                case "x+":
                    if (tg.x < gridSize - 1)
                    {
                        Wall_X.transform.position += new Vector3(CELL_SIZE, 0, 0);
                        tg.x++;
                    }
                    break;
                case "x-":
                    if (tg.x > 0)
                    {
                        Wall_X.transform.position -= new Vector3(CELL_SIZE, 0, 0);
                        tg.x--;
                    }
                    break;
                case "y+":
                    if (tg.y < gridSize - 1)
                    {
                        Wall_Y.transform.position += new Vector3(0, CELL_SIZE, 0);
                        tg.y++;
                    }
                    break;
                case "y-":
                    if (tg.y > 0)
                    {
                        Wall_Y.transform.position -= new Vector3(0, CELL_SIZE, 0);
                        tg.y--;
                    }
                    break;
                case "z+":
                    if (tg.z < gridSize - 1)
                    {
                        Wall_Z.transform.position += new Vector3(0, 0, CELL_SIZE);
                        tg.z++;
                    }
                    break;
                case "z-":
                    if (tg.z > 0)
                    {
                        Wall_Z.transform.position -= new Vector3(0, 0, CELL_SIZE);
                        tg.z--;
                    }
                    break;
            }
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button7))
        {
            switch (cameradir.Cameradirection())
            {
                case "x+":
                    if (tg.x > 0)
                    {
                        Wall_X.transform.position -= new Vector3(CELL_SIZE, 0, 0);
                        tg.x--;
                    }
                    break;
                case "x-":
                    if (tg.x < gridSize - 1)
                    {
                        Wall_X.transform.position += new Vector3(CELL_SIZE, 0, 0);
                        tg.x++;
                    }
                    break;
                case "y+":
                    if (tg.y > 0)
                    {
                        Wall_Y.transform.position -= new Vector3(0, CELL_SIZE, 0);
                        tg.y--;
                    }
                    break;
                case "y-":
                    if (tg.y < gridSize - 1)
                    {
                        Wall_Y.transform.position += new Vector3(0, CELL_SIZE, 0);
                        tg.y++;
                    }
                    break;
                case "z+":
                    if (tg.z > 0)
                    {
                        Wall_Z.transform.position -= new Vector3(0, 0, CELL_SIZE);
                        tg.z--;
                    }
                    break;
                case "z-":
                    if (tg.z < gridSize - 1)
                    {
                        Wall_Z.transform.position += new Vector3(0, 0, CELL_SIZE);
                        tg.z++;
                    }
                    break;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            Reset_cell();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Birth_bug(bugs.q0, bugs.q0_x, bugs.q0_y, bugs.q0_z, new int[] { 20, 20, 20 });
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            Birth_bug(bugs.bug, bugs.bug_x, bugs.bug_y, bugs.bug_z, new int[] { 17, 35, 16 });
            Birth_bug(bugs.q1, bugs.q1_x, bugs.q1_y, bugs.q1_z, new int[] { 20, 20, 20 });
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            Birth_bug(bugs.p0, bugs.p0_x, bugs.p0_y, bugs.p0_z, new int[] { 10, 11, 10 });
            Birth_bug(bugs.p0, bugs.p0_x, bugs.p0_y, bugs.p0_z, new int[] { 10, 11, 28 });
            Birth_bug(bugs.bug, bugs.bug_x, bugs.bug_y, bugs.bug_z, new int[] { 23, 10, 20 });
        }
                
        if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.Joystick1Button2))
        {
            Scan();
            StartCoroutine(LifeGameCoroutine());
        }

        if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.Joystick1Button4))
        {
            StopAllCoroutines();
        }
        /*
        if (Input.GetKeyDown(KeyCode.C))
        {
            Clean_cell();
        }
        */
        if (Input.GetKeyDown(KeyCode.T))
        {
            Scan();
            for (int i = 0; i < 10; i++)
            {
                Upcell();
                time += 1;
            }
        }
        /*
        if (Input.GetKeyDown(KeyCode.S))
        {
            string data = "";
            for (int x0 = 0; x0 < block; x0++)
            {
                for (int y0 = 0; y0 < block; y0++)
                {
                    for (int z0 = 0; z0 < block; z0++)
                    {
                        if (able[x0, y0, z0])
                        {
                            for (int a = x0 * range; a < (x0 + 1) * range; a++)
                            {
                                for (int b = y0 * range; b < (y0 + 1) * range; b++)
                                {
                                    for (int c = z0 * range; c < (z0 + 1) * range; c++)
                                    {
                                        if (cell[a, b, c].life)
                                        {
                                            data += "[" + a.ToString() + "," + b.ToString() + "," + b.ToString() + "]\n";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            WSANativeFilePicker.PickSaveFile("Save", ".txt", "Test Text File", WSAPickerLocationId.DocumentsLibrary, new List<KeyValuePair<string, IList<string>>>() { new KeyValuePair<string, IList<string>>("Text Files", new List<string>() { ".txt" }) }, result =>
            {
                if (result != null)
                {
                    result.WriteText(data);
                }
            });

        }
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            WSANativeFilePicker.PickSingleFile("Select", WSAPickerViewMode.Thumbnail, WSAPickerLocationId.PicturesLibrary, new[] { ".txt" }, result =>
            {
                string fileString = "";
                //string data = "";

                if (result != null)
                {
#pragma warning disable 0219
                    fileString = result.ReadText();
#pragma warning restore 0219
                    int x = 20;
                    int y = 20;
                    int z = 20;
                    int i = 0;
                    while (true)
                    {
                        y = 20;
                        while (true)
                        {
                            z = 20;
                            while (true)
                            {
                                if (fileString[i] == '0')
                                {
                                    if (cell[x, y, z].life)
                                    {
                                        Destroy(cell[x, y, z].obj);
                                        cell[x, y, z].life = false;
                                        state[x, y, z] = false;
                                        hush[x, y, z, memory_time] = false;
                                    }
                                    z++;
                                    i++;
                                }
                                else if (fileString[i] == '1')
                                {
                                    if (!cell[x, y, z].life)
                                    {
                                        cell[x, y, z].obj = Instantiate(CellPrefab) as GameObject;
                                        cell[x, y, z].obj.transform.localPosition = cell[x, y, z].Pos;
                                        cell[x, y, z].life = true;
                                        state[x, y, z] = true;
                                        hush[x, y, z, memory_time] = true;
                                    }
                                    z++;
                                    i++;
                                }
                                else if (fileString[i] == '\n')
                                {
                                    i++;
                                    break;
                                }
                            }
                            if (fileString[i] == '\n')
                            {
                                i++;
                                break;
                            }
                            else if (fileString[i] == '0' || fileString[i] == '1')
                            {
                                y++;
                            }
                        }
                        if (fileString[i] == '0' || fileString[i] == '1')
                        {
                            x++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            });
        }
        */
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Joystick1Button13))
        {
            memory_time -= 1;
            if (memory_time < 0)
            {
                memory_time = 50;
            }
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    for (int z = 0; z < gridSize; z++)
                    {
                        if (cell[x, y, z].life)
                        {
                            if (!hush[x, y, z, memory_time])
                            {
                                Destroy(cell[x, y, z].obj);
                            }
                        }
                        else
                        {
                            if (hush[x, y, z, memory_time])
                            {
                                cell[x, y, z].obj = Instantiate(CellPrefab) as GameObject;

                                cell[x, y, z].obj.transform.localPosition = cell[x, y, z].Pos;

                                if (z < tg.z)
                                {
                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                }
                            }
                        }
                        cell[x, y, z].life = hush[x, y, z, memory_time];
                        state[x, y, z] = hush[x, y, z, memory_time];
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Joystick1Button12))
        {
            memory_time += 1;
            if (memory_time == 50)
            {
                memory_time = 0;
            }
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    for (int z = 0; z < gridSize; z++)
                    {
                        if (cell[x, y, z].life)
                        {
                            if (!hush[x, y, z, memory_time])
                            {
                                Destroy(cell[x, y, z].obj);
                            }
                        }
                        else
                        {
                            if (hush[x, y, z, memory_time])
                            {
                                cell[x, y, z].obj = Instantiate(CellPrefab) as GameObject;

                                cell[x, y, z].obj.transform.localPosition = cell[x, y, z].Pos;

                                if (z < tg.z)
                                {
                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                }
                            }
                        }
                        cell[x, y, z].life = hush[x, y, z, memory_time];
                        state[x, y, z] = hush[x, y, z, memory_time];
                    }
                }
            }
        }
        /*
        switch (cameradir.Cameradirection())
        {
            case "x+":
                Wall_X.SetActive(true);
                Wall_Y.SetActive(false);
                Wall_Z.SetActive(false);
                break;
            case "x-":
                Wall_X.SetActive(true);
                Wall_Y.SetActive(false);
                Wall_Z.SetActive(false);
                break;
            case "y+":
                Wall_X.SetActive(false);
                Wall_Y.SetActive(true);
                Wall_Z.SetActive(false);
                break;
            case "y-":
                Wall_X.SetActive(false);
                Wall_Y.SetActive(true);
                Wall_Z.SetActive(false);
                break;
            case "z+":
                Wall_X.SetActive(false);
                Wall_Y.SetActive(false);
                Wall_Z.SetActive(true);
                break;
            case "z-":
                Wall_X.SetActive(false);
                Wall_Y.SetActive(false);
                Wall_Z.SetActive(true);
                break;
        }
        */
        Tag.text = "(" + tg.x.ToString() + ", " + tg.y.ToString() + ", " + tg.z.ToString() + ")";

    }

    private IEnumerator LifeGameCoroutine()
    {
        while (true)
        {
            Upcell();
            time += 1;
            yield return new WaitForSeconds(0.03f);
        }
    }

    private void Scan()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    if (state2[x / range, y / range, z / range])
                    {
                        continue;
                    }

                    if (cell[x, y, z].life)
                    {
                        state2[x / range, y / range, z / range] = true;
                    }
                }
            }
        }
    }

    private void Upcell()
    {
        for (int x0 = 0; x0 < block; x0++)
        {
            for (int y0 = 0; y0 < block; y0++)
            {
                for (int z0 = 0; z0 < block; z0++)
                {
                    if (state2[x0, y0, z0])
                    {
                        for (int a = x0 - 1; a < x0 + 2; a++)
                        {
                            for (int b = y0 - 1; b < y0 + 2; b++)
                            {
                                for (int c = z0 - 1; c < z0 + 2; c++)
                                {
                                    able[(a + block) % block, (b + block) % block, (c + block) % block] = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        for (int x0 = 0; x0 < block; x0++)
        {
            for (int y0 = 0; y0 < block; y0++)
            {
                for (int z0 = 0; z0 < block; z0++)
                {
                    if (able[x0, y0, z0])
                    {
                        Up(x0 * range, y0 * range, z0 * range, (x0 + 1) * range, (y0 + 1) * range, (z0 + 1) * range);
                    }
                }
            }
        }
        
        memory_time += 1;
        if (memory_time == 50)
        {
            memory_time = 1;
        }
        
        for (int x0 = 0; x0 < block; x0++)
        {
            for (int y0 = 0; y0 < block; y0++)
            {
                for (int z0 = 0; z0 < block; z0++)
                {
                    if (able[x0, y0, z0])
                    {
                        state2[x0, y0, z0] = false;
                        able[x0, y0, z0] = false;
                        for (int a = x0 * range; a < (x0 + 1) * range; a++)
                        {
                            for (int b = y0 * range; b < (y0 + 1) * range; b++)
                            {
                                for (int c = z0 * range; c < (z0 + 1) * range; c++)
                                {/*
                                    if (cell[a, b, c].life)
                                    {
                                        if (!state[a, b, c])
                                        {
                                            Destroy(cell[a, b, c].obj);
                                        }
                                    }
                                    else
                                    {
                                        if (state[a, b, c])
                                        {
                                            cell[a, b, c].obj = Instantiate(CellPrefab) as GameObject;
                                            cell[a, b, c].obj.transform.localPosition = cell[a, b, c].Pos;
                                            if (c < tg.z)
                                            {
                                                cell[a, b, c].obj.GetComponent<Renderer>().material = Cell_02;
                                            }
                                        }
                                    }*/
                                    cell[a, b, c].life = state[a, b, c];
                                    hush[a, b, c, memory_time] = state[a, b, c];
                                    if (cell[a, b, c].life)
                                    {
                                        state2[x0, y0, z0] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void Up(int start_x, int start_y, int start_z, int end_x, int end_y, int end_z)
    {
        int sum = 0;
        for (int x = start_x - rule[0]; x < start_x + (rule[0] + 1); x++)
        {
            for (int y = start_y - rule[0]; y < start_y + (rule[0] + 1); y++)
            {
                for (int z = start_z - rule[0]; z < start_z + (rule[0] + 1); z++)
                {
                    if (cell[(x + gridSize) % gridSize, (y + gridSize) % gridSize, (z + gridSize) % gridSize].life)
                    {
                        sum++;
                    }
                }
            }
        }

        for (int x = start_x; x < end_x; x++)
        {
            if (((x % range) % 2) == 0)
            {
                for (int y = start_y; y < end_y; y++)
                {
                    if (((y % range) % 2) == 0)
                    {
                        for (int z = start_z; z < end_z; z++)
                        {
                            if (cell[x, y, z].life)
                            {
                                if (sum < rule[3] || sum > rule[4])
                                {
                                    state[x, y, z] = false;
                                    Destroy(cell[x, y, z].obj);
                                }
                            }
                            else
                            {
                                if (sum >= rule[1] && sum <= rule[2])
                                {
                                    state[x, y, z] = true;
                                    cell[x, y, z].obj = Instantiate(CellPrefab) as GameObject;
                                    cell[x, y, z].obj.transform.localPosition = cell[x, y, z].Pos;
                                }
                            }
                            if (z != end_z - 1)
                            {
                                for (int x1 = x - rule[0]; x1 < x + (rule[0] + 1); x1++)
                                {
                                    for (int y1 = y - rule[0]; y1 < y + (rule[0] + 1); y1++)
                                    {
                                        if (cell[(x1 + gridSize) % gridSize, (y1 + gridSize) % gridSize, ((z - rule[0]) + gridSize) % gridSize].life)
                                        {
                                            sum--;
                                        }
                                        if (cell[(x1 + gridSize) % gridSize, (y1 + gridSize) % gridSize, ((z + (rule[0] + 1)) + gridSize) % gridSize].life)
                                        {
                                            sum++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int z = end_z - 1; z >= start_z; z--)
                        {
                            if (cell[x, y, z].life)
                            {
                                if (sum < rule[3] || sum > rule[4])
                                {
                                    state[x, y, z] = false;
                                    Destroy(cell[x, y, z].obj);
                                }
                            }
                            else
                            {
                                if (sum >= rule[1] && sum <= rule[2])
                                {
                                    state[x, y, z] = true;
                                    cell[x, y, z].obj = Instantiate(CellPrefab) as GameObject;
                                    cell[x, y, z].obj.transform.localPosition = cell[x, y, z].Pos;
                                }
                            }
                            if (z != start_z)
                            {
                                for (int x1 = x - rule[0]; x1 < x + (rule[0] + 1); x1++)
                                {
                                    for (int y1 = y - rule[0]; y1 < y + (rule[0] + 1); y1++)
                                    {
                                        if (cell[(x1 + gridSize) % gridSize, (y1 + gridSize) % gridSize, ((z + rule[0]) + gridSize) % gridSize].life)
                                        {
                                            sum--;
                                        }
                                        if (cell[(x1 + gridSize) % gridSize, (y1 + gridSize) % gridSize, ((z - (rule[0] + 1)) + gridSize) % gridSize].life)
                                        {
                                            sum++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (y != end_y - 1)
                    {
                        int nz;
                        if (((y % range) % 2) == 0)
                        {
                            nz = end_z - 1;
                        }
                        else
                        {
                            nz = start_z;
                        }
                        for (int x2 = x - rule[0]; x2 < x + (rule[0] + 1); x2++)
                        {
                            for (int z1 = nz - rule[0]; z1 < nz + (rule[0] + 1); z1++)
                            {
                                if (cell[(x2 + gridSize) % gridSize, ((y - rule[0]) + gridSize) % gridSize, (z1 + gridSize) % gridSize].life)
                                {
                                    sum--;
                                }
                                if (cell[(x2 + gridSize) % gridSize, ((y + (rule[0] + 1)) + gridSize) % gridSize, (z1 + gridSize) % gridSize].life)
                                {
                                    sum++;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int y = end_y - 1; y >= start_y; y--)
                {
                    if (((y % range) % 2) != 0)
                    {
                        for (int z = start_z; z < end_z; z++)
                        {
                            if (cell[x, y, z].life)
                            {
                                if (sum < rule[3] || sum > rule[4])
                                {
                                    state[x, y, z] = false;
                                    Destroy(cell[x, y, z].obj);
                                }
                            }
                            else
                            {
                                if (sum >= rule[1] && sum <= rule[2])
                                {
                                    state[x, y, z] = true;
                                    cell[x, y, z].obj = Instantiate(CellPrefab) as GameObject;
                                    cell[x, y, z].obj.transform.localPosition = cell[x, y, z].Pos;
                                }
                            }
                            if (z != end_z - 1)
                            {
                                for (int x1 = x - rule[0]; x1 < x + (rule[0] + 1); x1++)
                                {
                                    for (int y1 = y - rule[0]; y1 < y + (rule[0] + 1); y1++)
                                    {
                                        if (cell[(x1 + gridSize) % gridSize, (y1 + gridSize) % gridSize, ((z - rule[0]) + gridSize) % gridSize].life)
                                        {
                                            sum--;
                                        }
                                        if (cell[(x1 + gridSize) % gridSize, (y1 + gridSize) % gridSize, ((z + (rule[0] + 1)) + gridSize) % gridSize].life)
                                        {
                                            sum++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int z = end_z - 1; z >= start_z; z--)
                        {
                            if (cell[x, y, z].life)
                            {
                                if (sum < rule[3] || sum > rule[4])
                                {
                                    state[x, y, z] = false;
                                    Destroy(cell[x, y, z].obj);
                                }
                            }
                            else
                            {
                                if (sum >= rule[1] && sum <= rule[2])
                                {
                                    state[x, y, z] = true;
                                    cell[x, y, z].obj = Instantiate(CellPrefab) as GameObject;
                                    cell[x, y, z].obj.transform.localPosition = cell[x, y, z].Pos;
                                }
                            }
                            if (z != start_z)
                            {
                                for (int x1 = x - rule[0]; x1 < x + (rule[0] + 1); x1++)
                                {
                                    for (int y1 = y - rule[0]; y1 < y + (rule[0] + 1); y1++)
                                    {
                                        if (cell[(x1 + gridSize) % gridSize, (y1 + gridSize) % gridSize, ((z + rule[0]) + gridSize) % gridSize].life)
                                        {
                                            sum--;
                                        }
                                        if (cell[(x1 + gridSize) % gridSize, (y1 + gridSize) % gridSize, ((z - (rule[0] + 1)) + gridSize) % gridSize].life)
                                        {
                                            sum++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (y != start_y)
                    {
                        int nz;
                        if (((y % range) % 2) != 0)
                        {
                            nz = end_z - 1;
                        }
                        else
                        {
                            nz = start_z;
                        }
                        for (int x2 = x - rule[0]; x2 < x + (rule[0] + 1); x2++)
                        {
                            for (int z1 = nz - rule[0]; z1 < nz + (rule[0] + 1); z1++)
                            {
                                if (cell[(x2 + gridSize) % gridSize, ((y + rule[0]) + gridSize) % gridSize, (z1 + gridSize) % gridSize].life)
                                {
                                    sum--;
                                }
                                if (cell[(x2 + gridSize) % gridSize, ((y - (rule[0] + 1)) + gridSize) % gridSize, (z1 + gridSize) % gridSize].life)
                                {
                                    sum++;
                                }
                            }
                        }
                    }
                }
            }
            if (x != end_x)
            {
                int ny;
                int nz;
                if (((x % range) % 2) == 0)
                {
                    ny = end_y - 1;
                    nz = start_z;
                    if ((range % 2) == 1)
                    {
                        nz = end_z - 1;
                    }
                }
                else
                {
                    ny = start_y;
                    nz = start_z;
                }
                for (int y2 = ny - rule[0]; y2 < ny + (rule[0] + 1); y2++)
                {
                    for (int z2 = nz - rule[0]; z2 < nz + (rule[0] + 1); z2++)
                    {
                        if (cell[((x - rule[0]) + gridSize) % gridSize, (y2 + gridSize) % gridSize, (z2 + gridSize) % gridSize].life)
                        {
                            sum--;
                        }
                        if (cell[((x + (rule[0] + 1)) + gridSize) % gridSize, (y2 + gridSize) % gridSize, (z2 + gridSize) % gridSize].life)
                        {
                            sum++;
                        }
                    }
                }
            }
        }
    }





    public void Birth_bug(int[,,] bug, int b_x, int b_y, int b_z, int[] start)
    {

        Cell bgn;

        for (int x = 0; x < b_x; x++)
        {
            for (int y = 0; y < b_y; y++)
            {
                for (int z = 0; z < b_z; z++)
                {

                    bgn = cell[start[0] + y, start[1] + x, start[2] + z];
                    if (bug[x, y, z] == 0)
                    {
                        if (cell[start[0] + y, start[1] + x, start[2] + z].life == true)
                        {
                            Destroy(cell[start[0] + y, start[1] + x, start[2] + z].obj);
                            cell[start[0] + y, start[1] + x, start[2] + z].life = false;
                            state[start[0] + y, start[1] + x, start[2] + z] = false;
                            hush[start[0] + y, start[1] + x, start[2] + z, memory_time] = false;
                        }
                    }
                    else if (bug[x, y, z] == 1)
                    {
                        if (cell[start[0] + y, start[1] + x, start[2] + z].life == false)
                        {
                            cell[start[0] + y, start[1] + x, start[2] + z].obj = Instantiate(CellPrefab) as GameObject;
                            cell[start[0] + y, start[1] + x, start[2] + z].obj.transform.localPosition = cell[start[0] + y, start[1] + x, start[2] + z].Pos;
                            cell[start[0] + y, start[1] + x, start[2] + z].life = true;
                            state[start[0] + y, start[1] + x, start[2] + z] = true;
                            hush[start[0] + y, start[1] + x, start[2] + z, memory_time] = true;
                            if (start[2] + z < tg.z)
                            {
                                cell[start[0] + y, start[1] + x, start[2] + z].obj.GetComponent<Renderer>().material = Cell_02;
                            }
                        }
                    }
                }
            }
        }
    }

    public void Reset_cell()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    if (cell[x, y, z].life)
                    {
                        Destroy(cell[x, y, z].obj);
                        cell[x, y, z].life = false;
                        state[x, y, z] = false;
                    }
                    time = 0;
                }
            }
        }        
    }
    /*
    public void Clean_cell()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    if (cell[x, y, z].life)
                    {
                        cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_01;
                    }
                }
            }
        }
        //cell[tg.x, tg.y, tg.z].obj.layer = 0;
        
        Wall.SetActive(false);
        Target.SetActive(false);
        
        //Wall.transform.localPosition = new Vector3(cell[gridSize - 1, 0, 0].Pos.x / 2, cell[0, gridSize - 1, 0].Pos.y / 2, cell[0, 0, 0].Pos.x + CELL_SIZE / 2);
        //Wall_pos = Wall.transform.localPosition;
        //tg.z = 0;
    }
    */
    public int pos(float a)
    {
        if (cell[0, 0, 0].Pos.x == a)
        {
            return 0;
        }
        else
        {
            float pos = a / CELL_SIZE;
            return (int)pos;
        }
    }
    /*
    public void ChengeActive(Cell target, bool state)
    {
        if (state)
        {
            target.obj.GetComponent<Renderer>().material = Cell_01;
            //target.obj.transform.localPosition = target.Pos;
        }
        else
        {
            target.obj.GetComponent<Renderer>().material = Cell_02;
            //target.obj.transform.localPosition = target.Pos;
        }
    }

    // HoloLensが手を検出している間、位置が変わったりするたびに呼ばれる。
    private void InteractionSourceUpdated(InteractionSourceUpdatedEventArgs ev)
    {
        Vector3 position;
        if (ev.state.sourcePose.TryGetPosition(out position))
        {
            switch (cameradir.Cameradirection())
            {
                case "x+":
                    tg_cell = cell[tg.x, pos(position.y), pos(position.z)];
                    if (tapping)
                    {
                        if (cell[tg.x, pos(position.y), pos(position.z)].life != chenge_state)
                        {
                            if (cell[tg.x, pos(position.y), pos(position.z)].life)
                            {
                                Destroy(cell[tg.x, pos(position.y), pos(position.z)].obj);
                                cell[tg.x, pos(position.y), pos(position.z)].life = false;
                                state[tg.x, pos(position.y), pos(position.z)] = false;
                                hush[tg.x, pos(position.y), pos(position.z), memory_time] = false;
                            }
                            else
                            {
                                cell[tg.x, pos(position.y), pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                                cell[tg.x, pos(position.y), pos(position.z)].obj.transform.localPosition = cell[tg.x, pos(position.y), pos(position.z)].Pos;
                                cell[tg.x, pos(position.y), pos(position.z)].life = true;
                                state[tg.x, pos(position.y), pos(position.z)] = true;
                                hush[tg.x, pos(position.y), pos(position.z), memory_time] = true;
                            }
                        }
                    }
                    Target.transform.localPosition = tg_cell.Pos;
                    break;
                case "x-":
                    tg_cell = cell[tg.x, pos(position.y), pos(position.z)];
                    if (tapping)
                    {
                        if (cell[tg.x, pos(position.y), pos(position.z)].life != chenge_state)
                        {
                            if (cell[tg.x, pos(position.y), pos(position.z)].life)
                            {
                                Destroy(cell[tg.x, pos(position.y), pos(position.z)].obj);
                                cell[tg.x, pos(position.y), pos(position.z)].life = false;
                                state[tg.x, pos(position.y), pos(position.z)] = false;
                                hush[tg.x, pos(position.y), pos(position.z), memory_time] = false;
                            }
                            else
                            {
                                cell[tg.x, pos(position.y), pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                                cell[tg.x, pos(position.y), pos(position.z)].obj.transform.localPosition = cell[tg.x, pos(position.y), pos(position.z)].Pos;
                                cell[tg.x, pos(position.y), pos(position.z)].life = true;
                                state[tg.x, pos(position.y), pos(position.z)] = true;
                                hush[tg.x, pos(position.y), pos(position.z), memory_time] = true;
                            }
                        }
                    }
                    Target.transform.localPosition = tg_cell.Pos;
                    break;
                case "y+":
                    tg_cell = cell[pos(position.x), tg.y, pos(position.z)];
                    if (tapping)
                    {
                        if (cell[pos(position.x), tg.y, pos(position.z)].life != chenge_state)
                        {
                            if (cell[pos(position.x), tg.y, pos(position.z)].life)
                            {
                                Destroy(cell[pos(position.x), tg.y, pos(position.z)].obj);
                                cell[pos(position.x), tg.y, pos(position.z)].life = false;
                                state[pos(position.x), tg.y, pos(position.z)] = false;
                                hush[pos(position.x), tg.y, pos(position.z), memory_time] = false;
                            }
                            else
                            {
                                cell[pos(position.x), tg.y, pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                                cell[pos(position.x), tg.y, pos(position.z)].obj.transform.localPosition = cell[pos(position.x), tg.y, pos(position.z)].Pos;
                                cell[pos(position.x), tg.y, pos(position.z)].life = true;
                                state[pos(position.x), tg.y, pos(position.z)] = true;
                                hush[pos(position.x), tg.y, pos(position.z), memory_time] = true;
                            }
                        }
                    }
                    Target.transform.localPosition = tg_cell.Pos;
                    break;
                case "y-":
                    tg_cell = cell[pos(position.x), tg.y, pos(position.z)];
                    if (tapping)
                    {
                        if (cell[pos(position.x), tg.y, pos(position.z)].life != chenge_state)
                        {
                            if (cell[pos(position.x), tg.y, pos(position.z)].life)
                            {
                                Destroy(cell[pos(position.x), tg.y, pos(position.z)].obj);
                                cell[pos(position.x), tg.y, pos(position.z)].life = false;
                                state[pos(position.x), tg.y, pos(position.z)] = false;
                                hush[pos(position.x), tg.y, pos(position.z), memory_time] = false;
                            }
                            else
                            {
                                cell[pos(position.x), tg.y, pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                                cell[pos(position.x), tg.y, pos(position.z)].obj.transform.localPosition = cell[pos(position.x), tg.y, pos(position.z)].Pos;
                                cell[pos(position.x), tg.y, pos(position.z)].life = true;
                                state[pos(position.x), tg.y, pos(position.z)] = true;
                                hush[pos(position.x), tg.y, pos(position.z), memory_time] = true;
                            }
                        }
                    }
                    Target.transform.localPosition = tg_cell.Pos;
                    break;
                case "z+":
                    tg_cell = cell[pos(position.x), pos(position.y), tg.z];
                    if (tapping)
                    {
                        if (cell[pos(position.x), pos(position.y), tg.z].life != chenge_state)
                        {
                            if (cell[pos(position.x), pos(position.y), tg.z].life)
                            {
                                Destroy(cell[pos(position.x), pos(position.y), tg.z].obj);
                                cell[pos(position.x), pos(position.y), tg.z].life = false;
                                state[pos(position.x), pos(position.y), tg.z] = false;
                                hush[pos(position.x), pos(position.y), tg.z, memory_time] = false;
                            }
                            else
                            {
                                cell[pos(position.x), pos(position.y), tg.z].obj = Instantiate(CellPrefab) as GameObject;
                                cell[pos(position.x), pos(position.y), tg.z].obj.transform.localPosition = cell[pos(position.x), pos(position.y), tg.z].Pos;
                                cell[pos(position.x), pos(position.y), tg.z].life = true;
                                state[pos(position.x), pos(position.y), tg.z] = true;
                                hush[pos(position.x), pos(position.y), tg.z, memory_time] = true;
                            }
                        }
                    }
                    Target.transform.localPosition = tg_cell.Pos;
                    break;
                case "z-":
                    tg_cell = cell[pos(position.x), pos(position.y), tg.z];
                    if (tapping)
                    {
                        if (cell[pos(position.x), pos(position.y), tg.z].life != chenge_state)
                        {
                            if (cell[pos(position.x), pos(position.y), tg.z].life)
                            {
                                Destroy(cell[pos(position.x), pos(position.y), tg.z].obj);
                                cell[pos(position.x), pos(position.y), tg.z].life = false;
                                state[pos(position.x), pos(position.y), tg.z] = false;
                                hush[pos(position.x), pos(position.y), tg.z, memory_time] = false;
                            }
                            else
                            {
                                cell[pos(position.x), pos(position.y), tg.z].obj = Instantiate(CellPrefab) as GameObject;
                                cell[pos(position.x), pos(position.y), tg.z].obj.transform.localPosition = cell[pos(position.x), pos(position.y), tg.z].Pos;
                                cell[pos(position.x), pos(position.y), tg.z].life = true;
                                state[pos(position.x), pos(position.y), tg.z] = true;
                                hush[pos(position.x), pos(position.y), tg.z, memory_time] = true;
                            }
                        }
                    }
                    Target.transform.localPosition = tg_cell.Pos;
                    break;
            }
            pre_position = position;
        }

    }

    // air-tap の指を下ろした時に呼ばれる
    private void InteractionSourcePressed(InteractionSourcePressedEventArgs ev)
    {
        if (ev.state.sourcePose.TryGetPosition(out position))
        {
            switch (cameradir.Cameradirection())
            {
                case "x+":
                    if (cell[tg.x, pos(position.y), pos(position.z)].life)
                    {
                        Destroy(cell[tg.x, pos(position.y), pos(position.z)].obj);
                        cell[tg.x, pos(position.y), pos(position.z)].life = false;
                        state[tg.x, pos(position.y), pos(position.z)] = false;
                        hush[tg.x, pos(position.y), pos(position.z), memory_time] = false;
                        //chenge_state = false;
                    }
                    else
                    {
                        cell[tg.x, pos(position.y), pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                        cell[tg.x, pos(position.y), pos(position.z)].obj.transform.localPosition = cell[tg.x, pos(position.y), pos(position.z)].Pos;
                        cell[tg.x, pos(position.y), pos(position.z)].life = true;
                        state[tg.x, pos(position.y), pos(position.z)] = true;
                        hush[tg.x, pos(position.y), pos(position.z), memory_time] = true;
                        //chenge_state = true;
                    }
                    break;
                case "x-":
                    if (cell[tg.x, pos(position.y), pos(position.z)].life)
                    {
                        Destroy(cell[tg.x, pos(position.y), pos(position.z)].obj);
                        cell[tg.x, pos(position.y), pos(position.z)].life = false;
                        state[tg.x, pos(position.y), pos(position.z)] = false;
                        hush[tg.x, pos(position.y), pos(position.z), memory_time] = false;
                        //chenge_state = false;
                    }
                    else
                    {
                        cell[tg.x, pos(position.y), pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                        cell[tg.x, pos(position.y), pos(position.z)].obj.transform.localPosition = cell[tg.x, pos(position.y), pos(position.z)].Pos;
                        cell[tg.x, pos(position.y), pos(position.z)].life = true;
                        state[tg.x, pos(position.y), pos(position.z)] = true;
                        hush[tg.x, pos(position.y), pos(position.z), memory_time] = true;
                        //chenge_state = true;
                    }
                    break;
                case "y+":
                    if (cell[pos(position.x), tg.y, pos(position.z)].life)
                    {
                        Destroy(cell[pos(position.x), tg.y, pos(position.z)].obj);
                        cell[pos(position.x), tg.y, pos(position.z)].life = false;
                        state[pos(position.x), tg.y, pos(position.z)] = false;
                        hush[pos(position.x), tg.y, pos(position.z), memory_time] = false;
                        //chenge_state = false;
                    }
                    else
                    {
                        cell[pos(position.x), tg.y, pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                        cell[pos(position.x), tg.y, pos(position.z)].obj.transform.localPosition = cell[pos(position.x), tg.y, pos(position.z)].Pos;
                        cell[pos(position.x), tg.y, pos(position.z)].life = true;
                        state[pos(position.x), tg.y, pos(position.z)] = true;
                        hush[pos(position.x), tg.y, pos(position.z), memory_time] = true;
                        //chenge_state = true;
                    }
                    break;
                case "y-":
                    if (cell[pos(position.x), tg.y, pos(position.z)].life)
                    {
                        Destroy(cell[pos(position.x), tg.y, pos(position.z)].obj);
                        cell[pos(position.x), tg.y, pos(position.z)].life = false;
                        state[pos(position.x), tg.y, pos(position.z)] = false;
                        hush[pos(position.x), tg.y, pos(position.z), memory_time] = false;
                        //chenge_state = false;
                    }
                    else
                    {
                        cell[pos(position.x), tg.y, pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                        cell[pos(position.x), tg.y, pos(position.z)].obj.transform.localPosition = cell[pos(position.x), tg.y, pos(position.z)].Pos;
                        cell[pos(position.x), tg.y, pos(position.z)].life = true;
                        state[pos(position.x), tg.y, pos(position.z)] = true;
                        hush[pos(position.x), tg.y, pos(position.z), memory_time] = true;
                        //chenge_state = true;
                    }
                    break;
                case "z+":
                    if (cell[pos(position.x), pos(position.y), tg.z].life)
                    {
                        Destroy(cell[pos(position.x), pos(position.y), tg.z].obj);
                        cell[pos(position.x), pos(position.y), tg.z].life = false;
                        state[pos(position.x), pos(position.y), tg.z] = false;
                        hush[pos(position.x), pos(position.y), tg.z, memory_time] = false;
                        //chenge_state = false;
                    }
                    else
                    {
                        cell[pos(position.x), pos(position.y), tg.z].obj = Instantiate(CellPrefab) as GameObject;
                        cell[pos(position.x), pos(position.y), tg.z].obj.transform.localPosition = cell[pos(position.x), pos(position.y), tg.z].Pos;
                        cell[pos(position.x), pos(position.y), tg.z].life = true;
                        state[pos(position.x), pos(position.y), tg.z] = true;
                        hush[pos(position.x), pos(position.y), tg.z, memory_time] = true;
                        //chenge_state = true;
                    }
                    break;
                case "z-":
                    if (cell[pos(position.x), pos(position.y), tg.z].life)
                    {
                        Destroy(cell[pos(position.x), pos(position.y), tg.z].obj);
                        cell[pos(position.x), pos(position.y), tg.z].life = false;
                        state[pos(position.x), pos(position.y), tg.z] = false;
                        hush[pos(position.x), pos(position.y), tg.z, memory_time] = false;
                        //chenge_state = false;
                    }
                    else
                    {
                        cell[pos(position.x), pos(position.y), tg.z].obj = Instantiate(CellPrefab) as GameObject;
                        cell[pos(position.x), pos(position.y), tg.z].obj.transform.localPosition = cell[pos(position.x), pos(position.y), tg.z].Pos;
                        cell[pos(position.x), pos(position.y), tg.z].life = true;
                        state[pos(position.x), pos(position.y), tg.z] = true;
                        hush[pos(position.x), pos(position.y), tg.z, memory_time] = true;
                        //chenge_state = true;
                    }
                    break;
            }
            tapping = true;
        }
    }


    // air-tap の指を上げた時に呼ばれる

    private void InteractionSourceReleased(InteractionSourceReleasedEventArgs ev)
    {
        tapping = false;
    }
    /*
    // 手が視界から外れた時に呼ばれる
    private void InteractionSourceLost(InteractionSourceLostEventArgs ev)
    {
        
    }
    */
}
