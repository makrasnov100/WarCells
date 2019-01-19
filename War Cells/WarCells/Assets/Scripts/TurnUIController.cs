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
    public Slider unitAmountInput;
    public RectTransform unitAmountRect;
    public Image unitInputBG;
    public TMP_Text unitCurrent;
    public TMP_Text unitMax;
    public GameObject nextTurnBtn;


    public bool ignoreSliderEdits = false;

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

    public void HideUI()
    {
        nextTurnBtn.SetActive(true);
        nextTurnBG.gameObject.SetActive(true);
        unitAmountInput.gameObject.SetActive(false);
        unitCurrent.gameObject.SetActive(false);
        unitMax.gameObject.SetActive(false);
    }

    public void ShowDefenceUI(CellIdentity origin)
    {
        ignoreSliderEdits = true;
        unitInputBG.color = new Color(0, 0, 1f, 150f / 250f);

        unitCurrent.text = "" + origin.GetReserveUnits();
        unitMax.text = "/" + origin.GetCapacity();
        unitAmountInput.value = (float)origin.GetReserveUnits();
        unitAmountInput.maxValue = origin.GetCapacity();
        unitAmountRect.offsetMax = new Vector2(0, unitAmountRect.offsetMax.y);

        nextTurnBG.gameObject.SetActive(false);
        nextTurnBtn.SetActive(false);
        unitAmountInput.gameObject.SetActive(true);
        unitCurrent.gameObject.SetActive(true);
        unitMax.gameObject.SetActive(true);
        ignoreSliderEdits = false;
    }

    public void ChangeMaxUnitText(int unitsUsed, int maxUnits)
    {
        unitCurrent.text = "" + unitsUsed;
        unitMax.text = "/" + maxUnits;
        unitAmountInput.maxValue = maxUnits;
        unitAmountInput.value = unitsUsed;
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
