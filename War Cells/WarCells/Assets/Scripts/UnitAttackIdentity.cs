using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttackIdentity : MonoBehaviour
{
    private GameObject baseCell;
    private int unitsRemaining;
    private int unitOwner;
    private Color unitColor;
    private int unitType; //(TODO: Defender/Invaders)


    public void Construct(GameObject baseCell, int unitRemaining, int unitOwner, Color unitColor)
    {
        this.baseCell = baseCell;
        this.unitsRemaining = unitRemaining;
        this.unitOwner = unitOwner;
        this.unitColor = unitColor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == null || collision.gameObject == baseCell)
            return;

        if (collision.gameObject.tag == "Cell") // if unit hits a cell
        {
            CellIdentity ci = collision.gameObject.GetComponent<CellIdentity>();
            int cellOwner = ci.GetOwner();
            if (cellOwner == unitOwner) // if cell is same team, add it to the cells total.
            {
                ci.ChangeUnits(unitsRemaining , unitOwner, unitColor);
                Destroy(gameObject);
            }
            else // else, attack a cell with units remaining
            {
                ci.ChangeUnits(-unitsRemaining, unitOwner, unitColor);
                Destroy(gameObject);
            }
        }
        else if(collision.gameObject.tag == "Unit") // if unit hits another unit
        {
            UnitAttackIdentity uai = collision.gameObject.GetComponent<UnitAttackIdentity>();
            if (uai.GetUnitOwner() == unitOwner)
                return;
            else
            {
                //TODO Reintroduce larger armies
                Destroy(uai.gameObject);
                Destroy(gameObject);
            }
        }
    }

    public void UnitFight(int incomingUnits)
    {
        unitsRemaining -= incomingUnits;
        if(unitsRemaining <= 0)
        {
            Destroy(gameObject);
        }
    }

    //Accessor(s)/Modifier(s)
    public int GetUnitOwner()
    {
        return unitOwner;
    }

    public int GetUnitsRemaining()
    {
        return unitsRemaining;
    }
}

