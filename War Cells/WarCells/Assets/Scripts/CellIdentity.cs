﻿using System.Collections;
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
    private List<int[]> incomingAttacks = new List<int[]>();
    private List<int> animUnitsLeft = new List<int>();
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
        transform.Find("Sprite").localScale = curScale;                             // SET size of sprite
        transform.Find("ActivationBG").Find("Sprite").localScale = curScale;        // SET size of activation sprite

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
        incomingAttacks.Add(new int[] { otherId, 0, -1, 0 });
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
                {
                    connectionCells[i].GetComponent<CellIdentity>().ChangeActivationState(isActivated, false, 1);
                }
                else
                {
                    connectionCells[i].GetComponent<CellIdentity>().ChangeActivationState(isActivated, false, 2);
                }
            }
        }
    }

    //SwitchActivationColor: Switches cell background type red/green
    public void SwitchActivationColor(int otherId)
    {
        int otherIdx = GetOtherIdIndex(otherId);
        if (otherIdx != -1)
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
                connectionLines[otherIdx].startColor = nextColor;
            else
                connectionLines[otherIdx].startColor = nextColor;
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
                if (outgoingAttacks[h][1] / 25 > 0) // Size of 25
                {
                    int unitAmount = outgoingAttacks[h][1] / 25;
                    SendUnit(h, 25);
                }
                else if (outgoingAttacks[h][1] / 15 > 0)
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

        if (isUnitSendingDone)
        {
            isAttacking = false;
            animUnitsLeft.Clear();
            turnController.completeFighting -= CompleteCellFighting;
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
        //(TODO: IMPLEMENT OVERPOPULATION SYSTEM and NOTIFICATION)
        if (curOccupancy > unitCapacity)
        {
            return;
        }

        //Generate unit(s) if can
        curOccupancy = Mathf.Min(unitCapacity, curOccupancy + generationCapacity);
        UpdateCellLabel();
        RecalculateAttackUnits();
    }

    //RecalculateAttackUnits: Resets the amount of units to send to connections based on current cell settings
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
                if (enabled)
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


    ///[ACCESSORS/MUTATORS]
    public int GetId() { return id; }
    public int GetOwner() { return owner; }
    public int GetCapacity() { return unitCapacity; }
    public int GetReserveUnits() { return reserveUnits; }

    public void SetOwner(int id, Color color)
    {
        List<Player> players = playerManager.GetPlayers();

        foreach (Player p in players)
        {
            if (p.GetId() == owner)
                p.RemoveCell(gameObject);
            else if (p.GetId() == id)
                p.AddCell(gameObject);
        }

        owner = id;
        mainSprite.color = color;
        foreach (SpriteRenderer sr in arrowSprites)
        {
            sr.color = color;
        }
    }
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
