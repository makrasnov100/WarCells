using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CellIdentity : MonoBehaviour
{
    //Built References
    Animator animator;
    TurnUIController turnController;
    public SpriteRenderer mainSprite;
    public List<GameObject> arrows = new List<GameObject>(); //0 - up, 1 - down, 2 - left, 3 - right
    public List<SpriteRenderer> arrowSprites = new List<SpriteRenderer>();
    public List<TMP_Text> arrowText = new List<TMP_Text>();

    //Predifined Info
    public int attackCapacity;
    public int defenceCapacity;
    public int generationCapacity;

    //General Info
    private int id;
    private int owner;
    private List<int> connections = new List<int>();
    private List<int> connectionDirections = new List<int>();
    private List<GameObject> connectionLines = new List<GameObject>();
    private List<bool> connectionStartsHere = new List<bool>();
    private int curOccupancy;
    private int unitCapacity;
    private TMP_Text textComp;

    private List<int[]> outgoingAttacks = new List<int[]>();
    private List<int[]> incomingAttacks = new List<int[]>();
    private bool isAttacked;

    public void Awake()
    {
        isAttacked = false;
        animator = GetComponent<Animator>();
    }

    public void Construct(int id, TurnUIController turnController)
    {
        this.id = id;
        owner = -1;
        curOccupancy = 0;
        textComp = gameObject.GetComponent<TMP_Text>();
        this.turnController = turnController;
    }

    public void SetUnitCapacity(int size)
    {
        Vector3 curScale = new Vector3(1 + (size * .05f), 1 + (size * .05f), 1);
        transform.Find("Sprite").localScale = curScale;
        transform.Find("ActivationBG").Find("Sprite").localScale = curScale;
        unitCapacity = size;
        curOccupancy = Random.Range(0, (int)(size / 2));
        UpdateCellLabel();
    }

    public void SetOccupancy(int occupied)
    {
        curOccupancy = occupied;
        UpdateCellLabel();
    }

    void UpdateCellLabel()
    {
        textComp.text = curOccupancy + "/" + unitCapacity;
    }

    public void AddConnection(int otherId, int direction, GameObject line, bool isStart)
    {
        connections.Add(otherId);
        connectionDirections.Add(direction);
        connectionLines.Add(line);
        connectionStartsHere.Add(isStart);
        outgoingAttacks.Add(new int[] {otherId, 0, -1, 0});
        incomingAttacks.Add(new int[] {otherId, 0, -1, 0});
    }

    public void CompleteTurn()
    {
        //Generate unit(s) if can
        curOccupancy = Mathf.Min(unitCapacity, curOccupancy + generationCapacity);
        UpdateCellLabel();
    }

    public void ChangeActivationState(bool isActivated, int type)
    {
        animator.SetBool("isActivated", isActivated);
        animator.SetInteger("type", type);

        if (type == 1)
        {
            SetAllOpenConnectionColor(Color.green);
        }
        else if (type == 0)
        {
            SetAllOpenConnectionColor(Color.yellow);
        }
    }

    public void SetAllOpenConnectionColor(Color color)
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (outgoingAttacks[i][1] > 0)
                continue;
            if (connectionStartsHere[i])
                connectionLines[i].GetComponent<LineRenderer>().startColor = color;
            else
                connectionLines[i].GetComponent<LineRenderer>().endColor = color;
        }
    }

    public void SetSingleOpenConnectionColor(int otherCellId, Color color)
    {
        int conIdx = -1;
        for (int i = 0; i < connections.Count; i++)
        {
            if (otherCellId == connections[i])
            {
                if (outgoingAttacks[i][1] > 0)
                    break;
                conIdx = i;
                break;
            }
        }

        if (conIdx == -1) //Origin not found
            return;

        if (connectionStartsHere[conIdx])
        {
            connectionLines[conIdx].GetComponent<LineRenderer>().startColor = color;
        }
        else
        {
            connectionLines[conIdx].GetComponent<LineRenderer>().endColor = color;
        }
    }

    public int GetId()
    {
        return id;
    }
    public void SetOwner(int id, Color color)
    {
        owner = id;
        mainSprite.color = color;
        foreach (SpriteRenderer sr in arrowSprites)
        {
            sr.color = color;
        }
    }
    public int GetOwner()
    {
        return owner;
    }
    public int GetCurUnits()
    {
        return curOccupancy;
    }
    public int GetCurUnusedUnits(int ignoreCellId)
    {
        int recycledUnits = 0;
        for (int i = 0; i < outgoingAttacks.Count; i++)
        {
            if (outgoingAttacks[i][0] == ignoreCellId)
                recycledUnits = outgoingAttacks[i][1];
        }

        return curOccupancy + recycledUnits;
    }
    public int GetCapacity()
    {
        return unitCapacity;
    }
    public int GetAttackCapacity()
    {
        return attackCapacity;
    }

    public List<int> GetConnections()
    {
        return connections;
    }
    public bool IsConnectedTo(int otherId)
    {
        for (int i = 0; i < connections.Count; i++)
            if (connections[i] == otherId)
                return true;

        return false;
    }

    public void AddAttack(int[] attackInfo, bool isOutgoing)
    {
        if (isOutgoing)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (attackInfo[0] == outgoingAttacks[i][0])
                {
                    if (attackInfo[1] == 0)
                        arrows[connectionDirections[i]].SetActive(false);
                    else
                        arrows[connectionDirections[i]].SetActive(true);

                    arrowText[connectionDirections[i]].text = "" + attackInfo[1];
                    curOccupancy -= attackInfo[1] - outgoingAttacks[i][1];
                    outgoingAttacks[i][1] = attackInfo[1];
                    UpdateCellLabel();
                }
            }

        }
        else
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (attackInfo[0] == incomingAttacks[i][0])
                {
                    incomingAttacks[i] = attackInfo;
                }
            }

            //Determine if cell is attacked
            bool isAttackedCur = false;
            for (int i = 0; i < connections.Count; i++)
            {
                if (incomingAttacks[i][1] > 0)
                {
                    isAttackedCur = true;
                    break;
                }
            }

            //Determine if need to subscribe/unsubscribe based on given info
            if (!isAttacked && isAttackedCur)
            {
                turnController.completeFighting += CompleteCellFighting;
            }
            else if (isAttacked && !isAttackedCur)
            {
                turnController.completeFighting -= CompleteCellFighting;
            }
            isAttacked = isAttackedCur;
        }
    }

    public void CompleteCellFighting()
    {
        //TODO: find more efficint method than O(n^2)
        //TODO: is the below list garbage collected?
        List<int[]> ownershipChances = new List<int[]>(); // 0 - ownerID  1 - unit weight
                                                          //incomingAttacks  // 0 - otherCellID 1 - unit amount  2 - ownerID  3 - modifier 
        int totalUnits = 0;

        //Add the cell defences
        ownershipChances.Add(new int[] { owner, defenceCapacity * curOccupancy });
        totalUnits = ownershipChances[0][1];

        for (int i = 0; i < incomingAttacks.Count; i++)
        {
            bool isPresent = false;
            for (int x = 0; x < ownershipChances.Count; x++)
            {
                if (ownershipChances[x][0] == incomingAttacks[i][2])
                {
                    ownershipChances[x][1] += incomingAttacks[i][1] * incomingAttacks[i][3];
                    totalUnits += incomingAttacks[i][1] * incomingAttacks[i][3];
                    isPresent = true;
                    break;
                }
            }
            if (isPresent)
                continue;
            else
            {
                ownershipChances.Add(new int[] { incomingAttacks[i][2], incomingAttacks[i][1] * incomingAttacks[i][3] });
                totalUnits += incomingAttacks[i][1] * incomingAttacks[i][3];
            }

        }

        //Find who won and with how many units
        for (int b = 0; b < ownershipChances.Count; b++)
        {
            if (ownershipChances[b][1] <= 0)
            {
                ownershipChances.RemoveAt(b);
                b--;
            }
        }
        while (ownershipChances.Count != 1)
        {
            //Find who won the match
            int curRandom = Random.Range(0,ownershipChances.Count);
            for (int g = 0; g < ownershipChances.Count; g++)
            {
                if (g != curRandom)
                {
                    ownershipChances[g][1]--;
                }
            }

            //Remove all contendors with no troops left
            for (int j = 0; j < ownershipChances.Count; j++)
            {
                if (ownershipChances[j][1] <= 0)
                {
                    ownershipChances.RemoveAt(j);
                    j--;
                }
            }
        }

        if (ownershipChances[0][0] == -1)
        {
            SetOccupancy(ownershipChances[0][0]);
        }


        //HUGE TODO ADD REMOWAL OF CELL FROM PAST OWNER 
        //DO TOMOROW!!!!!!
        Player curPlayer = turnController.playerManager.GetPlayers()[ownershipChances[0][0]];
        SetOwner(ownershipChances[0][0], curPlayer.GetColor());
        //curPlayer.AddCell() COMPLETE TOMOROW
        SetOccupancy(ownershipChances[0][1]);
        //ALSO FIGURE OUT A WAY TO RESET ALL ARROWS AND CONNECTION COLORS AND ATTACK LISTS (EFFICIENCTLY)

        //Figure out who won (legacy - efficient but doesnt give remaining units)
        //int winningUnit = Random.Range(1, totalUnits+1);
        //int playerCount = 0;
        //while ((winningUnit - ownershipChances[playerCount][1]) >= 0)
        //{
        //    winningUnit -= ownershipChances[playerCount][1];
        //    playerCount++;
        //}
        //int winningPlayer = ownershipChances[playerCount][0];

        //TODO: make into a black screen with squares showing chances of winning and the cells that is being fought for

        string output = "OWNERSHIP CHANCES:" + System.Environment.NewLine;
        for (int u = 0; u < ownershipChances.Count; u++)
        {
            output += "Player: " + ownershipChances[u][0] + " | Units: " + ownershipChances[u][1] + System.Environment.NewLine;
        }
        output += "Total Units Fighting - " + totalUnits + System.Environment.NewLine;
        output += "Winning Player is " + ownershipChances[0][0] + " with " + ownershipChances[0][1] + " units remaining!";
        print(output);
        print("Completed Fighting In Cell:" + gameObject.name);
    }
}
