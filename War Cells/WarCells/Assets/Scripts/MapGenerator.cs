using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    //References TODO: Make Private
    public PlayerManager playerManager;
    public TurnUIController turnController;

    //Map Storage
    List<List<GameObject>> cells = new List<List<GameObject>>();

    //Connections Settings
    public float connectionPrevalence;
    public Material conMat;

    //Cell Settings
    public int minCellSize;
    public int maxCellSize;
    [Range(1, 10)]
    public float cellPadding;

    //Map Settings
    [Range(1,99)]
    public int mapSize;
    public int mapCenterPosX;
    public int mapCenterPosY;
    public bool horizontalSym;
    public bool verticalSym;
    public List<GameObject> cellTypes;

    // Start is called before the first frame update
    void Start()
    {
        if (!CreateMap())
        {
            print("Failed to Create Map!"); //TODO: Create user msg system (GUI)
        }
    }

    bool CreateMap()
    {
        if (mapSize % 2 == 0) // Need odd side length
            mapSize++;

        float startX = -(cellPadding * (mapSize / 2));
        float startY = startX;

        for (int y = 0; y < mapSize; y++)
        {
            List<GameObject> curRow = new List<GameObject>();
            cells.Add(curRow);

            for (int x = 0; x < mapSize; x++)
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
                    cellId.Construct((y*mapSize) + x, turnController);
                    cellId.SetUnitCapacity(Random.Range(minCellSize, maxCellSize + 1));

                    //Store Cell
                    cells[y].Add(curCell);

                    //Draw and Add Connections (up) then (left)
                    if (y != 0 && cells[y-1][x] != null && Random.Range(0f, 1f) <= connectionPrevalence)
                    {
                        GameObject curCon = new GameObject("Connection");
                        curCon.transform.parent = transform;
                        LineRenderer lr = curCon.AddComponent<LineRenderer>();
                        lr.SetPosition(0, cells[y-1][x].transform.position);
                        lr.SetPosition(1, curPos);
                        lr.material = new Material(Shader.Find("Sprites/Default")); //TODO: Remember to delete this material when deleteing connetions
                        lr.startColor = Color.yellow;
                        lr.endColor = Color.yellow;
                        lr.startWidth = .2f;
                        lr.endWidth = .2f;
                        lr.sortingOrder = -10;

                        cells[y - 1][x].GetComponent<CellIdentity>().AddConnection(cellId.GetId(), 0, curCon, true);
                        cellId.AddConnection(cells[y - 1][x].GetComponent<CellIdentity>().GetId(), 1, curCon, false);
                    }
                    if (x != 0 && cells[y][x-1] != null && Random.Range(0f, 1f) <= connectionPrevalence)
                    {
                        GameObject curCon = new GameObject("Connection");
                        curCon.transform.parent = transform;
                        LineRenderer lr = curCon.AddComponent<LineRenderer>();
                        lr.SetPosition(0, cells[y][x-1].transform.position);
                        lr.SetPosition(1, curPos);
                        lr.material = new Material(Shader.Find("Sprites/Default")); //TODO: Remember to delete this material when deleteing connetions
                        lr.startColor = Color.yellow;
                        lr.endColor = Color.yellow;
                        lr.startWidth = .2f;
                        lr.endWidth = .2f;
                        lr.sortingOrder = -10;

                        cells[y][x - 1].GetComponent<CellIdentity>().AddConnection(cellId.GetId(), 3, curCon, true);
                        cellId.AddConnection(cells[y][x - 1].GetComponent<CellIdentity>().GetId(), 2, curCon, false);
                    }
                }
                else
                {
                    cells[y].Add(null);
                }
            }
        }

        playerManager.Construct(cells);
        playerManager.SpawnPlayers();

        return true;
    }
}
