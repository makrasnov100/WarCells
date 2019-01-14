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
    public delegate void CompleteOwnershipAnim();
    public CompleteOwnershipAnim completeOwnershipAnim;

    //References
    public PlayerManager playerManager;
    public Image nextTurnBG;
    public Slider unitAmount;
    public TMP_Text unitCurrent;
    public TMP_Text unitMax;
    public GameObject nextTurnBtn;

    public void NextTurnClick()
    {
        //if (completeFighting != null)
        //{
        //    completeFighting();
        //}
        //completeFighting = null;

        StartCoroutine(UnitSendingAnim());

        //playerManager.CompleteCellActions(); //Finds influence amount and amount of units each person can generate
        //nextTurnBG.color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f, .4f, .4f);
    }

    public void ShowAttackUI(bool showAttack)
    {
        //TODO: Add nice animation and graphics
        if (showAttack)
        {
            nextTurnBG.gameObject.SetActive(false);
            nextTurnBtn.SetActive(false);
            unitAmount.gameObject.SetActive(true);
            unitCurrent.gameObject.SetActive(true);
            unitMax.gameObject.SetActive(true);
        }
        else
        {
            nextTurnBtn.SetActive(true);
            nextTurnBG.gameObject.SetActive(true);
            unitAmount.gameObject.SetActive(false);
            unitCurrent.gameObject.SetActive(false);
            unitMax.gameObject.SetActive(false);
        }
    }

    public void ChangeMaxUnitText(int unitsUsed, int maxUnits)
    {
        unitCurrent.text = "" + unitsUsed;
        unitMax.text = "/" + maxUnits;
        unitAmount.maxValue = maxUnits;
        unitAmount.value = unitsUsed;
    }

    public bool UnitTransferInfo(CellIdentity origin, CellIdentity destination, out int[] outInfo, out int[] inInfo)
    {
        int unitInput = (int)unitAmount.value;

        outInfo = new int[] {destination.GetId(), unitInput};
        inInfo = new int[] {origin.GetId(), unitInput, origin.GetOwner(), origin.GetAttackCapacity()};
        origin.SetSingleOpenConnectionColor(destination.GetId(), Color.blue);
        origin.SetSingleOpenConnectionColor(origin.GetId(), Color.blue);
        return true;
    }

    IEnumerator UnitSendingAnim()
    {
        while (completeFighting != null)
        {
            completeFighting();
            yield return new WaitForSeconds(.3f);
        }

        if (completeOwnershipAnim != null)
        {
            completeOwnershipAnim();
        }

        playerManager.CompleteCellActions(); //Finds influence amount and amount of units each person can generate
        nextTurnBG.color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f, .4f, .4f);

    }
}
