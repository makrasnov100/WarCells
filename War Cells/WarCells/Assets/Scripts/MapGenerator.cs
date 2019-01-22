using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    //References (TODO: Make Private)
    public PlayerManager playerManager;
    public TurnUIController turnController;

    //Map Storage
    List<List<GameObject>> cells = new List<List<GameObject>>();
    List<GameObject> newCells = new List<GameObject>();

    //Connections Settings
    public float connectionPrevalence;
    public Material conMat;

    //Cell Settings
    public int minCellSize;
    public int maxCellSize;
    [Range(1, 10)]
    public float cellPadding;
    public int mapSize;
    private Vector2[] cellPositions = new Vector2[6];
    public int chanceDecrease = 5;
    [Range(0, 100)]
    public float bridgeChance;
    public bool PrefabMap;

    //Map Settings
    [Range(1,99)]
    public int mapSizeVertical;
    [Range(1, 99)]
    public int mapSizeHorizontal;
    public int mapCenterPosX;
    public int mapCenterPosY;
    public bool horizontalSym;
    public bool verticalSym;
    public List<GameObject> cellTypes;

    //Map Creation helpers
    int cellCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (playerManager == null)
            playerManager = GameObject.FindGameObjectWithTag("playerManager").GetComponent<PlayerManager>();
        if (turnController == null)
            turnController = GameObject.FindGameObjectWithTag("turnController").GetComponent<TurnUIController>();
        if (PrefabMap)
            return;
        // Using Sqrt(3) as 1.732
        //Preset cell locations around a cell (Hexagon) (Starting veryleft, making it around clock wise)
        cellPositions[0] = new Vector2(-cellPadding, 0);
        cellPositions[1] = new Vector2(-cellPadding/2, (cellPadding / 2) * 1.732f);
        cellPositions[2] = new Vector2(cellPadding / 2, (cellPadding / 2) * 1.732f);
        cellPositions[3] = new Vector2(cellPadding, 0);
        cellPositions[4] = new Vector2(cellPadding / 2, -(cellPadding / 2) * 1.732f);
        cellPositions[5] = new Vector2(-cellPadding / 2, -(cellPadding / 2) * 1.732f);

        GenerateUniformWorld(0, null ,new Vector2(0,0)); //Generates starting from cell 0
        playerManager.Construct(cells, newCells);
        playerManager.SpawnPlayers();

        //if (!GenerateWorld())
        //  print("Failed to Create Map!");
    }

    private void GenerateUniformWorld(int curRecursive, CellIdentity baseCell, Vector2 curPosition)
    {
        
        //base case for center
        if(curRecursive == 0)
        {
            int curCellIdx = 1;
            //Instatiate Cell
            GameObject curCell = Instantiate(cellTypes[curCellIdx], new Vector2(0,0), new Quaternion(), transform);
            curCell.name = curCell.name + cellCounter;
            //Generate Cell Properties
            CellIdentity cellId = curCell.GetComponent<CellIdentity>();
            cellId.Construct(cellCounter++, Random.Range(minCellSize, maxCellSize + 1));
            baseCell = cellId;

            //Store Cell
            newCells.Add(curCell);
        }

        List<int> usedPos = new List<int>();
        usedPos.Add(0);
        usedPos.Add(1);
        usedPos.Add(2);
        usedPos.Add(3);
        usedPos.Add(4);
        usedPos.Add(5);

        while(usedPos.Count != 0)
        {
            int randPos = Random.Range(0, usedPos.Count-1);
            Vector2 pos = cellPositions[usedPos[randPos]];
            usedPos.RemoveAt(randPos);

            //Sees if that position will have a cell
            int curCellIdx = 0;
            int r = Random.Range(1, 100);
            int curPercent = Mathf.Min(chanceDecrease * curRecursive, 90);
            if (r <= curPercent)
            {
                curCellIdx = 0;//no cell
                newCells.Add(null);
            }               
            else
            {
                Collider2D cell = Physics2D.OverlapCircle(curPosition + pos, .5f);
                if (cell != null && !cell.gameObject.GetComponent<CellIdentity>().IsConnectedTo(baseCell.GetId())) // If a cell already exists at this position, and no bridge already. attempt to make a bridge to it 
                {
                    int r2 = Random.Range(1, 100);
                    if(r2 >= bridgeChance * curRecursive) // bridgechance, chance to mke a bridge
                    {
                        GameObject line = new GameObject();
                        line.transform.parent = gameObject.transform;
                        CellIdentity oldCellId = cell.gameObject.GetComponent<CellIdentity>();
                        LineRenderer lr2 = line.AddComponent<LineRenderer>();
                        lr2.SetPosition(0, curPosition);
                        lr2.SetPosition(1, curPosition + pos);
                        lr2.material = new Material(Shader.Find("Sprites/Default"));
                        lr2.startColor = Color.yellow;
                        lr2.endColor = Color.yellow;
                        lr2.startWidth = .2f;
                        lr2.endWidth = .2f;
                        lr2.sortingOrder = -10;
                        oldCellId.AddConnection(baseCell.GetId(), baseCell.gameObject, 0, lr2, false);
                        baseCell.AddConnection(oldCellId.GetId(), oldCellId.gameObject, 1, lr2, true);
                    }
                    continue;
                }
                else if(cell != null)
                {
                    continue;
                }


                curCellIdx = Random.Range(1, cellTypes.Count);//is a cell
                //Instatiate Cell
                GameObject curCell = Instantiate(cellTypes[curCellIdx], curPosition + pos, new Quaternion(), transform);
                curCell.name = curCell.name + cellCounter;
                //Generate Cell Properties
                CellIdentity cellId = curCell.GetComponent<CellIdentity>();
                cellId.Construct(cellCounter++, Random.Range(minCellSize, maxCellSize + 1));
                LineRenderer lr = curCell.AddComponent<LineRenderer>();
                lr.SetPosition(0, curPosition);
                lr.SetPosition(1, curPosition + pos);
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = Color.yellow;
                lr.endColor = Color.yellow;
                lr.startWidth = .2f;
                lr.endWidth = .2f;
                lr.sortingOrder = -10;
                cellId.AddConnection(baseCell.GetId(), baseCell.gameObject, 0, lr, false);
                baseCell.AddConnection(cellId.GetId(), cellId.gameObject, 1, lr, true);
                newCells.Add(curCell);
                if (curRecursive < mapSize)
                    GenerateUniformWorld(curRecursive + 1, cellId ,curPosition + pos);
            }
                          
        }
    }

    //Creates the game world (handles symmetry)
    private bool GenerateWorld()
    {
        //Create a template map
        bool success = CreateMap(mapSizeHorizontal, mapSizeVertical);

        //Finish symetric map if needed
        if (!success)
            return false;
        else
        {
            //Create map fillers and the entire right side
            if (horizontalSym)
            {
                //Find amount of cells to generate
                int sizeY = mapSizeVertical;
                if(verticalSym)
                    sizeY /= 2;
                int sizeX = mapSizeHorizontal - cells[0].Count;

                float xOffset = 0;
                if (mapSizeHorizontal % 2 == 0)
                    xOffset += cellPadding/2;
                float yOffset = 0;
                if (mapSizeVertical % 2 == 0)
                    yOffset += cellPadding / 2;

                Vector3 startPos = new Vector3(xOffset, (-(mapSizeVertical/2) * cellPadding) + yOffset, 0);

                //Create a vertical filler if an odd sized map
                if (mapSizeHorizontal % 2 != 0)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        int curCellIdx = Random.Range(0, cellTypes.Count);
                        if (cellTypes[curCellIdx].name != "Empty")
                        {
                            //Instatiate Cell
                            Vector3 curPos = new Vector3(startPos.x, startPos.y + (cellPadding * y), 0);
                            GameObject curCell = Instantiate(cellTypes[curCellIdx], curPos, new Quaternion(), transform);
                            curCell.name = curCell.name + cellCounter;
                            //Generate Cell Properties
                            CellIdentity cellId = curCell.GetComponent<CellIdentity>();
                            cellId.Construct(cellCounter++, Random.Range(minCellSize, maxCellSize + 1));

                            //Store Cell
                            cells[y].Add(curCell);
                        }
                        else
                        {
                            cells[y].Add(null);
                        }
                    }
                    startPos.x += cellPadding;
                }

                //Duplicate existing map
                for (int y = 0; y < sizeY; y++)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        if (cells[y][sizeX - x - 1 ] != null)
                        {
                            GameObject curCell = Instantiate(cells[y][sizeX - x - 1], new Vector3(startPos.x + (cellPadding * x), startPos.y + (cellPadding * y), 0), new Quaternion(), transform);
                            curCell.name = curCell.name + cellCounter;

                            CellIdentity cellId = curCell.GetComponent<CellIdentity>();
                            cellId.Reconstruct(cellCounter++, cells[y][sizeX - x - 1].GetComponent<CellIdentity>());

                            //Store Cell
                            cells[y].Add(curCell);
                        }
                        else
                        {
                            cells[y].Add(null);
                        }
                    }
                }
            }

            //Clone the top of map of needs to be vertically symmetrical
            if (verticalSym)
            {
                //Calculate starting location
                float yOffset = 0;
                if (mapSizeVertical % 2 == 0)
                    yOffset += cellPadding/2;
                float XOffset = 0;
                if (mapSizeHorizontal % 2 == 0)
                    XOffset += cellPadding / 2;
                Vector3 startPos = new Vector3((-(mapSizeHorizontal/2) * cellPadding) + XOffset, yOffset, 0);

                //Complete copy of the map top (TODO: exept last row if odd sized vertically)
                for (int y = 0; y < (mapSizeVertical/2); y++)
                {
                    List<GameObject> curRow = new List<GameObject>();
                    for (int x = 0; x < mapSizeHorizontal; x++)
                    {
                        if (cells[(mapSizeVertical / 2) - y - 1][x] != null)
                        {
                            GameObject curCell = Instantiate(cells[(mapSizeVertical/2) - y - 1][x], new Vector3(startPos.x + (cellPadding * x), startPos.y + (cellPadding * y), 0), new Quaternion(), transform);
                            curCell.name = curCell.name + cellCounter;

                            CellIdentity cellId = curCell.GetComponent<CellIdentity>();
                            cellId.Reconstruct(cellCounter++, cells[(mapSizeVertical / 2) - y - 1][x].GetComponent<CellIdentity>());

                            //Store Cell
                            curRow.Add(curCell);
                        }
                        else
                        {
                            curRow.Add(null);
                        }
                    }
                    cells.Add(curRow);
                }
            }
        }

        //Add connections to other cells
        CreateConnections();

        //Add needed amounts of players to the map (both bots and real)
        playerManager.Construct(cells, newCells);
        playerManager.SpawnPlayers();

        return true;
    }

    //Populates the current cell list with cells and connections given the size of the map
    bool CreateMap(int sizeX, int sizeY)
    {
        //Find starting cell location
        float startX = -(cellPadding * ((float)sizeX / 2));
        float startY = -(cellPadding * ((float)sizeY / 2));
        if (mapSizeHorizontal % 2 == 0)
            startX += cellPadding / 2;
        if (mapSizeVertical % 2 == 0)
            startY += cellPadding / 2;
        if (horizontalSym)
            sizeX /= 2;
        if (verticalSym)
            sizeY /= 2;

        for (int y = 0; y < sizeY; y++)
        {
            List<GameObject> curRow = new List<GameObject>();
            cells.Add(curRow);

            for (int x = 0; x < sizeX; x++)
            {
                int curCellIdx = Random.Range(0, cellTypes.Count);
                if (cellTypes[curCellIdx].name != "Empty")
                {
                    //Instatiate Cell
                    Vector3 curPos = new Vector3(startX + (cellPadding * x), startY + (cellPadding * y), 0);
                    GameObject curCell = Instantiate(cellTypes[curCellIdx], curPos, new Quaternion(), transform);
                    curCell.name = curCell.name + " " + x + ", " + y;
                    //Generate Cell Properties
                    CellIdentity cellId = curCell.GetComponent<CellIdentity>();
                    cellId.Construct(cellCounter++, Random.Range(minCellSize, maxCellSize + 1));

                    //Store Cell
                    cells[y].Add(curCell);
                }
                else
                {
                    cells[y].Add(null);
                }
            }
        }
        return true;
    }

    //CreateConnections: creates connections and accepts symmetry
    private void CreateConnections()
    {
        for (int y = 0; y < cells.Count; y++)
        {
            for (int x = 0; x < cells[0].Count; x++)
            {
                //Draw and Add Connections (up) then (left)
                if (y != 0 && cells[y - 1][x] != null && cells[y][x] != null && Random.Range(0f, 1f) <= connectionPrevalence)
                {
                    GameObject curCon = new GameObject("Connection");
                    curCon.transform.parent = transform;
                    LineRenderer lr = curCon.AddComponent<LineRenderer>();
                    lr.SetPosition(0, cells[y - 1][x].transform.position);
                    lr.SetPosition(1, cells[y][x].transform.position);
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                    lr.startColor = Color.yellow;
                    lr.endColor = Color.yellow;
                    lr.startWidth = .2f;
                    lr.endWidth = .2f;
                    lr.sortingOrder = -10;

                    cells[y - 1][x].GetComponent<CellIdentity>().AddConnection(cells[y][x].GetComponent<CellIdentity>().GetId(), cells[y][x], 0, lr, true);
                    cells[y][x].GetComponent<CellIdentity>().AddConnection(cells[y - 1][x].GetComponent<CellIdentity>().GetId(), cells[y - 1][x], 1, lr, false);
                }
                if (x != 0 && cells[y][x - 1] != null && cells[y][x] != null && Random.Range(0f, 1f) <= connectionPrevalence)
                {
                    GameObject curCon = new GameObject("Connection");
                    curCon.transform.parent = transform;
                    LineRenderer lr = curCon.AddComponent<LineRenderer>();
                    lr.SetPosition(0, cells[y][x - 1].transform.position);
                    lr.SetPosition(1, cells[y][x].transform.position);
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                    lr.startColor = Color.yellow;
                    lr.endColor = Color.yellow;
                    lr.startWidth = .2f;
                    lr.endWidth = .2f;
                    lr.sortingOrder = -10;

                    cells[y][x - 1].GetComponent<CellIdentity>().AddConnection(cells[y][x].GetComponent<CellIdentity>().GetId(), cells[y][x], 3, lr, true);
                    cells[y][x].GetComponent<CellIdentity>().AddConnection(cells[y][x - 1].GetComponent<CellIdentity>().GetId(), cells[y][x - 1], 2, lr, false);
                }
            }
        }
    }
}
