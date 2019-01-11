using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnUIController : MonoBehaviour
{
    //Fighting Delegate HUGE TODO: make private 
    public delegate void CompleteFighting();
    public CompleteFighting completeFighting;

    //References
    public PlayerManager playerManager;
    public Image nextTurnBG;
    public TMP_InputField unitAmount;
    public TMP_Text unitMax;
    public GameObject nextTurnBtn;
    public GameObject confirmAttackBtn;

    public void NextTurnClick()
    {
        if (completeFighting != null)
        {
            completeFighting();
        }
        completeFighting = null;

        playerManager.CompleteCellActions(); //Finds influence amount and amount of units each person can generate
        nextTurnBG.color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f, .4f, .4f);
    }

    public void ShowAttackUI(bool showAttack)
    {
        //TODO: Add nice animation and graphics
        if (showAttack)
        {
            nextTurnBtn.SetActive(false);
            confirmAttackBtn.SetActive(true);
            unitAmount.gameObject.SetActive(true);
            unitMax.gameObject.SetActive(true);
        }
        else
        {
            nextTurnBtn.SetActive(true);
            confirmAttackBtn.SetActive(false);
            unitAmount.gameObject.SetActive(false);
            unitMax.gameObject.SetActive(false);
        }
    }

    public void ChangeMaxUnitText(int maxUnits)
    {
        unitMax.text = "/" + maxUnits;
    }

    public bool UnitTransferInfo(CellIdentity origin, CellIdentity destination, out int[] outInfo, out int[] inInfo)
    {

        //Find how much units can be assigned
        int avaliableUnits = origin.GetCurUnusedUnits(destination.GetId());
        //Find how much units is too much for destination cell
        int maxDestinationUnits = origin.GetCapacity();

        int unitInput;
        System.Int32.TryParse(unitAmount.text, out unitInput);

        if (unitInput > avaliableUnits || unitInput < 0)
        {
            outInfo = new int[] {-1,-1};
            inInfo = new int[] {-1,-1};

            //TODO: notify users too many units was selected
            print("Too many (or negative) units selected for attack!");

            return false;
        }

        outInfo = new int[] {destination.GetId(), unitInput};
        inInfo = new int[] {origin.GetId(), unitInput, origin.GetOwner(), origin.GetAttackCapacity()};
        origin.SetSingleOpenConnectionColor(destination.GetId(), Color.blue);
        origin.SetSingleOpenConnectionColor(origin.GetId(), Color.blue);
        return true;
    }
}
