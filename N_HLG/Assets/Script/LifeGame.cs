using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using CI.WSANative.Pickers;

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
    public UnityEngine.UI.Text ab;

    private const float CELL_SIZE = 0.015f;

    public GameObject CellPrefab;
    public Material Cell_01;
    public Material Cell_02;

    public GameObject TargetPrefab;
    public GameObject Target;
    public Material Cell_edit;
    public Material Cell_box;
    //public Material Cell_copy;

    public GameObject Box_Start;

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

    public int mood = 0;

    public bool[,,] state;

    public bool[,,] state2;

    public bool[,,] able;

    public bool[,,,] hush;

    public bool chenge_state;

    public bool box = false;
    public bool[,,] copy;

    public bool edit_state = true;
    public bool change_depth = true;

    public bool tapping = false;
    public bool boxing = false;
    public bool copying = false;

    public bool active_Wall = true;

    public Vector3Int tg;

    public Vector3Int Start_pos;
    public Vector3Int End_pos;
    public Vector3Int Now_pos;

    public Vector3 Wall_Xpos;
    public Vector3 Wall_Ypos;
    public Vector3 Wall_Zpos;

    public int block;

    public int time = 0;

    public int range;

    public int memory_time = 0;

    public int[] rule;

    public int count;

    public int resent_memory = 0;

    public float h1;
    public float v1;

    public float scroll;

    public Cameradir cameradir;

    public Vector3 position;
    public Vector3 pre_position;

    string fileString;
    string direction;

    // Use this for initialization
    void Start()
    {

        InteractionManager.InteractionSourceUpdated += InteractionSourceUpdated;
        InteractionManager.InteractionSourcePressed += InteractionSourcePressed;
        InteractionManager.InteractionSourceReleased += InteractionSourceReleased;


        rule = new int[] { 4, 102, 133, 102, 142 };

        range = rule[0] * 2 + 1;
        bugs = new Pattern();
        tg = new Vector3Int(0, 0, 0);
        block = gridSize / range;

        Now_pos = new Vector3Int(0, 0, 0);

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

        Wall_X.SetActive(false);
        Wall_Y.SetActive(false);
        Wall_Z.SetActive(true);

        ab.text = "[" + rule[0].ToString() + "," + rule[1].ToString() + "," + rule[2].ToString() + "," + rule[3].ToString() + "," + rule[4].ToString() + "]";

        cameradir = new Cameradir();
        direction = cameradir.Cameradirection();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            //cell_up();
            Scan();
            Upcell();
            time += 1;
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button9))
        {
            if (mood == 1)
            {
                if (boxing)
                {
                    boxing = false;
                    Destroy(Box_Start);
                } else
                {
                    chenge_state = !chenge_state;
                }            
            } else
            {
                chenge_state = !chenge_state;
            }            
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button6) && active_Wall)
        {
            switch (direction)
            {
                case "x+":
                    if (tg.x < gridSize - 1)
                    {
                        Wall_X.transform.position += new Vector3(CELL_SIZE, 0, 0);
                        for (int w_y = 0; w_y < gridSize; w_y++)
                        {
                            for (int w_z = 0; w_z < gridSize; w_z++)
                            {
                                if (cell[tg.x, w_y, w_z].life)
                                {
                                    cell[tg.x, w_y, w_z].obj.GetComponent<Renderer>().material = Cell_02;
                                }
                            }
                        }
                        tg.x++;
                    }
                    break;
                case "x-":
                    if (tg.x > 0)
                    {
                        Wall_X.transform.position -= new Vector3(CELL_SIZE, 0, 0);
                        for (int w_y = 0; w_y < gridSize; w_y++)
                        {
                            for (int w_z = 0; w_z < gridSize; w_z++)
                            {
                                if (cell[tg.x, w_y, w_z].life)
                                {
                                    cell[tg.x, w_y, w_z].obj.GetComponent<Renderer>().material = Cell_02;
                                }
                            }
                        }
                        tg.x--;
                    }
                    break;
                case "y+":
                    if (tg.y < gridSize - 1)
                    {
                        Wall_Y.transform.position += new Vector3(0, CELL_SIZE, 0);
                        for (int w_x = 0; w_x < gridSize; w_x++)
                        {
                            for (int w_z = 0; w_z < gridSize; w_z++)
                            {
                                if (cell[w_x, tg.y, w_z].life)
                                {
                                    cell[w_x, tg.y, w_z].obj.GetComponent<Renderer>().material = Cell_02;
                                }
                            }
                        }
                        tg.y++;
                    }
                    break;
                case "y-":
                    if (tg.y > 0)
                    {
                        Wall_Y.transform.position -= new Vector3(0, CELL_SIZE, 0);
                        for (int w_x = 0; w_x < gridSize; w_x++)
                        {
                            for (int w_z = 0; w_z < gridSize; w_z++)
                            {
                                if (cell[w_x, tg.y, w_z].life)
                                {
                                    cell[w_x, tg.y, w_z].obj.GetComponent<Renderer>().material = Cell_02;
                                }
                            }
                        }
                        tg.y--;
                    }
                    break;
                case "z+":
                    if (tg.z < gridSize - 1)
                    {
                        Wall_Z.transform.position += new Vector3(0, 0, CELL_SIZE);
                        for (int w_y = 0; w_y < gridSize; w_y++)
                        {
                            for (int w_x = 0; w_x < gridSize; w_x++)
                            {
                                if (cell[w_x, w_y, tg.z].life)
                                {
                                    cell[w_x, w_y, tg.z].obj.GetComponent<Renderer>().material = Cell_02;
                                }
                            }
                        }
                        tg.z++;
                    }
                    break;
                case "z-":
                    if (tg.z > 0)
                    {
                        Wall_Z.transform.position -= new Vector3(0, 0, CELL_SIZE);
                        for (int w_y = 0; w_y < gridSize; w_y++)
                        {
                            for (int w_x = 0; w_x < gridSize; w_x++)
                            {
                                if (cell[w_x, w_y, tg.z].life)
                                {
                                    cell[w_x, w_y, tg.z].obj.GetComponent<Renderer>().material = Cell_02;
                                }
                            }
                        }
                        tg.z--;
                    }
                    break;
            }
            Tag.text = "(" + tg.x.ToString() + ", " + tg.y.ToString() + ", " + tg.z.ToString() + ")";
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button7) && active_Wall)
        {
            switch (direction)
            {
                case "x+":
                    if (tg.x > 0)
                    {
                        Wall_X.transform.position -= new Vector3(CELL_SIZE, 0, 0);
                        for (int w_y = 0; w_y < gridSize; w_y++)
                        {
                            for (int w_z = 0; w_z < gridSize; w_z++)
                            {
                                if (cell[tg.x, w_y, w_z].life)
                                {
                                    cell[tg.x, w_y, w_z].obj.GetComponent<Renderer>().material = Cell_01;
                                }
                            }
                        }
                        tg.x--;
                    }
                    break;
                case "x-":
                    if (tg.x < gridSize - 1)
                    {
                        Wall_X.transform.position += new Vector3(CELL_SIZE, 0, 0);
                        for (int w_y = 0; w_y < gridSize; w_y++)
                        {
                            for (int w_z = 0; w_z < gridSize; w_z++)
                            {
                                if (cell[tg.x, w_y, w_z].life)
                                {
                                    cell[tg.x, w_y, w_z].obj.GetComponent<Renderer>().material = Cell_01;
                                }
                            }
                        }
                        tg.x++;
                    }
                    break;
                case "y+":
                    if (tg.y > 0)
                    {
                        Wall_Y.transform.position -= new Vector3(0, CELL_SIZE, 0);
                        for (int w_x = 0; w_x < gridSize; w_x++)
                        {
                            for (int w_z = 0; w_z < gridSize; w_z++)
                            {
                                if (cell[w_x, tg.y, w_z].life)
                                {
                                    cell[w_x, tg.y, w_z].obj.GetComponent<Renderer>().material = Cell_01;
                                }
                            }
                        }
                        tg.y--;
                    }
                    break;
                case "y-":
                    if (tg.y < gridSize - 1)
                    {
                        Wall_Y.transform.position += new Vector3(0, CELL_SIZE, 0);
                        for (int w_x = 0; w_x < gridSize; w_x++)
                        {
                            for (int w_z = 0; w_z < gridSize; w_z++)
                            {
                                if (cell[w_x, tg.y, w_z].life)
                                {
                                    cell[w_x, tg.y, w_z].obj.GetComponent<Renderer>().material = Cell_01;
                                }
                            }
                        }
                        tg.y++;
                    }
                    break;
                case "z+":
                    if (tg.z > 0)
                    {
                        Wall_Z.transform.position -= new Vector3(0, 0, CELL_SIZE);
                        tg.z--;
                        for (int w_y = 0; w_y < gridSize; w_y++)
                        {
                            for (int w_x = 0; w_x < gridSize; w_x++)
                            {
                                if (cell[w_x, w_y, tg.z].life)
                                {
                                    cell[w_x, w_y, tg.z].obj.GetComponent<Renderer>().material = Cell_01;
                                }
                            }
                        }
                    }
                    break;
                case "z-":
                    if (tg.z < gridSize - 1)
                    {
                        Wall_Z.transform.position += new Vector3(0, 0, CELL_SIZE);
                        for (int w_y = 0; w_y < gridSize; w_y++)
                        {
                            for (int w_x = 0; w_x < gridSize; w_x++)
                            {
                                if (cell[w_x, w_y, tg.z].life)
                                {
                                    cell[w_x, w_y, tg.z].obj.GetComponent<Renderer>().material = Cell_01;
                                }
                            }
                        }
                        tg.z++;
                    }
                    break;
            }
            Tag.text = "(" + tg.x.ToString() + ", " + tg.y.ToString() + ", " + tg.z.ToString() + ")";
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button14))
        {
            direction = cameradir.Cameradirection();
            switch (direction)
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
            active_Wall = true;
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button8))
        {
            if (active_Wall)
            {
                Clean_cell();
            }
            else
            {
                switch (direction)
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
            }
            active_Wall = !active_Wall;
        }

        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            Reset_cell();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Birth_bug(bugs.q0, bugs.q0_x, bugs.q0_y, bugs.q0_z, new int[] { 48, 48, 48 });
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            Birth_bug(bugs.bug, bugs.bug_x, bugs.bug_y, bugs.bug_z, new int[] { 45, 63, 44 });
            Birth_bug(bugs.q1, bugs.q1_x, bugs.q1_y, bugs.q1_z, new int[] { 48, 48, 48 });
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            Birth_bug(bugs.p0, bugs.p0_x, bugs.p0_y, bugs.p0_z, new int[] { 48, 49, 48 });
            Birth_bug(bugs.p0, bugs.p0_x, bugs.p0_y, bugs.p0_z, new int[] { 48, 49, 66 });
            Birth_bug(bugs.bug, bugs.bug_x, bugs.bug_y, bugs.bug_z, new int[] { 61, 48, 58 });
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
        
        if (Input.GetKeyDown(KeyCode.Joystick1Button3))
        {
            pathting(Now_pos, copy);
        }
        
        if (Input.GetKeyDown(KeyCode.Joystick1Button5))
        {
            mood++;
            if (mood > 2)
            {
                mood = 0;
                Target.GetComponent<Renderer>().material = Cell_edit;
            } else if(mood == 1)
            {
                Target.GetComponent<Renderer>().material = Cell_box;
            }/* else if (mood == 2)
            {
                Target.GetComponent<Renderer>().material = Cell_edit;
            }*/
        }

        /*
        if (Input.GetKeyDown(KeyCode.C))
        {
            Clean_cell();
        }
        */

        if (Input.GetKeyDown(KeyCode.M))
        {
            Change_rule();            
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Scan();
            for (int i = 0; i < 10; i++)
            {
                Upcell();
                time += 1;
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            string data = "";
            for (int a = 0; a < gridSize; a++)
            {
                for (int b = 0; b < gridSize; b++)
                {
                    for (int c = 0; c < gridSize; c++)
                    {
                        if (cell[a, b, c].life)
                        {
                            data += a.ToString() + "," + b.ToString() + "," + c.ToString() + ",\n";
                        }
                    }
                }
            }
            WSANativeFilePicker.PickSaveFile("Save", ".txt", "Test Text File", WSAPickerLocationId.DocumentsLibrary, new List<KeyValuePair<string, IList<string>>>() { new KeyValuePair<string, IList<string>>("Text Files", new List<string>() { ".txt" }) }, result =>
            {
                if (result != null)
                {
                    result.WriteBytes(System.Text.Encoding.ASCII.GetBytes(data));
                    result.WriteText(data);
                }
            });

        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            WSANativeFilePicker.PickSingleFile("Select", WSAPickerViewMode.Thumbnail, WSAPickerLocationId.PicturesLibrary, new[] { ".txt" }, result =>
            {
                //string data = "";

                if (result != null)
                {
                    byte[] fileBytes = result.ReadBytes();
#pragma warning disable 0219
                    string fileString = result.ReadText();
#pragma warning restore 0219
                    string num = "";
                    int[] zahyo = new int[3];
                    int purin = 0;
                    for (int i = 0; i < fileString.Length; i++)
                    {
                        if (fileString[i] >= '0' && fileString[i] <= '9')
                        {
                            num += fileString[i];
                        }
                        else if (fileString[i] == ',')
                        {
                            zahyo[purin] = int.Parse(num);
                            purin++;
                            num = "";
                        }                        
                        else if (purin == 3)
                        {
                            cell[zahyo[0], zahyo[1], zahyo[2]].obj = Instantiate(CellPrefab) as GameObject;
                            cell[zahyo[0], zahyo[1], zahyo[2]].obj.transform.localPosition = cell[zahyo[0], zahyo[1], zahyo[2]].Pos;
                            cell[zahyo[0], zahyo[1], zahyo[2]].life = true;
                            state[zahyo[0], zahyo[1], zahyo[2]] = true;
                            hush[zahyo[0], zahyo[1], zahyo[2], memory_time] = true;
                            purin = 0;
                        }
                    }
                }                
            });

        }

        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Joystick1Button13))
        {
            if (resent_memory + 1 != memory_time)
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
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Joystick1Button12))
        {
            if (resent_memory != memory_time)
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
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            System.Random rnd = new System.Random();
            int randomNumber;
            for (int x = 25; x < 75; x++)
            {
                for (int y = 25; y < 75; y++)
                {
                    for (int z = 25; z < 75; z++)
                    {
                        randomNumber = rnd.Next(500);
                        if((randomNumber <= 1) != cell[x, y, z].life)
                        {
                            if (cell[x, y, z].life)
                            {
                                Destroy(cell[x, y, z].obj);
                                cell[x, y, z].life = false;
                                state[x, y, z] = false;
                                hush[x, y, z, memory_time] = false;
                            }
                            else
                            {
                                cell[x, y, z].obj = Instantiate(CellPrefab) as GameObject;
                                cell[x, y, z].obj.transform.localPosition = cell[x, y, z].Pos;
                                cell[x, y, z].life = true;
                                state[x, y, z] = true;
                                hush[x, y, z, memory_time] = true;
                            }
                        }
                    }
                }
            }
        }

        /*
        if (active_Wall)
        {
            switch (direction)
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
        }*/        
    }

    private IEnumerator LifeGameCoroutine()
    {
        while (true)
        {
            Scan();
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
                                    if (!able[(a + block) % block, (b + block) % block, (c + block) % block])
                                    {
                                        able[(a + block) % block, (b + block) % block, (c + block) % block] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /*
        for (int x0 = 0; x0 < block; x0++)
        {
            for (int y0 = 0; y0 < block; y0++)
            {
                for (int z0 = 0; z0 < block; z0++)
                {
                    if(able[x0, y0, z0])
                    {
                        count++;
                    }
                }
            }
        }
        ab.text = count.ToString();
        */

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
        resent_memory += 1;
        if (memory_time == 50)
        {
            memory_time = 0;
            resent_memory = 0;
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
                                    if (!cell[a, b, c].life)
                                    {
                                        if (state[a, b, c])
                                        {
                                            cell[a, b, c].obj = Instantiate(CellPrefab) as GameObject;
                                            cell[a, b, c].obj.transform.localPosition = cell[a, b, c].Pos;
                                            if (active_Wall)
                                            {
                                                switch (direction)
                                                {
                                                    case "x+":
                                                        if (x < tg.x)
                                                        {
                                                            cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                        }
                                                        break;
                                                    case "x-":
                                                        if (x > tg.x)
                                                        {
                                                            cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                        }
                                                        break;
                                                    case "y+":
                                                        if (y < tg.y)
                                                        {
                                                            cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                        }
                                                        break;
                                                    case "y-":
                                                        if (y > tg.y)
                                                        {
                                                            cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                        }
                                                        break;
                                                    case "z+":
                                                        if (z < tg.z)
                                                        {
                                                            cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                        }
                                                        break;
                                                    case "z-":
                                                        if (z > tg.z)
                                                        {
                                                            cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if(!state[a, b, c])
                                        {
                                            Destroy(cell[a, b, c].obj);
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
        count = 0;/*
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    stateChec(state, x, y, z);
                }
            }
        }*/
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
                                    if (active_Wall)
                                    {
                                        switch (direction)
                                        {
                                            case "x+":
                                                if (x < tg.x)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "x-":
                                                if (x > tg.x)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "y+":
                                                if (y < tg.y)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "y-":
                                                if (y > tg.y)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "z+":
                                                if (z < tg.z)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "z-":
                                                if (z > tg.z)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                        }
                                    }
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
                                    if (active_Wall)
                                    {
                                        switch (direction)
                                        {
                                            case "x+":
                                                if (x < tg.x)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "x-":
                                                if (x > tg.x)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "y+":
                                                if (y < tg.y)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "y-":
                                                if (y > tg.y)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "z+":
                                                if (z < tg.z)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "z-":
                                                if (z > tg.z)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                        }
                                    }
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
                                    if (active_Wall)
                                    {
                                        switch (direction)
                                        {
                                            case "x+":
                                                if (x < tg.x)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "x-":
                                                if (x > tg.x)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "y+":
                                                if (y < tg.y)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "y-":
                                                if (y > tg.y)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "z+":
                                                if (z < tg.z)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "z-":
                                                if (z > tg.z)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                        }
                                    }
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
                                    if (active_Wall)
                                    {
                                        switch (direction)
                                        {
                                            case "x+":
                                                if (x < tg.x)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "x-":
                                                if (x > tg.x)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "y+":
                                                if (y < tg.y)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "y-":
                                                if (y > tg.y)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "z+":
                                                if (z < tg.z)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                            case "z-":
                                                if (z > tg.z)
                                                {
                                                    cell[x, y, z].obj.GetComponent<Renderer>().material = Cell_02;
                                                }
                                                break;
                                        }
                                    }
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
                            if ((start[2] + z < tg.z) && active_Wall)
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
        Wall_X.SetActive(false);
        Wall_Y.SetActive(false);
        Wall_Z.SetActive(false);
        //cell[tg.x, tg.y, tg.z].obj.layer = 0;

        //Wall.SetActive(false);
        //Target.SetActive(false);

        //Wall.transform.localPosition = new Vector3(cell[gridSize - 1, 0, 0].Pos.x / 2, cell[0, gridSize - 1, 0].Pos.y / 2, cell[0, 0, 0].Pos.x + CELL_SIZE / 2);
        //Wall_pos = Wall.transform.localPosition;
        //tg.z = 0;
    }

    public int pos(float a)
    {
        if (cell[0, 0, 0].Pos.x == a)
        {
            return 0;
        }
        else
        {
            float pos = a / CELL_SIZE;
            if((int)pos >= gridSize)
            {
                return (gridSize - 1);
            } else if((int)pos < 0)
            {
                return 0;
            } else
            {
                return (int)pos;
            }
            
        }
    }

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
            switch (direction)
            {
                case "x+":
                    tg_cell = cell[tg.x, pos(position.y), pos(position.z)];
                    Now_pos = new Vector3Int (tg.x, pos(position.y), pos(position.z));
                    if (tapping)
                    {
                        if (mood == 0)
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
                    }
                    if (chenge_state && (mood == 0))
                    {
                        if ((tg_cell.life) && (tg.x != 0))
                        {
                            int j = 1;
                            while (true)
                            {
                                if (cell[tg.x - j, pos(position.y), pos(position.z)].life)
                                {
                                    if (tg.x - j > 0)
                                    {
                                        j++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    Target.transform.localPosition = cell[tg.x - j, pos(position.y), pos(position.z)].Pos;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Target.transform.localPosition = tg_cell.Pos;
                        }
                    }
                    else
                    {
                        Target.transform.localPosition = tg_cell.Pos;
                    }
                    cell[tg.x, pos(position.y), pos(position.z)] = tg_cell;
                    break;
                case "x-":
                    tg_cell = cell[tg.x, pos(position.y), pos(position.z)];
                    Now_pos = new Vector3Int(tg.x, pos(position.y), pos(position.z));
                    if (tapping)
                    {
                        if (mood == 0)
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
                    }
                    if (chenge_state && (mood == 0))
                    {
                        if (tg_cell.life && (tg.x != gridSize -1))
                        {
                            int j = 1;
                            while (true)
                            {
                                if (cell[tg.x + j, pos(position.y), pos(position.z)].life)
                                {
                                    if (tg.x + j < gridSize - 1)
                                    {
                                        j++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    Target.transform.localPosition = cell[tg.x + j, pos(position.y), pos(position.z)].Pos;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Target.transform.localPosition = tg_cell.Pos;
                        }
                    }
                    else
                    {
                        Target.transform.localPosition = tg_cell.Pos;
                    }
                    cell[tg.x, pos(position.y), pos(position.z)] = tg_cell;
                    break;
                case "y+":
                    tg_cell = cell[pos(position.x), tg.y, pos(position.z)];
                    Now_pos = new Vector3Int(pos(position.x), tg.y, pos(position.z));
                    if (tapping)
                    {
                        if (mood == 0)
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
                    }
                    if (chenge_state && (mood == 0))
                    {
                        if (tg_cell.life && (tg.y != 0))
                        {
                            int j = 1;
                            while (true)
                            {
                                if (cell[pos(position.x), tg.y - j, pos(position.z)].life)
                                {
                                    if (tg.y - j > 0)
                                    {
                                        j++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    Target.transform.localPosition = cell[pos(position.x), tg.y - j, pos(position.z)].Pos;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Target.transform.localPosition = tg_cell.Pos;
                        }
                    }
                    else
                    {
                        Target.transform.localPosition = tg_cell.Pos;
                    }
                    cell[pos(position.x), tg.y, pos(position.z)] = tg_cell;
                    break;
                case "y-":
                    tg_cell = cell[pos(position.x), tg.y, pos(position.z)];
                    Now_pos = new Vector3Int(pos(position.x), tg.y, pos(position.z));
                    if (tapping)
                    {
                        if (mood == 0)
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
                    }
                    if (chenge_state && (mood == 0))
                    {
                        if (tg_cell.life && (tg.y != gridSize-1))
                        {
                            int j = 1;
                            while (true)
                            {
                                if (cell[pos(position.x), tg.y + j, pos(position.z)].life)
                                {
                                    if (tg.y + j > gridSize - 1)
                                    {
                                        j++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    Target.transform.localPosition = cell[pos(position.x), tg.y + j, pos(position.z)].Pos;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Target.transform.localPosition = tg_cell.Pos;
                        }
                    }
                    else
                    {
                        Target.transform.localPosition = tg_cell.Pos;
                    }
                    cell[pos(position.x), tg.y, pos(position.z)] = tg_cell;
                    break;
                case "z+":
                    tg_cell = cell[pos(position.x), pos(position.y), tg.z];
                    Now_pos = new Vector3Int(pos(position.x), pos(position.y), tg.z);
                    if (tapping)
                    {
                        if(mood == 0)
                        {
                            if (tg_cell.life != chenge_state)
                            {
                                if (tg_cell.life)
                                {
                                    Destroy(tg_cell.obj);
                                    tg_cell.life = false;
                                    state[pos(position.x), pos(position.y), tg.z] = false;
                                    hush[pos(position.x), pos(position.y), tg.z, memory_time] = false;
                                }
                                else
                                {
                                    tg_cell.obj = Instantiate(CellPrefab) as GameObject;
                                    tg_cell.obj.transform.localPosition = tg_cell.Pos;
                                    tg_cell.life = true;
                                    state[pos(position.x), pos(position.y), tg.z] = true;
                                    hush[pos(position.x), pos(position.y), tg.z, memory_time] = true;
                                }
                            }
                        }
                    }
                    if (chenge_state && (mood == 0))
                    {
                        if (tg_cell.life && (tg.z != 0))
                        {
                            int j = 1;
                            while (true)
                            {
                                if (cell[pos(position.x), pos(position.y), tg.z - j].life)
                                {
                                    if (tg.z - j > 0)
                                    {
                                        j++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    Target.transform.localPosition = cell[pos(position.x), pos(position.y), tg.z - j].Pos;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Target.transform.localPosition = tg_cell.Pos;
                        }
                    }
                    else
                    {
                        Target.transform.localPosition = tg_cell.Pos;
                    }
                    cell[pos(position.x), pos(position.y), tg.z] = tg_cell;
                    break;
                case "z-":
                    tg_cell = cell[pos(position.x), pos(position.y), tg.z];
                    Now_pos = new Vector3Int(pos(position.x), pos(position.y), tg.z);
                    if (tapping)
                    {
                        if(mood == 0)
                        {
                            if (tg_cell.life != chenge_state)
                            {
                                if (tg_cell.life)
                                {
                                    Destroy(tg_cell.obj);
                                    tg_cell.life = false;
                                    state[pos(position.x), pos(position.y), tg.z] = false;
                                    hush[pos(position.x), pos(position.y), tg.z, memory_time] = false;
                                }
                                else
                                {
                                    tg_cell.obj = Instantiate(CellPrefab) as GameObject;
                                    tg_cell.obj.transform.localPosition = tg_cell.Pos;
                                    tg_cell.life = true;
                                    state[pos(position.x), pos(position.y), tg.z] = true;
                                    hush[pos(position.x), pos(position.y), tg.z, memory_time] = true;
                                }
                            }
                        }
                    }
                    if (chenge_state && (mood == 0))
                    {
                        if (tg_cell.life && (tg.z != gridSize -1))
                        {
                            int j = 1;
                            while (true)
                            {
                                if (cell[pos(position.x), pos(position.y), tg.z + j].life)
                                {
                                    if (tg.z + j < gridSize - 1)
                                    {
                                        j++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    Target.transform.localPosition = cell[pos(position.x), pos(position.y), tg.z + j].Pos;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Target.transform.localPosition = tg_cell.Pos;
                        }
                    }
                    else
                    {
                        Target.transform.localPosition = tg_cell.Pos;
                    }
                    cell[pos(position.x), pos(position.y), tg.z] = tg_cell;
                    break;
            }
            if (mood == 0)
            {

            } else if(mood == 1)
            {

            }
            pre_position = position;
        }
    }

    // air-tap の指を下ろした時に呼ばれる
    private void InteractionSourcePressed(InteractionSourcePressedEventArgs ev)
    {
        if (ev.state.sourcePose.TryGetPosition(out position))
        {
            switch (direction)
            {
                case "x+":
                    if(mood == 0)
                    {
                        if (!chenge_state)
                        {
                            if (cell[tg.x, pos(position.y), pos(position.z)].life)
                            {
                                Destroy(cell[tg.x, pos(position.y), pos(position.z)].obj);
                                cell[tg.x, pos(position.y), pos(position.z)].life = false;
                                state[tg.x, pos(position.y), pos(position.z)] = false;
                                hush[tg.x, pos(position.y), pos(position.z), memory_time] = false;
                                //chenge_state = false;
                            }
                        }
                        else
                        {
                            if (!cell[tg.x, pos(position.y), pos(position.z)].life)
                            {
                                cell[tg.x, pos(position.y), pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                                cell[tg.x, pos(position.y), pos(position.z)].obj.transform.localPosition = cell[tg.x, pos(position.y), pos(position.z)].Pos;
                                cell[tg.x, pos(position.y), pos(position.z)].life = true;
                                state[tg.x, pos(position.y), pos(position.z)] = true;
                                hush[tg.x, pos(position.y), pos(position.z), memory_time] = true;
                                //chenge_state = true;
                            }
                            else
                            {
                                int i = 1;
                                while (true)
                                {
                                    if (cell[tg.x - i, pos(position.y), pos(position.z)].life)
                                    {
                                        if (tg.x - i > 0)
                                        {
                                            i++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        cell[tg.x - i, pos(position.y), pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                                        cell[tg.x - i, pos(position.y), pos(position.z)].obj.transform.localPosition = cell[tg.x - i, pos(position.y), pos(position.z)].Pos;
                                        cell[tg.x - i, pos(position.y), pos(position.z)].life = true;
                                        state[tg.x - i, pos(position.y), pos(position.z)] = true;
                                        hush[tg.x - i, pos(position.y), pos(position.z), memory_time] = true;
                                        //chenge_state = true;
                                        break;
                                    }
                                }
                            }
                        }
                    } else if(mood == 1)
                    {
                        if (boxing)
                        {
                            End_pos = new Vector3Int(tg.x, pos(position.y), pos(position.z));
                            Destroy(Box_Start);
                            Boxing(Start_pos, End_pos);
                            boxing = false;
                        }
                        else
                        {
                            Start_pos = new Vector3Int(tg.x, pos(position.y), pos(position.z));
                            Box_Start = Instantiate(TargetPrefab) as GameObject;
                            Box_Start.GetComponent<Renderer>().material = Cell_box;
                            Box_Start.transform.localPosition = cell[Start_pos.x, Start_pos.y, Start_pos.z].Pos;
                            boxing = true;
                        }
                    } else if(mood == 2)
                    {
                        if (copying)
                        {
                            End_pos = new Vector3Int(tg.x, pos(position.y), pos(position.z));
                            copy = Coping(Start_pos, Now_pos);
                            copying = false;
                        }
                        else
                        {
                            Start_pos = new Vector3Int(tg.x, pos(position.y), pos(position.z));
                            copying = true;
                        }
                    }
                    break;
                case "x-":
                    if (mood == 0)
                    {
                        if (!chenge_state)
                        {
                            if (cell[tg.x, pos(position.y), pos(position.z)].life)
                            {
                                Destroy(cell[tg.x, pos(position.y), pos(position.z)].obj);
                                cell[tg.x, pos(position.y), pos(position.z)].life = false;
                                state[tg.x, pos(position.y), pos(position.z)] = false;
                                hush[tg.x, pos(position.y), pos(position.z), memory_time] = false;
                                //chenge_state = false;
                            }
                        }
                        else
                        {
                            if (!cell[tg.x, pos(position.y), pos(position.z)].life)
                            {
                                cell[tg.x, pos(position.y), pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                                cell[tg.x, pos(position.y), pos(position.z)].obj.transform.localPosition = cell[tg.x, pos(position.y), pos(position.z)].Pos;
                                cell[tg.x, pos(position.y), pos(position.z)].life = true;
                                state[tg.x, pos(position.y), pos(position.z)] = true;
                                hush[tg.x, pos(position.y), pos(position.z), memory_time] = true;
                                //chenge_state = true;
                            }
                            else
                            {
                                int i = 1;
                                while (true)
                                {
                                    if (cell[tg.x + i, pos(position.y), pos(position.z)].life)
                                    {
                                        if (tg.x + i < gridSize - 1)
                                        {
                                            i++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        cell[tg.x + i, pos(position.y), pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                                        cell[tg.x + i, pos(position.y), pos(position.z)].obj.transform.localPosition = cell[tg.x + i, pos(position.y), pos(position.z)].Pos;
                                        cell[tg.x + i, pos(position.y), pos(position.z)].life = true;
                                        state[tg.x + i, pos(position.y), pos(position.z)] = true;
                                        hush[tg.x + i, pos(position.y), pos(position.z), memory_time] = true;
                                        //chenge_state = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (mood == 1)
                    {
                        if (boxing)
                        {
                            End_pos = new Vector3Int(tg.x, pos(position.y), pos(position.z));
                            Destroy(Box_Start);
                            Boxing(Start_pos, End_pos);
                            boxing = false;
                        }
                        else
                        {
                            Start_pos = new Vector3Int(tg.x, pos(position.y), pos(position.z));
                            Box_Start = Instantiate(TargetPrefab) as GameObject;
                            Box_Start.GetComponent<Renderer>().material = Cell_box;
                            Box_Start.transform.localPosition = cell[Start_pos.x, Start_pos.y, Start_pos.z].Pos;
                            boxing = true;
                        }
                    }
                    else if (mood == 2)
                    {
                        if (copying)
                        {
                            End_pos = new Vector3Int(tg.x, pos(position.y), pos(position.z));
                            copy = Coping(Start_pos, Now_pos);
                            copying = false;
                        }
                        else
                        {
                            Start_pos = new Vector3Int(tg.x, pos(position.y), pos(position.z));
                            copying = true;
                        }
                    }
                    break;
                case "y+":
                    if (mood == 0)
                    {
                        if (!chenge_state)
                        {
                            if (cell[pos(position.x), tg.y, pos(position.z)].life)
                            {
                                Destroy(cell[pos(position.x), tg.y, pos(position.z)].obj);
                                cell[pos(position.x), tg.y, pos(position.z)].life = false;
                                state[pos(position.x), tg.y, pos(position.z)] = false;
                                hush[pos(position.x), tg.y, pos(position.z), memory_time] = false;
                                //chenge_state = false;
                            }
                        }
                        else
                        {
                            if (!cell[pos(position.x), tg.y, pos(position.z)].life)
                            {
                                cell[pos(position.x), tg.y, pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                                cell[pos(position.x), tg.y, pos(position.z)].obj.transform.localPosition = cell[pos(position.x), tg.y, pos(position.z)].Pos;
                                cell[pos(position.x), tg.y, pos(position.z)].life = true;
                                state[pos(position.x), tg.y, pos(position.z)] = true;
                                hush[pos(position.x), tg.y, pos(position.z), memory_time] = true;
                                //chenge_state = true;
                            }
                            else
                            {
                                int i = 1;
                                while (true)
                                {
                                    if (cell[pos(position.x), tg.y - i, pos(position.z)].life)
                                    {
                                        if (tg.y - i > 0)
                                        {
                                            i++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        cell[pos(position.x), tg.y - i, pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                                        cell[pos(position.x), tg.y - i, pos(position.z)].obj.transform.localPosition = cell[pos(position.x), tg.y - i, pos(position.z)].Pos;
                                        cell[pos(position.x), tg.y - i, pos(position.z)].life = true;
                                        state[pos(position.x), tg.y - i, pos(position.z)] = true;
                                        hush[pos(position.x), tg.y - i, pos(position.z), memory_time] = true;
                                        //chenge_state = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (mood == 1)
                    {
                        if (boxing)
                        {
                            End_pos = new Vector3Int(pos(position.x), tg.y, pos(position.z));
                            Destroy(Box_Start);
                            Boxing(Start_pos, End_pos);
                            boxing = false;
                        }
                        else
                        {
                            Start_pos = new Vector3Int(pos(position.x), tg.y, pos(position.z));
                            Box_Start.GetComponent<Renderer>().material = Cell_box;
                            Box_Start = Instantiate(TargetPrefab) as GameObject;
                            Box_Start.transform.localPosition = cell[Start_pos.x, Start_pos.y, Start_pos.z].Pos;
                            boxing = true;
                        }
                    }
                    else if (mood == 2)
                    {
                        if (copying)
                        {
                            End_pos = new Vector3Int(pos(position.x), tg.y, pos(position.z));
                            copy = Coping(Start_pos, Now_pos);
                            copying = false;
                        }
                        else
                        {
                            Start_pos = new Vector3Int(pos(position.x), tg.y, pos(position.z));
                            copying = true;
                        }
                    }
                    break;
                case "y-":
                    if (mood == 0)
                    {
                        if (!chenge_state)
                        {
                            if (cell[pos(position.x), tg.y, pos(position.z)].life)
                            {
                                Destroy(cell[pos(position.x), tg.y, pos(position.z)].obj);
                                cell[pos(position.x), tg.y, pos(position.z)].life = false;
                                state[pos(position.x), tg.y, pos(position.z)] = false;
                                hush[pos(position.x), tg.y, pos(position.z), memory_time] = false;
                                //chenge_state = false;
                            }
                        }
                        else
                        {
                            if (!cell[pos(position.x), tg.y, pos(position.z)].life)
                            {
                                cell[pos(position.x), tg.y, pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                                cell[pos(position.x), tg.y, pos(position.z)].obj.transform.localPosition = cell[pos(position.x), tg.y, pos(position.z)].Pos;
                                cell[pos(position.x), tg.y, pos(position.z)].life = true;
                                state[pos(position.x), tg.y, pos(position.z)] = true;
                                hush[pos(position.x), tg.y, pos(position.z), memory_time] = true;
                                //chenge_state = true;
                            }
                            else
                            {
                                int i = 1;
                                while (true)
                                {
                                    if (cell[pos(position.x), tg.y + i, pos(position.z)].life)
                                    {
                                        if (tg.y + i < gridSize - 1)
                                        {
                                            i++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        cell[pos(position.x), tg.y + i, pos(position.z)].obj = Instantiate(CellPrefab) as GameObject;
                                        cell[pos(position.x), tg.y + i, pos(position.z)].obj.transform.localPosition = cell[pos(position.x), tg.y + i, pos(position.z)].Pos;
                                        cell[pos(position.x), tg.y + i, pos(position.z)].life = true;
                                        state[pos(position.x), tg.y + i, pos(position.z)] = true;
                                        hush[pos(position.x), tg.y + i, pos(position.z), memory_time] = true;
                                        //chenge_state = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (mood == 1)
                    {
                        if (boxing)
                        {
                            End_pos = new Vector3Int(pos(position.x), tg.y, pos(position.z));
                            Destroy(Box_Start);
                            Boxing(Start_pos, End_pos);
                            boxing = false;
                        }
                        else
                        {
                            Start_pos = new Vector3Int(pos(position.x), tg.y, pos(position.z));
                            Box_Start = Instantiate(TargetPrefab) as GameObject;
                            Box_Start.GetComponent<Renderer>().material = Cell_box;
                            Box_Start.transform.localPosition = cell[Start_pos.x, Start_pos.y, Start_pos.z].Pos;
                            boxing = true;
                        }
                    }
                    else if (mood == 2)
                    {
                        if (copying)
                        {
                            End_pos = new Vector3Int(pos(position.x), tg.y, pos(position.z));
                            copy = Coping(Start_pos, Now_pos);
                            copying = false;
                        }
                        else
                        {
                            Start_pos = new Vector3Int(pos(position.x), tg.y, pos(position.z));
                            copying = true;
                        }
                    }
                    break;
                case "z+":
                    if (mood == 0)
                    {
                        if (!chenge_state)
                        {
                            if (cell[pos(position.x), pos(position.y), tg.z].life)
                            {
                                Destroy(cell[pos(position.x), pos(position.y), tg.z].obj);
                                cell[pos(position.x), pos(position.y), tg.z].life = false;
                                state[pos(position.x), pos(position.y), tg.z] = false;
                                hush[pos(position.x), pos(position.y), tg.z, memory_time] = false;
                                //chenge_state = false;
                            }
                        }
                        else
                        {
                            if (!cell[pos(position.x), pos(position.y), tg.z].life)
                            {
                                cell[pos(position.x), pos(position.y), tg.z].obj = Instantiate(CellPrefab) as GameObject;
                                cell[pos(position.x), pos(position.y), tg.z].obj.transform.localPosition = cell[pos(position.x), pos(position.y), tg.z].Pos;
                                cell[pos(position.x), pos(position.y), tg.z].life = true;
                                state[pos(position.x), pos(position.y), tg.z] = true;
                                hush[pos(position.x), pos(position.y), tg.z, memory_time] = true;
                                //chenge_state = true;
                            }
                            else
                            {
                                int i = 1;
                                while (true)
                                {
                                    if (cell[pos(position.x), pos(position.y), tg.z - i].life)
                                    {
                                        if (tg.z - i > 0)
                                        {
                                            i++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        cell[pos(position.x), pos(position.y), tg.z - i].obj = Instantiate(CellPrefab) as GameObject;
                                        cell[pos(position.x), pos(position.y), tg.z - i].obj.transform.localPosition = cell[pos(position.x), pos(position.y), tg.z - i].Pos;
                                        cell[pos(position.x), pos(position.y), tg.z - i].life = true;
                                        state[pos(position.x), pos(position.y), tg.z - i] = true;
                                        hush[pos(position.x), pos(position.y), tg.z - i, memory_time] = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (mood == 1)
                    {
                        if (boxing)
                        {
                            End_pos = new Vector3Int(pos(position.x), pos(position.y), tg.z);
                            Destroy(Box_Start);
                            Boxing(Start_pos, End_pos);
                            boxing = false;
                        }
                        else
                        {
                            Start_pos = new Vector3Int(pos(position.x), pos(position.y), tg.z);
                            Box_Start = Instantiate(TargetPrefab) as GameObject;
                            Box_Start.GetComponent<Renderer>().material = Cell_box;
                            Box_Start.transform.localPosition = cell[Start_pos.x, Start_pos.y, Start_pos.z].Pos;
                            boxing = true;
                        }
                    }
                    else if (mood == 2)
                    {
                        if (copying)
                        {
                            End_pos = new Vector3Int(pos(position.x), pos(position.y), tg.z);
                            copy = Coping(Start_pos, Now_pos);
                            copying = false;
                        }
                        else
                        {
                            Start_pos = new Vector3Int(pos(position.x), pos(position.y), tg.z);
                            copying = true;
                        }
                    }
                    break;
                case "z-":
                    if (mood == 0)
                    {
                        if (!chenge_state)
                        {
                            if (cell[pos(position.x), pos(position.y), tg.z].life)
                            {
                                Destroy(cell[pos(position.x), pos(position.y), tg.z].obj);
                                cell[pos(position.x), pos(position.y), tg.z].life = false;
                                state[pos(position.x), pos(position.y), tg.z] = false;
                                hush[pos(position.x), pos(position.y), tg.z, memory_time] = false;
                                //chenge_state = false;
                            }
                        }
                        else
                        {
                            if (!cell[pos(position.x), pos(position.y), tg.z].life)
                            {
                                cell[pos(position.x), pos(position.y), tg.z].obj = Instantiate(CellPrefab) as GameObject;
                                cell[pos(position.x), pos(position.y), tg.z].obj.transform.localPosition = cell[pos(position.x), pos(position.y), tg.z].Pos;
                                cell[pos(position.x), pos(position.y), tg.z].life = true;
                                state[pos(position.x), pos(position.y), tg.z] = true;
                                hush[pos(position.x), pos(position.y), tg.z, memory_time] = true;
                                //chenge_state = true;
                            }
                            else
                            {
                                int i = 1;
                                while (true)
                                {
                                    if (cell[pos(position.x), pos(position.y), tg.z + i].life)
                                    {
                                        if (tg.z + i < gridSize - 1)
                                        {
                                            i++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        cell[pos(position.x), pos(position.y), tg.z + i].obj = Instantiate(CellPrefab) as GameObject;
                                        cell[pos(position.x), pos(position.y), tg.z + i].obj.transform.localPosition = cell[pos(position.x), pos(position.y), tg.z + i].Pos;
                                        cell[pos(position.x), pos(position.y), tg.z + i].life = true;
                                        state[pos(position.x), pos(position.y), tg.z + i] = true;
                                        hush[pos(position.x), pos(position.y), tg.z + i, memory_time] = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (mood == 1)
                    {
                        if (boxing)
                        {
                            End_pos = new Vector3Int(pos(position.x), pos(position.y), tg.z);
                            Destroy(Box_Start);
                            Boxing(Start_pos, End_pos);
                            boxing = false;
                        }
                        else
                        {
                            Start_pos = new Vector3Int(pos(position.x), pos(position.y), tg.z);
                            Box_Start = Instantiate(TargetPrefab) as GameObject;
                            Box_Start.GetComponent<Renderer>().material = Cell_box;
                            Box_Start.transform.localPosition = cell[Start_pos.x, Start_pos.y, Start_pos.z].Pos;
                            boxing = true;
                        }
                    }
                    else if (mood == 2)
                    {
                        if (copying)
                        {
                            End_pos = new Vector3Int(pos(position.x), pos(position.y), tg.z);
                            copy = Coping(Start_pos, Now_pos);
                            copying = false;
                        }
                        else
                        {
                            Start_pos = new Vector3Int(pos(position.x), pos(position.y), tg.z);
                            copying = true;
                        }
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

    private void stateChec(bool[,,] Che_sta, int st_x, int st_y, int st_z)
    {/*
        for (int a = st_x * range; a < (st_x + 1) * range; a++)
        {
            for (int b = st_y * range; b < (st_y + 1) * range; b++)
            {
                for (int c = st_z * range; c < (st_z + 1) * range; c++)
                {
                    if (state[a,b,c])
                    {
                        cell[a, b, c].life = true;
                        hush[a, b, c, memory_time] = true;
                    } else
                    {
                        cell[a, b, c].life = false;
                        hush[a, b, c, memory_time] = false;
                    }
                }
            }
        }*/
        if (state[st_x, st_y, st_z])
        {
            cell[st_x, st_y, st_z].life = true;
            hush[st_x, st_y, st_z, memory_time] = true;
        }
        else
        {
            cell[st_x, st_y, st_z].life = false;
            hush[st_x, st_y, st_z, memory_time] = false;
        }
    }

    private void Change_rule()
    {
        WSANativeFilePicker.PickSingleFile("Select", WSAPickerViewMode.Thumbnail, WSAPickerLocationId.PicturesLibrary, new[] { ".txt" }, result =>
        {
             //string data = "";

            if (result != null)
            {
                byte[] fileBytes = result.ReadBytes();
#pragma warning disable 0219
                string fileString = result.ReadText();
#pragma warning restore 0219
                string num = "";
                int[] new_rule = new int[5];
                int purin = 0;
                for (int i = 0; i < fileString.Length; i++)
                {
                    if (fileString[i] >= '0' && fileString[i] <= '9')
                    {
                        num += fileString[i];
                    }
                    else if (fileString[i] == ',')
                    {
                        new_rule[purin] = int.Parse(num);
                        purin++;
                        num = "";
                    }
                    if (purin > 4)
                    {
                        rule = new_rule;
                    }
                }
                ab.text = "[" + rule[0].ToString() + "," + rule[1].ToString() + "," + rule[2].ToString() + "," + rule[3].ToString() + "," + rule[4].ToString() + "]";
            }            
        });
    }

    private void Boxing(Vector3Int Start, Vector3Int End)
    {
        int start_x;
        int start_y;
        int start_z;
        int end_x;
        int end_y;
        int end_z;
        if (Start.x <= End.x)
        {
            start_x = Start.x;
            end_x = End.x;
        }
        else
        {
            start_x = End.x;
            end_x = Start.x;
        }
        if (Start.y <= End.y)
        {
            start_y = Start.y;
            end_y = End.y;
        }
        else
        {
            start_y = End.y;
            end_y = Start.y;
        }
        if (Start.z <= End.z)
        {
            start_z = Start.z;
            end_z = End.z;
        }
        else
        {
            start_z = End.z;
            end_z = Start.z;
        }
        if (start_x != end_x || start_y != end_y)
        {
            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    for (int z = start_z; z <= end_z; z++)
                    {
                        if (cell[x, y, z].life != chenge_state)
                        {
                            if (cell[x, y, z].life)
                            {
                                Destroy(cell[x, y, z].obj);
                                cell[x, y, z].life = false;
                                state[x, y, z] = false;
                                hush[x, y, z, memory_time] = false;
                            }
                            else
                            {
                                cell[x, y, z].obj = Instantiate(CellPrefab) as GameObject;
                                cell[x, y, z].obj.transform.localPosition = cell[x, y, z].Pos;
                                cell[x, y, z].life = true;
                                state[x, y, z] = true;
                                hush[x, y, z, memory_time] = true;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool[,,] Coping(Vector3Int Start, Vector3Int End)
    {
        int start_x;
        int start_y;
        int start_z;
        int end_x;
        int end_y;
        int end_z;
        if (Start.x <= End.x)
        {
            start_x = Start.x;
            end_x = End.x;
        }
        else
        {
            start_x = End.x;
            end_x = Start.x;
        }
        if (Start.y <= End.y)
        {
            start_y = Start.y;
            end_y = End.y;
        }
        else
        {
            start_y = End.y;
            end_y = Start.y;
        }
        if (Start.z <= End.z)
        {
            start_z = Start.z;
            end_z = End.z;
        }
        else
        {
            start_z = End.z;
            end_z = Start.z;
        }
        bool[,,] copy = new bool[end_x - start_x + 1, end_y - start_y + 1, end_z - start_z + 1];
        if (start_x != end_x || start_y != end_y)
        {
            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    for (int z = start_z; z <= end_z; z++)
                    {
                        copy[x - start_x, y - start_y, z - start_z] = cell[x, y, z].life;
                    }
                }
            }
        }
        return copy;

    }

    private void pathting(Vector3Int Start, bool[,,] copy)
    {
        int copy_x = copy.GetLength(0);
        int copy_y = copy.GetLength(1);
        int copy_z = copy.GetLength(2);
        for (int x = 0; x <= copy_x; x++)
        {
            for (int y = 0; y <= copy_y; y++)
            {
                for (int z = 0; z <= copy_z; z++)
                {
                    if (copy[x,y,z])
                    {
                        cell[x+Start.x, y + Start.y, z + Start.z].obj = Instantiate(CellPrefab) as GameObject;
                        cell[x + Start.x, y + Start.y, z + Start.z].obj.transform.localPosition = cell[x + Start.x, y + Start.y, z + Start.z].Pos;
                        cell[x + Start.x, y + Start.y, z + Start.z].life = true;
                        state[x + Start.x, y + Start.y, z + Start.z] = true;
                        hush[x + Start.x, y + Start.y, z + Start.z, memory_time] = true;
                    }
                    else
                    {                        
                        Destroy(cell[x + Start.x, y + Start.y, z + Start.z].obj);
                        cell[x + Start.x, y + Start.y, z + Start.z].life = false;
                        state[x + Start.x, y + Start.y, z + Start.z] = false;
                        hush[x + Start.x, y + Start.y, z + Start.z, memory_time] = false;
                    }
                }
            }
        }
    }
}
