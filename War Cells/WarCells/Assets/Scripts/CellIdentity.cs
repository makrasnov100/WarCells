using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CellIdentity : MonoBehaviour
{
    //Built References
    Animator animator;
    TurnUIController turnController;
    public PlayerManager playerManager;
    public SpriteRenderer mainSprite;
    public List<GameObject> arrows = new List<GameObject>(); //0 - up, 1 - down, 2 - left, 3 - right
    public List<SpriteRenderer> arrowSprites = new List<SpriteRenderer>();
    public List<TMP_Text> arrowText = new List<TMP_Text>();
    public TMP_Text reserveIndicator;

    //Predifined Info
    public int attackCapacity;
    public int defenceCapacity;
    public int generationCapacity;
    public GameObject attackUnit;

    //Instance Variables
    private int id;
    private int owner;
    private List<int> connections = new List<int>();
    private List<GameObject> connectionCells = new List<GameObject>();
    private List<int> connectionDirections = new List<int>();
    private List<GameObject> connectionLines = new List<GameObject>();
    private List<bool> connectionStartsHere = new List<bool>();
    private int curOccupancy;
    private int unitCapacity;
    private int reserveUnits;
    private TMP_Text textComp;

    private List<bool> isAttackingConnection = new List<bool>();
    private int latestUnitSentTo = -1;
    private List<int[]> outgoingAttacks = new List<int[]>();
    private List<int[]> incomingAttacks = new List<int[]>();
    private bool isAttacking;
    //private bool isCalculationComplete = false;
    private List<int> animUnitsLeft = new List<int>();

    public void Awake()
    {
        isAttacking = false;
        animator = GetComponent<Animator>();
        playerManager = GameObject.FindGameObjectWithTag("playerManager").GetComponent<PlayerManager>();
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

    public void ChangeUnits(int delta, int owner, Color color)
    {
        curOccupancy += delta;
        if(curOccupancy < 0)
        {
            SetOwner(owner, color);
            curOccupancy = -curOccupancy;
        }
        UpdateCellLabel();
    }

    void UpdateCellLabel()
    {
        textComp.text = curOccupancy + "/" + unitCapacity;
    }
    public void UpdateReserveIndicator(int reserveUnits)
    {
        if (reserveUnits == 0)
            reserveIndicator.gameObject.SetActive(false);
        else
        {
            reserveIndicator.gameObject.SetActive(true);
            reserveIndicator.text = "" + reserveUnits;
        }
    }

    public void AddConnection(int otherId, GameObject otherObj, int direction, GameObject line, bool isStart)
    {
        connections.Add(otherId);
        connectionDirections.Add(direction);
        connectionLines.Add(line);
        connectionStartsHere.Add(isStart);
        outgoingAttacks.Add(new int[] { otherId, 0, -1, 0 });
        incomingAttacks.Add(new int[] { otherId, 0, -1, 0 });
        connectionCells.Add(otherObj);
        isAttackingConnection.Add(false);
    }

    public void CompleteTurn()
    {
        //Cancel all attacks and disable arrows //TODO: unless selected attack to be reoccuring
        /*for (int i = 0; i < outgoingAttacks.Count; i++)
        {
            outgoingAttacks[i][1] = 0;
            //arrows[i].SetActive(false);
        }
        for (int y = 0; y < arrows.Count; y++)
        {
            arrows[y].SetActive(false);
            arrowText[y].text = "0";
        }
        SetAllOpenConnectionColor(Color.yellow);
        */

        //Generate unit(s) if can
        if (curOccupancy > unitCapacity)
        {
            ////TODO: IMPLEMENT OVERPOPULATION SYSTEM and NOTIFICATION
            return;
        }

        curOccupancy -= GetOutboundUnits();
        curOccupancy = Mathf.Min(unitCapacity, curOccupancy + generationCapacity);
        UpdateCellLabel();
        RecalculateAttackUnits();
    }

    public int GetOutboundUnits()
    {
        int outboundUnits = 0;
        foreach (int[] attack in outgoingAttacks)
        {
            outboundUnits += attack[1];
        }
        return outboundUnits;
    }

    public int GetOutboundUnits(int otherId)
    {
        int outboundUnits = 0;
        foreach (int[] attack in outgoingAttacks)
        {
            if (attack[0] == otherId)
            {
                outboundUnits += attack[1];
                break;
            }
        }
        return outboundUnits;
    }

    public void UpdateAttackArrows(int inputAmount, int destinationId)
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i] == destinationId)
            {
                arrowText[connectionDirections[i]].text = inputAmount + "%";
                float newScale = 1f + ((float)inputAmount / 150f);
                arrows[connectionDirections[i]].transform.localScale = new Vector3(newScale, newScale, 1);

                if (inputAmount == 0)
                {
                    arrows[connectionDirections[i]].SetActive(false);
                }
                else
                {
                    arrows[connectionDirections[i]].SetActive(true);
                }

                break;
            }
        }
    }

    public void ChangeActivationState(bool isActivated, bool isOrigin, int type)
    {
        animator.SetBool("isActivated", isActivated);

        if (isActivated)
            animator.SetInteger("type", type);
        else
            animator.SetInteger("type", 0);

        if (isOrigin)
        {
            if (type == 0)
                SetAllOpenConnectionColor(Color.yellow);
            else if (type == 1)
                SetAllOpenConnectionColor(Color.green);

            for(int i = 0; i < connectionCells.Count; i++)
            {
                if (isAttackingConnection[i])
                {
                    connectionCells[i].GetComponent<CellIdentity>().ChangeActivationState(isActivated, false, 1);
                    connectionCells[i].GetComponent<CellIdentity>().SetConnectionColor(id, Color.green);
                }
                else
                {
                    connectionCells[i].GetComponent<CellIdentity>().ChangeActivationState(isActivated, false, 2);
                    connectionCells[i].GetComponent<CellIdentity>().SetConnectionColor(id, Color.red);
                }
            }

            
        }
    }

    public void SwitchActivationColor(int otherId)
    {
        int otherIdx = GetOtherIdIndex(otherId);
        if(otherIdx != -1)
        {
            int type = animator.GetInteger("type");
            Color nextColor;
            if (type == 1)
            {
                nextColor = Color.red;
                animator.SetInteger("type", 2);
            }
            else
            {
                nextColor = Color.green;
                animator.SetInteger("type", 1);
            }

            if (connectionStartsHere[otherIdx])
                connectionLines[otherIdx].GetComponent<LineRenderer>().startColor = nextColor;
            else
                connectionLines[otherIdx].GetComponent<LineRenderer>().startColor = nextColor;
        }
    }

    public void IndicateAttackConnection()
    {
        for (int i = 0; i < isAttackingConnection.Count; i++)
        {
            if (isAttackingConnection[i])
            {
                if (connectionStartsHere[i])
                    connectionLines[i].GetComponent<LineRenderer>().startColor = Color.blue;
                else
                    connectionLines[i].GetComponent<LineRenderer>().endColor = Color.blue;
            }
        }
    }

    private int GetOtherIdIndex(int otherId)
    {
        for (int i = 0; i < connections.Count; i++)
            if (otherId == connections[i])
                return i;
        return -1;
    }

    private void SetConnectionColor(int otherId, Color color)
    {
        int otherIdx = GetOtherIdIndex(otherId);
        if (otherIdx != -1)
        {
            if (connectionStartsHere[otherIdx])
                connectionLines[otherIdx].GetComponent<LineRenderer>().startColor = color;
            else
                connectionLines[otherIdx].GetComponent<LineRenderer>().endColor = color;
        }
    }

    public void SetAllOpenConnectionColor(Color color)
    {
        for (int i = 0; i < isAttackingConnection.Count; i++)
        {
            if (isAttackingConnection[i])
                continue;
            if (connectionStartsHere[i])
                connectionLines[i].GetComponent<LineRenderer>().startColor = color;
            else
                connectionLines[i].GetComponent<LineRenderer>().endColor = color;
        }
    }

    public int GetId()
    {
        return id;
    }
    public void SetOwner(int id, Color color)
    {
        List<Player> players = playerManager.GetPlayers();

        foreach (Player p in players)
        {
            if (p.GetId() == owner)
            {
                p.RemoveCell(gameObject);
            }
            else if (p.GetId() == id)
            {
                p.AddCell(gameObject);
            }
        }

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
    public int GetReserveUnits()
    {
        return reserveUnits;
    }

    public void SetReserveUnits(int reserveUnits)
    {
        this.reserveUnits = reserveUnits;
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
                    //curOccupancy -= attackInfo[1] - outgoingAttacks[i][1];
                    outgoingAttacks[i][1] = attackInfo[1];
                    UpdateCellLabel();
                }
            }


            //Determine if cell is attacked
            bool isAttackingCur = false;
            for (int i = 0; i < connections.Count; i++)
            {
                if (outgoingAttacks[i][1] > 0)
                {
                    isAttackingCur = true;
                    break;
                }
            }

            //Determine if need to subscribe/unsubscribe based on given info
            if (!isAttacking && isAttackingCur)
            {
                turnController.completeFighting += CompleteCellFighting;
            }
            else if (isAttacking && !isAttackingCur)
            {
                turnController.completeFighting -= CompleteCellFighting;
            }
            isAttacking = isAttackingCur;

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
        }
    }

    public void SwitchAttackOption(int otherId)
    {
        int otherIdx = GetOtherIdIndex(otherId);
        if (otherIdx != -1)
            isAttackingConnection[otherIdx] = !isAttackingConnection[otherIdx];
    }

    public void RecalculateAttackUnits()
    {
        int attackUnitsAvaliable = curOccupancy - reserveUnits;
        //Find how much attack directions there are
        int directions = 0;
        foreach (bool enabled in isAttackingConnection)
            if (enabled)
                directions++;

        if (attackUnitsAvaliable <= 0 || directions == 0)
        {
            for (int i = 0; i < outgoingAttacks.Count; i++)
            {
                SetOutgoingAttack(i, 0);
                //connectionCells[i].GetComponent<CellIdentity>().SetIncomingAttack(id, 0, owner, attackCapacity);
            }
        }
        else
        {
            List<int> curAttackUnits = new List<int>();

            int attackUnitsPerDirection = attackUnitsAvaliable / directions;

            foreach (bool enabled in isAttackingConnection)
            {
                if (enabled)
                    curAttackUnits.Add(attackUnitsPerDirection);
                else
                    curAttackUnits.Add(0);
            }

            string output = "ATTACK UNITS FOR CELL - " + gameObject.name + System.Environment.NewLine;
            for (int h = 0; h < curAttackUnits.Count; h++)
            {
                output += "Attack to " + outgoingAttacks[h][0] + " with " + curAttackUnits[h] + " units" + System.Environment.NewLine;
            }
            print(output);

            for (int i = 0; i < curAttackUnits.Count; i++)
            {
                SetOutgoingAttack(i, curAttackUnits[i]);
                //connectionCells[i].GetComponent<CellIdentity>().SetIncomingAttack(id, curAttackUnits[i], owner, attackCapacity);
            }
        }
    }

    public void SetOutgoingAttack(int otherIdx, int unitAmount)
    {
        outgoingAttacks[otherIdx][1] = unitAmount;

        int outgoingUnits = 0;
        for (int i = 0; i < connections.Count; i++)
        {
            outgoingUnits += outgoingAttacks[i][1];
            if (outgoingUnits > 0)
                break;
        }

        if (!isAttacking && outgoingUnits > 0)
        {
            turnController.completeFighting += CompleteCellFighting;
            isAttacking = true;
        }
        else if (isAttacking && outgoingUnits == 0)
        {
            turnController.completeFighting -= CompleteCellFighting;
            isAttacking = false; 
        }
    }

    public void CompleteCellFighting()
    {
        /*
        if (!isCalculationComplete)
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

            //Debug
            string output = gameObject.name + " - OWNERSHIP CHANCES:" + System.Environment.NewLine;
            for (int u = 0; u < ownershipChances.Count; u++)
            {
                output += "Player: " + ownershipChances[u][0] + " | Units: " + ownershipChances[u][1] + System.Environment.NewLine;
            }
            output += "Total Units Fighting - " + totalUnits + System.Environment.NewLine;

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
                int curRandom = Random.Range(0, ownershipChances.Count);
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

            //Find final unit value (without modifiers)
            int unitsReg = 0;
            int unitsMod = 0;
            foreach (int[] attack in incomingAttacks)
            {
                if (attack[2] == ownershipChances[0][0])
                {
                    unitsReg += attack[1];
                    unitsMod += attack[1] * attack[3];
                }
                animUnitsLeft.Add(attack[1]); //Add units regularly
            }
            if (owner == ownershipChances[0][0])
            {
                unitsReg += curOccupancy;
                unitsMod += curOccupancy * defenceCapacity;
            }
            float actualUnitToModUnitRatio = (float)unitsReg / (float)unitsMod;
            int finalUnitAmount = (int)Mathf.Round(ownershipChances[0][1] * actualUnitToModUnitRatio);

            //print("CELL OWNER TRANSFER - original:" + owner + " to new:" + ownershipChances[0][0]);

            if (owner != ownershipChances[0][0])
            {
                if (owner != -1)
                {
                    Player pastOwner = turnController.playerManager.GetPlayers()[owner];
                    pastOwner.RemoveCell(this.gameObject);
                }
                Player curOwner = turnController.playerManager.GetPlayers()[ownershipChances[0][0]];
                curOwner.AddCell(this.gameObject);
                SetOwner(ownershipChances[0][0], curOwner.GetColor());
            }
            SetOccupancy(finalUnitAmount);
            isCalculationComplete = true;
            output += "Winning Player is " + ownershipChances[0][0] + " with " + finalUnitAmount + " units remaining!" + System.Environment.NewLine;
            output += "Final units determined with ratio of " + actualUnitToModUnitRatio + " | " + ownershipChances[0][1] + " became " + finalUnitAmount;
            print(output);
        }
        */

        bool isUnitSendingDone = true;     
        for(int h = 0; h < outgoingAttacks.Count; h++)
        {
            if (outgoingAttacks[h][1] > 0) // If has units to send (SEND UNITS)
            {
                //Test for size of unit
                if (outgoingAttacks[h][1] / 25 > 0) // Size of 25
                {
                    int unitAmount = outgoingAttacks[h][1] / 25;
                    SendUnit(h, 25);
                }
                else if(outgoingAttacks[h][1] / 15 > 0)
                {
                    int unitAmount = outgoingAttacks[h][1] / 15;
                    SendUnit(h, 15);
                }
                else if (outgoingAttacks[h][1] / 5 > 0)
                {
                    int unitAmount = outgoingAttacks[h][1] / 5;
                    SendUnit(h, 5);
                }
                else
                    SendUnit(h, 1);

                isUnitSendingDone = false;
            }
        }

        void SendUnit(int h, int unitSize)
        {
            curOccupancy -= unitSize;
            GameObject curAttackUnit = Instantiate(attackUnit, transform.position, transform.rotation, transform);
            curAttackUnit.GetComponent<LerpAtStart>().Construct(transform.position, connectionCells[h].transform.position, 2f);
            curAttackUnit.GetComponent<UnitAttackIdentity>().Construct(gameObject, unitSize, owner, mainSprite.color); 
            curAttackUnit.GetComponent<SpriteRenderer>().color = mainSprite.color;
            outgoingAttacks[h][1] -= unitSize;
            UpdateCellLabel();
        }



        if (isUnitSendingDone)
        {
            isAttacking = false; //TODO HUGE: add cell to fighting cells if has reocurring attacks
            //isCalculationComplete = false;
            animUnitsLeft.Clear();
            turnController.completeFighting -= CompleteCellFighting;
        }

        //TODO: make into a black screen with squares showing chances of winning and the cells that is being fought for
    }
}
