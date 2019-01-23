using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CellIdentity : MonoBehaviour
{
    [Header("Cell Modifiers")]
    public int attackCapacity;
    public int defenceCapacity;
    public int generationCapacity;
    public GameObject attackUnit;

    // Main Scripts References
    TurnUIController turnController;
    PlayerManager playerManager;

    [Header("Component References")]
    public Animator animator;
    public SpriteRenderer mainSprite;
    public List<SpriteRenderer> arrowSprites = new List<SpriteRenderer>(); //0 - up, 1 - down, 2 - left, 3 - right
    public TMP_Text reserveIndicator;
    private TMP_Text textComp;

    [Header("GameObject References")]
    public List<GameObject> arrows = new List<GameObject>();

    //Instance Variables
    // - general
    private int id;
    private int owner = -1;
    private int originalOwner = -1;
    // - unit info
    private int curOccupancy = -1;
    private int unitCapacity = 0;
    private int reserveUnits;
    // - connection info
    private List<int> connections = new List<int>();
    private List<GameObject> connectionCells = new List<GameObject>();
    private List<int> connectionDirections = new List<int>();
    private List<LineRenderer> connectionLines = new List<LineRenderer>();
    private List<bool> connectionStartsHere = new List<bool>();
    private List<bool> isAttackingConnection = new List<bool>();
    private int latestUnitSentTo = -1;
    private List<int[]> outgoingAttacks = new List<int[]>();
    // - cell condition
    private bool isAttacking = false;

    ///[CONSTRUCTOR(S)]
    public void Awake()
    {
        playerManager = GameObject.FindGameObjectWithTag("playerManager").GetComponent<PlayerManager>();
        turnController = GameObject.FindGameObjectWithTag("turnController").GetComponent<TurnUIController>();
        textComp = gameObject.GetComponent<TMP_Text>();
    }
    public void Construct(int id, int unitCapacity)
    {
        this.id = id;                                                               // SET id of cell
        this.unitCapacity = unitCapacity;                                           // SET capacity of cell
        curOccupancy = Random.Range(0, (int)(unitCapacity / 2));                    // SET occupancy of cell

        Vector3 curScale = new Vector3(1 + (unitCapacity * .05f), 1 + (unitCapacity * .05f), 1);
        transform.localScale = curScale;                             // SET size of sprite

        UpdateCellLabel();
    }
    public void Reconstruct(int id, CellIdentity cellInfo)
    {
        this.id = id;
        this.curOccupancy = cellInfo.GetCurOccupancy();
        this.unitCapacity = cellInfo.GetCapacity();

        UpdateCellLabel();
    }


    ///[CONNECTION INTERACTION]
    //Adds a connection and all associated information to parallel lists
    public void AddConnection(int otherId, GameObject otherObj, int direction, LineRenderer line, bool isStart)
    {
        connections.Add(otherId);
        connectionDirections.Add(direction);
        connectionLines.Add(line);
        connectionStartsHere.Add(isStart);
        outgoingAttacks.Add(new int[] { otherId, 0, -1, 0 });
        connectionCells.Add(otherObj);
        isAttackingConnection.Add(false);
    }

    //Switches the setting to attack a certain cell or not
    public void SwitchAttackOption(int otherId)
    {
        int otherIdx = GetOtherIdIndex(otherId);
        if (otherIdx != -1)
        {
            // Set attack direction as enabled/disabled
            isAttackingConnection[otherIdx] = !isAttackingConnection[otherIdx];

            // Determine if current cell is attacking anything
            bool curIsAttacking = false;
            foreach (bool enabled in isAttackingConnection)
            {
                if (enabled)
                {
                    curIsAttacking = true;
                    break;
                }
            }

            // Subscribe/unsubsribe according to new state
            if (!isAttacking && curIsAttacking)
                turnController.completeFighting += CompleteCellFighting;
            else if(isAttacking && !curIsAttacking)
                turnController.completeFighting -= CompleteCellFighting;

            //Update logic flag
            isAttacking = curIsAttacking;

        }
    }

    //ResetAllOutConnectionColor: Resets the color of outgoing connection ends
    private void ResetAllOutConnectionColor()
    {
        for (int i = 0; i < isAttackingConnection.Count; i++)
        {
            if (isAttackingConnection[i])
            {
                if (connectionStartsHere[i])
                    connectionLines[i].startColor = Color.blue;
                else
                    connectionLines[i].endColor = Color.blue;
            }
            else
            {
                if (connectionStartsHere[i])
                    connectionLines[i].startColor = Color.white;
                else
                    connectionLines[i].endColor = Color.white;
            }
        }
    }

    //SetBulkConnectionColor: Sets the initial and final connection line colors (when an origin is selected and when deselected)
    private void SetBulkConnectionColor(bool isOriginActivated)
    {
        //Set start color (sides closest to origin)
        for (int i = 0; i < isAttackingConnection.Count; i++)
        {
            if (connectionStartsHere[i])
            {
                if (isAttackingConnection[i])
                {
                    if (isOriginActivated)
                        connectionLines[i].startColor = Color.green;
                    else
                        connectionLines[i].startColor = mainSprite.color;
                }
                else
                {
                    if (isOriginActivated)
                        connectionLines[i].startColor = Color.red;
                    else
                        connectionLines[i].startColor = Color.white;
                }
            }
            else
            {
                if (isAttackingConnection[i])
                {
                    if (isOriginActivated)
                        connectionLines[i].endColor = Color.green;
                    else
                        connectionLines[i].endColor = mainSprite.color;
                }
                else
                {
                    if (isOriginActivated)
                        connectionLines[i].endColor = Color.red;
                    else
                        connectionLines[i].endColor = Color.white;
                }
            }
        }

        //Set end color (sides farthest from origin)
        for (int g = 0; g < isAttackingConnection.Count; g++)
        {
            if (isOriginActivated)
            {
                if (connectionStartsHere[g])
                {
                    if (isAttackingConnection[g])
                        connectionLines[g].endColor = Color.green;
                    else
                        connectionLines[g].endColor = Color.red;
                }
                else
                {
                    if (isAttackingConnection[g])
                        connectionLines[g].startColor = Color.green;
                    else
                        connectionLines[g].startColor = Color.red;
                }
            }
            else
            {
                Color otherSideColor = Color.white;
                if (connectionCells[g].GetComponent<CellIdentity>().IsAttacking(id))
                    otherSideColor = connectionCells[g].GetComponent<CellIdentity>().mainSprite.color; 

                if (connectionStartsHere[g])
                    connectionLines[g].endColor = otherSideColor;
                else
                    connectionLines[g].startColor = otherSideColor;
            }
        }
    }


    ///[CELL UI UPDATORS]
    //UpdateCellLabel: Updates the main cell text
    void UpdateCellLabel()
    {
        textComp.text = curOccupancy + "/" + unitCapacity;
    }

    //UpdateReserveIndicator: Updates the reserve cell text
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

    ///[ANIMATIONS]
    //ChangeActivationState: Updates state of cell animations
    public void ChangeActivationState(bool isActivated, bool isOrigin, int type)
    {
        animator.SetBool("isActivated", isActivated);
        animator.SetBool("isOrigin", isOrigin);

        if (isActivated)
            animator.SetInteger("type", type);
        else
            animator.SetInteger("type", 0);

        if (isOrigin)
        {
            for (int i = 0; i < connectionCells.Count; i++)
            {
                if (isAttackingConnection[i])
                    connectionCells[i].GetComponent<CellIdentity>().ChangeActivationState(isActivated, false, 1);
                else
                    connectionCells[i].GetComponent<CellIdentity>().ChangeActivationState(isActivated, false, 2);
            }

            SetBulkConnectionColor(isActivated);
        }
    }

    //SwitchActivationColor: Switches cell background type red/green
    public void SwitchActivationColor(int otherId)
    {
        int otherIdx = GetOtherIdIndex(otherId);
        if (otherIdx != -1)
        {
            //Find the correct color to change to based on wether the attack direction is enabled
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
            {
                connectionLines[otherIdx].startColor = nextColor;
                connectionLines[otherIdx].endColor = nextColor;
            }
            else
            {
                connectionLines[otherIdx].startColor = nextColor;
                connectionLines[otherIdx].endColor = nextColor;
            }
        }
    }


    ///[CELL FIGHTING]
    //CompleteCellFighting: Initiates a unit attack of the correct size -> finishes attack stage once no attack units left
    public void CompleteCellFighting()
    {
        bool isUnitSendingDone = true;
        for (int h = 0; h < outgoingAttacks.Count; h++)
        {
            if (outgoingAttacks[h][1] > 0) // If has units to send (SEND UNITS)
            {
                //Test for size of unit
                if (outgoingAttacks[h][1] / 50 > 0) // Size of 50
                {
                    int unitAmount = outgoingAttacks[h][1] / 50;
                    SendUnit(h, 50);
                }
                if (outgoingAttacks[h][1] / 25 > 0) // Size of 25
                {
                    int unitAmount = outgoingAttacks[h][1] / 25;
                    SendUnit(h, 25);
                }
                else if (outgoingAttacks[h][1] / 15 > 0) // 15
                {
                    int unitAmount = outgoingAttacks[h][1] / 15;
                    SendUnit(h, 15);
                }
                else if (outgoingAttacks[h][1] / 5 > 0) // 5
                {
                    int unitAmount = outgoingAttacks[h][1] / 5;
                    SendUnit(h, 5);
                }
                else
                    SendUnit(h, 1); // 1

                isUnitSendingDone = false;
            }
        }

        if (isUnitSendingDone)
        {
            turnController.completeFighting -= CompleteCellFighting;
            turnController.postTurnRecalculation += PostTurnRecalculation;
        }
    }

    //SendUnit: Creates and animates a unit of certain size
    void SendUnit(int otherIdx, int unitSize)
    {
        curOccupancy -= unitSize;
        GameObject curAttackUnit = Instantiate(attackUnit, transform.position, transform.rotation, transform);
        curAttackUnit.GetComponent<LerpAtStart>().Construct(transform.position, connectionCells[otherIdx].transform.position, 2f);
        curAttackUnit.GetComponent<UnitAttackIdentity>().Construct(gameObject, unitSize, owner, mainSprite.color);
        curAttackUnit.GetComponent<SpriteRenderer>().color = mainSprite.color;
        outgoingAttacks[otherIdx][1] -= unitSize;
        UpdateCellLabel();
    }


    ///[CELL TURN ACTIONS]
    //CompleteTurn: performs over population and unit generation calculations at the end of each turn
    public void CompleteTurn()
    {
        if (owner != originalOwner)
        {
            turnController.postTurnRecalculation -= PostTurnRecalculation;
            isAttacking = false;
            for (int i = 0; i < isAttackingConnection.Count; i++)
                isAttackingConnection[i] = false;
            ResetAllOutConnectionColor();
        }
        originalOwner = owner;

        //(TODO: IMPLEMENT OVERPOPULATION SYSTEM and NOTIFICATION)
        if (curOccupancy > unitCapacity)
            return;

        //Generate unit(s) if can
        curOccupancy = Mathf.Min(unitCapacity, curOccupancy + generationCapacity);
        UpdateCellLabel();
    }

    //PostTurnRecalculation: performs unit recalculation for those cells that had attacks in the last turn
    public void PostTurnRecalculation()
    {
        RecalculateAttackUnits();

        turnController.completeFighting += CompleteCellFighting;
        isAttacking = true;
    }

    //RecalculateAttackUnits: Resets the amount of units to send to connections based on current cell settings
    //@return - TRUE if attacking something, FALSE if not attacking anything
    public void RecalculateAttackUnits()
    {
        //Find avaliable attack units
        int attackUnitsAvaliable = curOccupancy - reserveUnits;

        //Find how much attack directions there are
        int directions = 0;
        foreach (bool enabled in isAttackingConnection)
            if (enabled)
                directions++;

        //No outgoing units if avalible cells is zero or no attack directions
        if (attackUnitsAvaliable <= 0 || directions == 0)
        {
            for (int i = 0; i < outgoingAttacks.Count; i++)
            {
                SetOutgoingAttack(i, 0);
            }
        }
        else //Otherwise find the amount of attack units to send in each direction
        {

            int attackUnitsPerDirection = attackUnitsAvaliable / directions;

            //Add attack units to the enabled attack connections
            for (int i = 0; i < outgoingAttacks.Count; i++)
            {
                if (isAttackingConnection[i])
                    outgoingAttacks[i][1] = attackUnitsPerDirection;
                else
                    outgoingAttacks[i][1] = 0;
            }

            //Debug
            string output = "ATTACK UNITS FOR CELL - " + gameObject.name + System.Environment.NewLine;
            for (int h = 0; h < outgoingAttacks.Count; h++)
            {
                output += "Attack to " + outgoingAttacks[h][0] + " with " + outgoingAttacks[h][1] + " units" + System.Environment.NewLine;
            }
            print(output);
        }
    }


    ///[UTILITY]
    //GetOtherIdIndex: Get connection array index of another neighbor cell by its id (-1 if not a connected cell)
    private int GetOtherIdIndex(int otherId)
    {
        for (int i = 0; i < connections.Count; i++)
            if (otherId == connections[i])
                return i;
        return -1;
    }

    //IsConnectedTo: IsConnectedToReturns TRUE if the id of another cell is connected to current cell
    public bool IsConnectedTo(int otherId)
    {
        for (int i = 0; i < connections.Count; i++)
            if (connections[i] == otherId)
                return true;

        return false;
    }

    //IsAttacking: returns TRUE if current cell is attacking the other cell with given id
    public bool IsAttacking(int otherId)
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (outgoingAttacks[i][0] == otherId)
            {
                if (outgoingAttacks[i][1] > 0)
                    return true;
                else
                {
                    return false;
                }
            }
        }

        return false;
    }


    ///[ACCESSORS/MUTATORS]
    public int GetId() { return id; }
    public int GetOwner() { return owner; }
    public int GetCurOccupancy() { return curOccupancy; }
    public int GetCapacity() { return unitCapacity; }
    public int GetReserveUnits() { return reserveUnits; }

    public void SetOwner(int playerId, Color color)
    {
        if (owner == playerId && mainSprite.color == color)
            return;

        List<Player> players = playerManager.GetPlayers();

        foreach (Player p in players)
        {
            if (p.GetId() == owner)
                p.RemoveCell(gameObject);
            else if (p.GetId() == playerId)
                p.AddCell(gameObject);
        }

        owner = playerId;
        mainSprite.color = color;
        foreach (SpriteRenderer sr in arrowSprites)
            sr.color = color;
    }
    public void SetOriginalOwner(int originalOwner) { this.originalOwner = originalOwner; }
    public void SetReserveUnits(int reserveUnits) { this.reserveUnits = reserveUnits; }
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
    public void SetOccupancy(int newOccupancy)
    {
        curOccupancy = newOccupancy;
        UpdateCellLabel();
    }
    public void ChangeUnits(int delta, int owner, Color color)
    {
        curOccupancy += delta;
        if (curOccupancy < 0)
        {
            SetOwner(owner, color);
            curOccupancy = -curOccupancy;
        }
        UpdateCellLabel();
    }
}
