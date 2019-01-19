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
    [Header("MobileUI")]
    public Canvas mobileCanvas;
    public Image mobileNextTurnBG;
    public Slider mobileUnitAmountInput;
    public RectTransform mobileUnitAmountRect;
    public Image mobileUnitInputBG;
    public TMP_Text mobileUnitCurrent;
    public TMP_Text mobileUnitMax;
    public GameObject mobileNextTurnBtn;

    [Header("PcUI")]
    public Canvas pcCanvas;
    public Image pcNextTurnBG;
    public Slider pcUnitAmountInput;
    public RectTransform pcUnitAmountRect;
    public Image pcUnitInputBG;
    public TMP_Text pcUnitCurrent;
    public TMP_Text pcUnitMax;
    public GameObject pcNextTurnBtn;

    public PlayerManager playerManager;
    private Canvas canvas;
    private Image nextTurnBG;
    private Slider unitAmountInput;
    private RectTransform unitAmountRect;
    private Image unitInputBG;
    private TMP_Text unitCurrent;
    private TMP_Text unitMax;
    private GameObject nextTurnBtn;

    public bool ignoreSliderEdits = false;

    public void Start()
    {
        //Sets UI based off the platform.
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
        {
            //Phone
            canvas = mobileCanvas;
            nextTurnBG = mobileNextTurnBG;
            unitAmountInput = mobileUnitAmountInput;
            unitAmountRect = mobileUnitAmountRect;
            unitInputBG = mobileUnitInputBG;
            unitCurrent = mobileUnitCurrent;
            unitMax = mobileUnitMax;
            nextTurnBtn = mobileNextTurnBtn;
        }
        else
        {
            //PC
            canvas = pcCanvas;
            nextTurnBG = pcNextTurnBG;
            unitAmountInput = pcUnitAmountInput;
            unitAmountRect = pcUnitAmountRect;
            unitInputBG = pcUnitInputBG;
            unitCurrent = pcUnitCurrent;
            unitMax = pcUnitMax;
            nextTurnBtn = pcNextTurnBtn;
        }

        canvas.gameObject.SetActive(true);


}

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

    //Getters/Setters
    //Unit amount input
    public Slider getUnitAmountInput()
    {
        return unitAmountInput;
    }
    public void setUnitAmountInput(Slider input)
    {
        this.unitAmountInput = input;
    }
    //unit current
    public TMP_Text getUnitCurrent()
    {
        return unitCurrent;
    }
    public void setUnitCurrent(TMP_Text input)
    {
        this.unitCurrent = input;
    }
}
