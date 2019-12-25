using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitAttackIdentity : MonoBehaviour
{
    //Instance Variables
    private GameObject baseCell;
    private float unitsRemaining;
    private int unitOwner;
    private Color unitColor;
    public TMP_Text unitSizeText;
    public float unitPower;

    ///[CONSTRUCTOR]
    public void Construct(GameObject baseCell, int unitRemaining, int unitOwner, Color unitColor)
    {
        this.baseCell = baseCell;
        this.unitsRemaining = unitRemaining;
        this.unitOwner = unitOwner;
        this.unitColor = unitColor;
        unitSizeText.text = unitsRemaining.ToString();
    }

    ///[UNIT FIGHTING ACTIONS]
    //OnCollisionEnter2D: determines what the unit collided with and what impact to make
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Collided object doesnt exist or collided with origin cell -> ignore colision
        if (collision.gameObject == null || collision.gameObject == baseCell)
            return;

        if (collision.gameObject.tag == "Cell") //Collided with a cell
        {
            //Get the owner of the other cell
            CellIdentity ci = collision.gameObject.GetComponent<CellIdentity>();
            int cellOwner = ci.GetOwner();

            if (cellOwner == unitOwner)         // - Cell owner thesame
            {
                ci.ChangeUnits(unitsRemaining, 1, unitOwner, unitColor);
                Destroy(gameObject);
            }
            else                                // - Cell owner different
            {
                ci.ChangeUnits(-unitsRemaining, unitPower, unitOwner, unitColor);
                Destroy(gameObject);
            }
        }
        else if(collision.gameObject.tag == "Unit") //Collided with a unit
        {
            UnitAttackIdentity uai = collision.gameObject.GetComponent<UnitAttackIdentity>();

            if (uai.GetUnitOwner() == unitOwner)    // - Ignore other unit if thesame owner
            {
                return;
            }
            else                                    // - Fight the other if differnt owner
            {
                float thisUnitR = GetUnitsRemaining();
                float thisUnitP = GetUnitsPower();
                float thatUnitR = uai.GetUnitsRemaining();
                float thatUnitP = uai.GetUnitsPower();
                uai.UnitFight(thisUnitR, thisUnitP); // Run UnitFight on other unit (OurUnits remaining)
                UnitFight(thatUnitR, thatUnitP); // Run UnitFight on this Unit (Their units remaining)
            }
        }
    }
    //UnitFight: completes the unit vs unit fighting procedure
    public void UnitFight(float incomingUnits, float incomingPower)
    {
        if (incomingUnits > 0)
            unitsRemaining -= incomingUnits * incomingPower;
        //Destroy unit container if no remaining units after fight
        if (unitsRemaining <= 0)
        {
            Destroy(gameObject);
        }
        //Update unit amount remaining if container survived
        unitSizeText.text = unitsRemaining.ToString();
    }

    ///[ACCESSOR(S)/MODIFIER(S)]
    public int GetUnitOwner() { return unitOwner; }
    public float GetUnitsRemaining() { return unitsRemaining;}
    public float GetUnitsPower() { return unitPower; }
}

