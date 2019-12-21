using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    //References (TODO: Make Private)
    public PlayerManager playerManager;
    public TurnUIController turnController;

    //Map Storage
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
        //GenerateSymetricWorld(0, null, new Vector2(0, 0)); // Generate symetric map
        playerManager.Construct(newCells);
        playerManager.SpawnPlayers();

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
                    if(r2 >= bridgeChance) // bridgechance, chance to mke a bridge
                    {
                        GameObject line = new GameObject();
                        line.transform.parent = gameObject.transform;
                        CellIdentity oldCellId = cell.gameObject.GetComponent<CellIdentity>();
                        LineRenderer lr2 = line.AddComponent<LineRenderer>();
                        lr2.SetPosition(0, curPosition);
                        lr2.SetPosition(1, curPosition + pos);
                        lr2.material = new Material(Shader.Find("Sprites/Default"));
                        lr2.startColor = Color.black;
                        lr2.endColor = Color.black;
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
                lr.startColor = Color.black;
                lr.endColor = Color.black;
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

    //Only build the world symetricly
    private void GenerateSymetricWorld(int curRecursive, CellIdentity baseCell, Vector2 curPosition)
    {

        GenerateHalf(1);
        GenerateHalf(-1);

        void GenerateHalf(int half)
        {
            Random.InitState(1);
            //base case for center
            if (curRecursive == 0)
            {

                int curCellIdx = 1;
                //Instatiate Cell
                GameObject curCell = Instantiate(cellTypes[curCellIdx], new Vector2(0, 0), new Quaternion(), transform);
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
            usedPos.Add(5);

            while (usedPos.Count != 0)
            {
                int randPos = Random.Range(0, usedPos.Count - 1);
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
                    Collider2D cell = Physics2D.OverlapCircle(half * (curPosition + pos), .5f);
                    if (cell != null && !cell.gameObject.GetComponent<CellIdentity>().IsConnectedTo(baseCell.GetId())) // If a cell already exists at this position, and no bridge already. attempt to make a bridge to it 
                    {
                        int r2 = Random.Range(1, 100);
                        if (r2 >= bridgeChance * curRecursive) // bridgechance, chance to mke a bridge
                        {
                            GameObject line = new GameObject();
                            line.transform.parent = gameObject.transform;
                            CellIdentity oldCellId = cell.gameObject.GetComponent<CellIdentity>();
                            LineRenderer lr2 = line.AddComponent<LineRenderer>();
                            lr2.SetPosition(0, half * curPosition);
                            lr2.SetPosition(1, half * (curPosition + pos));
                            lr2.material = new Material(Shader.Find("Sprites/Default"));
                            lr2.startColor = Color.black;
                            lr2.endColor = Color.black;
                            lr2.startWidth = .2f;
                            lr2.endWidth = .2f;
                            lr2.sortingOrder = -10;
                            oldCellId.AddConnection(baseCell.GetId(), baseCell.gameObject, 0, lr2, false);
                            baseCell.AddConnection(oldCellId.GetId(), oldCellId.gameObject, 1, lr2, true);
                        }
                        continue;
                    }
                    else if (cell != null)
                    {
                        continue;
                    }


                    curCellIdx = Random.Range(1, cellTypes.Count);//is a cell
                                                                  //Instatiate Cell
                    GameObject curCell = Instantiate(cellTypes[curCellIdx], half*(curPosition + pos), new Quaternion(), transform);
                    curCell.name = curCell.name + cellCounter;
                    //Generate Cell Properties
                    CellIdentity cellId = curCell.GetComponent<CellIdentity>();
                    cellId.Construct(cellCounter++, Random.Range(minCellSize, maxCellSize + 1));
                    LineRenderer lr = curCell.AddComponent<LineRenderer>();
                    lr.SetPosition(0, half * curPosition);
                    lr.SetPosition(1, half * (curPosition + pos));
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                    lr.startColor = Color.black;
                    lr.endColor = Color.black;
                    lr.startWidth = .2f;
                    lr.endWidth = .2f;
                    lr.sortingOrder = -10;
                    cellId.AddConnection(baseCell.GetId(), baseCell.gameObject, 0, lr, false);
                    baseCell.AddConnection(cellId.GetId(), cellId.gameObject, 1, lr, true);
                    newCells.Add(curCell);
                    if (curRecursive < mapSize)
                        GenerateUniformWorld(curRecursive + 1, cellId, half * (curPosition + pos));
                }
            }

        }
    }

    //[ACCESSORS/MUTATORS]
    public void SetMapSize(int newSize) { mapSize = newSize; }
    public void SetMinCellSize(int newMin) { minCellSize = newMin; }
    public void SetMaxCellSize(int newMax) { maxCellSize = newMax; }
}
