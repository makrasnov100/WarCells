using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    public TurnUIController turnManager;
    public bool ProcessTurn(HashSet<GameObject> ownedCells)
    {
        foreach (GameObject cell in ownedCells)// For each cell
        {
            //Get all of its connections
            List<GameObject> connectedCells = cell.GetComponent<CellIdentity>().GetConnections();
            //Testfor the best cell to attack
            int smallestCellId = -1;
            int smallestCellOcc = int.MaxValue;
            foreach (GameObject conCell in connectedCells) //Try and find cells not already owned by itself
            {
                CellIdentity cci = conCell.GetComponent<CellIdentity>();
                //If cell is already owned - skip
                if (cci.GetOwner() == cell.GetComponent<CellIdentity>().GetOwner())
                    continue;
                if (cci.GetOccupancy() < smallestCellOcc)
                {
                    smallestCellId = cci.GetId();
                    smallestCellOcc = cci.GetOccupancy();
                }
            }
            //If for some reason no empty cells are connected, find the cell with the least amount of units
            if(smallestCellId == -1)
            {
                foreach (GameObject conCell in connectedCells) //Try and find cells not already owned by itself
                {
                    CellIdentity cci = conCell.GetComponent<CellIdentity>();
                    if (cci.GetOccupancy() < smallestCellOcc)
                    {
                        smallestCellId = cci.GetId();
                        smallestCellOcc = cci.GetOccupancy();
                    }
                }
            }
            
            // Setup attack on the found smallest cell
            CellIdentity cci2 = cell.GetComponent<CellIdentity>();
            cci2.ResetOutgoingAttacks();
            cci2.SwitchAttackOption(smallestCellId);
            cci2.RecalculateAttackUnits();
        }

        //Ends bots turn
        return true;
    }


}
