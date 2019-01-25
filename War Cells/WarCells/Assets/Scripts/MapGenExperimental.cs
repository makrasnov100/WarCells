using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Edge
{
    public float distance;
    public GameObject origin;
    public GameObject destination;

    public Edge(float distance, GameObject origin, GameObject destination)
    {
        this.distance = distance;
        this.origin = origin;
        this.destination = destination;
    }
}

public class MapGenExperimental : MonoBehaviour
{
    //References (TODO: Make Private)
    public PlayerManager playerManager;
    public TurnUIController turnController;

    //Map Storage
    List<List<GameObject>> cells = new List<List<GameObject>>();

    //Connections Settings
    public float connectionPrevalence;
    public Material conMat;
    public float maxConnectionDistance;

    //Cell Settings
    public int minCellSize;
    public int maxCellSize;
    [Range(1, 10)]
    public float cellPadding;

    //Map Settings
    [Range(1, 99)]
    public int mapSizeVertical;
    [Range(1, 99)]
    public int mapSizeHorizontal;
    public int mapCenterPosX;
    public int mapCenterPosY;
    public bool horizontalSym;
    public bool verticalSym;
    public bool diagonalLayout;
    public bool randomizerLayout;
    public List<GameObject> cellTypes;

    //Map Creation helpers
    int cellCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (!GenerateWorld())
            print("Failed to Create Map!");
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
                if (verticalSym)
                    sizeY /= 2;
                int sizeX = mapSizeHorizontal - cells[0].Count;

                float xOffset = 0;
                if (mapSizeHorizontal % 2 == 0)
                    xOffset += cellPadding / 2;
                float yOffset = 0;
                if (mapSizeVertical % 2 == 0)
                    yOffset += cellPadding / 2;

                Vector3 startPos = new Vector3(xOffset, (-(mapSizeVertical / 2) * cellPadding) + yOffset, 0);

