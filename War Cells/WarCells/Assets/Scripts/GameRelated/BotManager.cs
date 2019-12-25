using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    public TurnUIController turnManager;
    public void ProcessTurn(HashSet<GameObject> ownedCells, string botDifficulty)
    {
        //Call the bot difficulty
        if (botDifficulty == "Easy")
            EasyBot();
        if (botDifficulty == "Medium")
            MediumBot();
        if (botDifficulty == "Hard")
            HardBot();


        //Easy difficulty - only searches around intermediate cells for lowest unowned - else sends to lowest owned
        void EasyBot()
        {
            foreach (GameObject cell in ownedCells)// For each cell
            {
                //Get all of its connections
                List<GameObject> connectedCells = cell.GetComponent<CellIdentity>().GetConnections();
                //Testfor the best cell to attack
                int smallestCellId = -1;
                float smallestCellOcc = float.MaxValue;
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
                if (smallestCellId == -1)
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
                CellIdentity curCellIdent = cell.GetComponent<CellIdentity>();
                curCellIdent.ResetOutgoingAttacks();
                curCellIdent.SwitchAttackOption(smallestCellId);
                curCellIdent.RecalculateAttackUnits();
            }
        }

        //Medium - searchest for unowned cells - out 2 cells max
        void MediumBot()
        {

            foreach (GameObject cell in ownedCells)// For each cell
            {
                //Get all of its connections
                List<GameObject> connectedCells = cell.GetComponent<CellIdentity>().GetConnections();
                //Testfor the best cell to attack
                int smallestCellId = -1;
                float smallestCellOcc = float.MaxValue;

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
                //Test for further out - any non-selfowned cells.
                if (smallestCellId == -1)
                {
                    foreach (GameObject conCell in connectedCells) //Try and find cells not already owned by itself
                    {
                        foreach (GameObject conConCell in conCell.GetComponent<CellIdentity>().GetConnections())
                        {
                            CellIdentity cccId = conConCell.GetComponent<CellIdentity>();
                            if (cccId.GetOwner() == cell.GetComponent<CellIdentity>().GetOwner())
                                continue;
                            if (cccId.GetOccupancy() < smallestCellOcc)
                            {
                                smallestCellId = conCell.GetComponent<CellIdentity>().GetId(); // Sets cell that leads to the empty cells
                                smallestCellOcc = cccId.GetOccupancy();
                            }
                        }
                    }
                }
                //If all nearby cells are owned - find nearest, smallest cell
                if (smallestCellId == -1)
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
                CellIdentity curCellIdent = cell.GetComponent<CellIdentity>();
                curCellIdent.ResetOutgoingAttacks();
                curCellIdent.SwitchAttackOption(smallestCellId);
                curCellIdent.RecalculateAttackUnits();
            }
        }

        //Medium - searchest for unowned cells - out 3 cells max
        void HardBot()
        {

            foreach (GameObject cell in ownedCells)// For each cell
            {
                //Get all of its connections
                List<GameObject> connectedCells = cell.GetComponent<CellIdentity>().GetConnections();
                //Testfor the best cell to attack
                int smallestCellId = -1;
                float smallestCellOcc = float.MaxValue;

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
                //Test for further out (2)- any non-selfowned cells.
                if (smallestCellId == -1)
                {
                    foreach (GameObject conCell in connectedCells) //Try and find cells not already owned by itself
                    {
                        foreach (GameObject conConCell in conCell.GetComponent<CellIdentity>().GetConnections())
                        {
                            CellIdentity cccId = conConCell.GetComponent<CellIdentity>();
                            if (cccId.GetOwner() == cell.GetComponent<CellIdentity>().GetOwner())
                                continue;
                            if (cccId.GetOccupancy() < smallestCellOcc)
                            {
                                smallestCellId = conCell.GetComponent<CellIdentity>().GetId(); // Sets cell that leads to the empty cells
                                smallestCellOcc = cccId.GetOccupancy();
                            }
                        }
                    }
                }
                //Test for further out (3)- any non-selfowned cells.
                if (smallestCellId == -1)
                {
                    foreach (GameObject conCell in connectedCells) //Try and find cells not already owned by itself
                    {
                        foreach (GameObject secondSetCell in conCell.GetComponent<CellIdentity>().GetConnections())
                        {
                            foreach (GameObject thirdSetCell in secondSetCell.GetComponent<CellIdentity>().GetConnections())
                            {
                                CellIdentity tsci = thirdSetCell.GetComponent<CellIdentity>();
                                if (tsci.GetOwner() == cell.GetComponent<CellIdentity>().GetOwner())
                                    continue;
                                if (tsci.GetOccupancy() < smallestCellOcc)
                                {
                                    smallestCellId = conCell.GetComponent<CellIdentity>().GetId(); // Sets cell that leads to the empty cells
                                    smallestCellOcc = tsci.GetOccupancy();
                                }
                            }
                        }
                    }
                }
                //If all nearby cells are owned - find nearest, smallest cell
                if (smallestCellId == -1)
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
                CellIdentity curCellIdent = cell.GetComponent<CellIdentity>();
                curCellIdent.ResetOutgoingAttacks();
                curCellIdent.SwitchAttackOption(smallestCellId);
                curCellIdent.RecalculateAttackUnits();
            }
        }
    }
    
}