                //Duplicate existing map
                for (int y = 0; y < sizeY; y++)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        if (cells[y][sizeX - x - 1] != null)
                        {
                            //Layout Offsets
                            Vector3 layoutOffset;
                            if (diagonalLayout)
                            {
                                if (y % 2 == 0)
                                    layoutOffset = new Vector3(-cellPadding / 4, 0, 0);
                                else
                                    layoutOffset = new Vector3(cellPadding / 4, 0, 0);
                            }
                            else if (randomizerLayout)
                            {
                                layoutOffset = new Vector3(Random.Range(-cellPadding / 3, cellPadding / 3), Random.Range(-cellPadding / 3, cellPadding / 3), 0);
                            }
                            else
                            {
                                layoutOffset = Vector3.zero;
                            }

                            GameObject curCell = Instantiate(cells[y][sizeX - x - 1], new Vector3(startPos.x + (cellPadding * x), startPos.y + (cellPadding * y), 0) + layoutOffset, new Quaternion(), transform);
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

            //Clone the top of map if needs to be vertically symmetrical
            if (verticalSym)
            {
                //Calculate starting location
                float yOffset = 0;
                if (mapSizeVertical % 2 == 0)
                    yOffset += cellPadding / 2;
                float XOffset = 0;
                if (mapSizeHorizontal % 2 == 0)
                    XOffset += cellPadding / 2;
                Vector3 startPos = new Vector3((-(mapSizeHorizontal / 2) * cellPadding) + XOffset, yOffset, 0);

                //Complete copy of the map top (TODO: exept last row if odd sized vertically)
                for (int y = 0; y < (mapSizeVertical / 2); y++)
                {
                    List<GameObject> curRow = new List<GameObject>();
                    for (int x = 0; x < mapSizeHorizontal; x++)
                    {
                        if (cells[(mapSizeVertical / 2) - y - 1][x] != null)
                        {
                            //Layout Offsets
                            Vector3 layoutOffset;
                            if (diagonalLayout)
                            {
                                if (y % 2 == 0)
                                    layoutOffset = new Vector3(-cellPadding / 4, 0, 0);
                                else
                                    layoutOffset = new Vector3(cellPadding / 4, 0, 0);
                            }
                            else if (randomizerLayout)
                            {
                                layoutOffset = new Vector3(Random.Range(-cellPadding / 3, cellPadding / 3), Random.Range(-cellPadding / 3, cellPadding / 3), 0);
                            }
                            else
                            {
                                layoutOffset = Vector3.zero;
                            }

                            GameObject curCell = Instantiate(cells[(mapSizeVertical / 2) - y - 1][x], new Vector3(startPos.x + (cellPadding * x), startPos.y + (cellPadding * y), 0) + layoutOffset, new Quaternion(), transform);
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
        playerManager.Construct(cells);
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
                    //Layout Offsets
                    Vector3 layoutOffset;
                    if (diagonalLayout)
                    {
                        if (y % 2 == 0)
                            layoutOffset = new Vector3(-cellPadding / 4, 0, 0);
                        else
                            layoutOffset = new Vector3(cellPadding / 4, 0, 0);
                    }
                    else if (randomizerLayout)
                    {
                        layoutOffset = new Vector3(Random.Range(-cellPadding / 3, cellPadding / 3), Random.Range(-cellPadding / 3, cellPadding / 3), 0);
                    }
                    else
                    {
                        layoutOffset = Vector3.zero;
                    }

                    //Instatiate Cell
                    Vector3 curPos = new Vector3(startX + (cellPadding * x), startY + (cellPadding * y), 0);
                    GameObject curCell = Instantiate(cellTypes[curCellIdx], curPos + layoutOffset, new Quaternion(), transform);
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
        //Sort smallest distances
        List<Edge> distances = new List<Edge>();
        int cellCount = 0;

        //Find all possible edges subject to predifined length
        for (int y = 0; y < cells.Count; y++)
        {
            for (int x = 0; x < cells[y].Count; x++)
            {
                if (cells[y][x] != null)
                {
                    Collider2D[] allCells = Physics2D.OverlapCircleAll(cells[y][x].transform.position, maxConnectionDistance);
                    foreach (Collider2D c in allCells)
                        if (c.gameObject.GetComponent<CellIdentity>().GetId() != cells[y][x].GetComponent<CellIdentity>().GetId())
                            distances.Add(new Edge(Vector3.Distance(cells[y][x].transform.position, c.gameObject.transform.position), cells[y][x], c.gameObject));

                    cellCount++;
                }
            }
        }

        //Sort all edjes based on their distances
        List<Edge> sortedDistances;
        sortedDistances = MergeSortEdges(distances);

        //Hash
        HashSet<int> cellsId = new HashSet<int>();
        Dictionary<int, List<int>> curNetwork = new Dictionary<int, List<int>>();
        int maxConnections = cellCount - 1;
        for (int i = 0; i < maxConnections; i++)
        {
            if (i >= sortedDistances.Count)
                return;

            if (EdgeFormsLoop(sortedDistances[i], curNetwork))
            {
                maxConnections++;
                continue;
            }

            //Create connection component
            GameObject curCon = new GameObject("Connection");
            curCon.transform.parent = transform;
            LineRenderer lr = curCon.AddComponent<LineRenderer>();
            lr.SetPosition(0, sortedDistances[i].origin.transform.position);
            lr.SetPosition(1, sortedDistances[i].destination.transform.position);
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = .2f;
            lr.endWidth = .2f;
            lr.sortingOrder = -10;

            //Add connection info top scripts
            int originId = sortedDistances[i].origin.GetComponent<CellIdentity>().GetId();
            int destinationId = sortedDistances[i].destination.GetComponent<CellIdentity>().GetId();
            sortedDistances[i].origin.GetComponent<CellIdentity>().AddConnection(destinationId, sortedDistances[i].destination, 0, lr, true);
            sortedDistances[i].destination.GetComponent<CellIdentity>().AddConnection(originId, sortedDistances[i].origin, 1, lr, false);

            //Enlargen current network
            if (curNetwork.ContainsKey(originId))
                curNetwork[originId].Add(destinationId);
            else
                curNetwork.Add(originId, new List<int>() { destinationId });

            if (curNetwork.ContainsKey(destinationId))
                curNetwork[destinationId].Add(originId);
            else
                curNetwork.Add(destinationId, new List<int>() { originId });
        }

        //Additional connections (random)
        int additionalConnections = maxConnections + cellCount;

        if (sortedDistances.Count > maxConnections)
        {
            for (int i = maxConnections; i < additionalConnections; i++)
            {
                int curEdgeIdx = Random.Range(maxConnections, sortedDistances.Count);
                while (EdgeIntersectsAnotherCell(sortedDistances[curEdgeIdx]))
                {
                    sortedDistances[curEdgeIdx] = sortedDistances[sortedDistances.Count - 1];
                    sortedDistances.RemoveAt(sortedDistances.Count - 1);
                    curEdgeIdx = Random.Range(maxConnections, sortedDistances.Count);
                    if (sortedDistances.Count - maxConnections <= 0)
                        return;
                }

                //Create connection component
                GameObject curCon = new GameObject("Connection");
                curCon.transform.parent = transform;
                LineRenderer lr = curCon.AddComponent<LineRenderer>();
                lr.SetPosition(0, sortedDistances[curEdgeIdx].origin.transform.position);
                lr.SetPosition(1, sortedDistances[curEdgeIdx].destination.transform.position);
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = Color.white;
                lr.endColor = Color.white;
                lr.startWidth = .2f;
                lr.endWidth = .2f;
                lr.sortingOrder = -10;

                //Add connection info top scripts
                int originId = sortedDistances[curEdgeIdx].origin.GetComponent<CellIdentity>().GetId();
                int destinationId = sortedDistances[curEdgeIdx].destination.GetComponent<CellIdentity>().GetId();
                sortedDistances[curEdgeIdx].origin.GetComponent<CellIdentity>().AddConnection(destinationId, sortedDistances[curEdgeIdx].destination, 0, lr, true);
                sortedDistances[curEdgeIdx].destination.GetComponent<CellIdentity>().AddConnection(originId, sortedDistances[curEdgeIdx].origin, 1, lr, false);

                //Enlargen current network
                if (curNetwork.ContainsKey(originId))
                    curNetwork[originId].Add(destinationId);
                else
                    curNetwork.Add(originId, new List<int>() { destinationId });

                if (curNetwork.ContainsKey(destinationId))
                    curNetwork[destinationId].Add(originId);
                else
                    curNetwork.Add(destinationId, new List<int>() { originId });
            }
        }
    }

    //DoesEdgeFormLoop: shows if adding the 
    private bool EdgeFormsLoop(Edge edje, Dictionary<int, List<int>> network)
    {
        int originId = edje.origin.GetComponent<CellIdentity>().GetId();
        int destinationId = edje.destination.GetComponent<CellIdentity>().GetId();
        int reachedCells = 0;

        HashSet<int> visitedId = new HashSet<int>();
        Stack<int> curPath = new Stack<int>();
        if (network.ContainsKey(originId))
            curPath.Push(originId);
        else
            return false;

        while (reachedCells < network.Count)
        {
            List<int> connections = network[curPath.Pop()];
            foreach (int cellId in connections)
            {
                if (cellId == destinationId)
                    return true;
                if (!visitedId.Contains(cellId))
                {
                    visitedId.Add(cellId);
                    curPath.Push(cellId);
                    reachedCells++;
                }
            }
            if (curPath.Count == 0)
                return false;
        }
        return false;

    }

    //EdgeIntersectsAnotherCell: TRUE if connection overlaps something other than origin or destination
    bool EdgeIntersectsAnotherCell(Edge edge)
    {
        edge.origin.SetActive(false);
        RaycastHit2D hit = Physics2D.Raycast(edge.origin.transform.position, edge.destination.transform.position - edge.origin.transform.position);
        edge.origin.SetActive(true);

        if (hit && hit.collider.gameObject == edge.destination)
            return false;

        return true;
    }

    //TODO: FIX DOESNT RETURN SORTED EDJES
    private List<Edge> MergeSortEdges(List<Edge> curEdges)
    {
        if (curEdges.Count <= 5) //cutOff point to simpler sorting algorithm
        {
            for (int i = 0; i < curEdges.Count; i++)
            {
                int curLowIdx = i;
                float curLowDistance = 100000;

                for (int x = i; x < curEdges.Count; x++)
                {
                    if (curEdges[x].distance < curLowDistance)
                    {
                        curLowDistance = curEdges[x].distance;
                        curLowIdx = x;
                    }
                }
                if (curLowIdx == i)
                {
                    continue;
                }
                else
                {
                    Edge curEdge = curEdges[i];
                    curEdges[i] = curEdges[curLowIdx];
                    curEdges[curLowIdx] = curEdge;
                }
            }
            return curEdges;
        }
        else
        {
            int halfIndex = curEdges.Count / 2;
            List<Edge> leftEdges = new List<Edge>();
            for (int i = 0; i < halfIndex; i++)
                leftEdges.Add(curEdges[i]);
            List<Edge> rightEdges = new List<Edge>();
            for (int i = halfIndex; i < curEdges.Count; i++)
                rightEdges.Add(curEdges[i]);

            leftEdges = MergeSortEdges(leftEdges);
            rightEdges = MergeSortEdges(rightEdges);

            //Combine solutions
            List<Edge> resultSorted = new List<Edge>();
            int depletedLeft = 0;
            int depletedRight = 0;
            while (leftEdges.Count > depletedLeft && rightEdges.Count > depletedRight)
            {
                if (leftEdges[depletedLeft].distance < rightEdges[depletedRight].distance)
                {
                    resultSorted.Add(leftEdges[depletedLeft]);
                    depletedLeft++;
                }
                else
                {
                    resultSorted.Add(rightEdges[depletedRight]);
                    depletedRight++;
                }
            }

            //Add remaining edges
            if (leftEdges.Count > depletedLeft)
                for (int i = depletedLeft; i < leftEdges.Count; i++)
                    resultSorted.Add(leftEdges[i]);
            if (rightEdges.Count > depletedRight)
                for (int i = depletedRight; i < rightEdges.Count; i++)
                    resultSorted.Add(rightEdges[i]);

            return resultSorted;
        }
    }
}
