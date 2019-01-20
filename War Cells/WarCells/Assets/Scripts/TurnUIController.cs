using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnUIController : MonoBehaviour
{
    //Fighting Delegate (HUGE TODO: make private )
    public delegate void CompleteFighting();
    public CompleteFighting completeFighting;

    //Script References
    public PlayerManager playerManager;
    public AttackController attackController;

    //UI References
    [Header("Mobile UI")]
    public Canvas mobileCanvas;
    public Image mobileNextTurnBG;
    public Slider mobileUnitAmountInput;
    public Image mobileUnitInputBG;
    public TMP_Text mobileUnitCurrent;
    public TMP_Text mobileUnitMax;
    public GameObject mobileNextTurnBtn;

    [Header("PC UI")]
    public Canvas pcCanvas;
    public Image pcNextTurnBG;
    public Slider pcUnitAmountInput;
    public Image pcUnitInputBG;
    public TMP_Text pcUnitCurrent;
    public TMP_Text pcUnitMax;
    public GameObject pcNextTurnBtn;

    private Canvas canvas;
    private Image nextTurnBG;
    private Slider unitAmountInput;
    private Image unitInputBG;
    private TMP_Text unitCurrent;
    private TMP_Text unitMax;
    private GameObject nextTurnBtn;

    //Instance variables (used for correct logic)
    public bool ignoreSliderEdits = false;  //TRUE - when setting slider value by script | FALSE - otherwise
    public bool isAnimationPlaying = false; //TRUE - while unit sending animation is playing | FALSE - when the animation is fully complete


    ///[UNITY DEFAULT]
    public void Start()
    {
        //Sets correct UI refernces based of current platform
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
        {
            //Mobile - Touch Input
            canvas = mobileCanvas;
            nextTurnBG = mobileNextTurnBG;
            unitAmountInput = mobileUnitAmountInput;
            unitInputBG = mobileUnitInputBG;
            unitCurrent = mobileUnitCurrent;
            unitMax = mobileUnitMax;
            nextTurnBtn = mobileNextTurnBtn;
        }
        else
        {
            //PC - Mouse Input
            canvas = pcCanvas;
            nextTurnBG = pcNextTurnBG;
            unitAmountInput = pcUnitAmountInput;
            unitInputBG = pcUnitInputBG;
            unitCurrent = pcUnitCurrent;
            unitMax = pcUnitMax;
            nextTurnBtn = pcNextTurnBtn;
        }

        canvas.gameObject.SetActive(true);
    }

    ///[UI UPDATES]
    //Hides all UI components (used when animation is playing)
    public void HideUI()
    {
        //Enable/Disable correct UI components
        nextTurnBtn.SetActive(false);
        nextTurnBG.gameObject.SetActive(false);
        unitAmountInput.gameObject.SetActive(false);
        unitCurrent.gameObject.SetActive(false);
        unitMax.gameObject.SetActive(false);
    }

    //Shows only the UI components that are needed to initiate the next turn / hand over to new user
    public void ShowNextTurnUI()
    {
        if (isAnimationPlaying)
            return;

        //Enable/Disable correct UI components
        nextTurnBtn.SetActive(true);
        nextTurnBG.gameObject.SetActive(true);
        unitAmountInput.gameObject.SetActive(false);
        unitCurrent.gameObject.SetActive(false);
        unitMax.gameObject.SetActive(false);
    }

    //Shows only the UI neccessary to set reserve units and sets correct slider values
    public void ShowDefenceUI(CellIdentity origin)
    {
        if (isAnimationPlaying)
            return;

        ignoreSliderEdits = true;  // > START slider ignore flag

        unitInputBG.color = new Color(0, 0, 1f, 150f / 250f);       //SET Defence Slider Color
        unitCurrent.text = "" + origin.GetReserveUnits();           //SET Current Units in Reserve Text
        unitMax.text = "/" + origin.GetCapacity();                  //SET Max avaliable reserve units
        unitAmountInput.maxValue = origin.GetCapacity();            //SET Max slider value
        unitAmountInput.value = (float)origin.GetReserveUnits();    //SET Slider position to current value

        //Enable/Disable correct UI components
        nextTurnBG.gameObject.SetActive(false);
        nextTurnBtn.SetActive(false);
        unitAmountInput.gameObject.SetActive(true);
        unitCurrent.gameObject.SetActive(true);
        unitMax.gameObject.SetActive(true);

        ignoreSliderEdits = false; // > END slider ignore flag
    }


    ///[UI INPUTS]
    //Performs the next turn click functions
    public void NextTurnClick()
    {
        HideUI();
        StartCoroutine(CellAttackAnimation());
    }


    ///[FIGHTING SYSTEM]
    //CellAttackAnimation: Coroutine starting unit sending animations and completing end turn procedures for each owned cell
    IEnumerator CellAttackAnimation()
    {
        isAnimationPlaying = true; // > START animation playing flag

        //Instatiate attacking units for each cell that has remaining units
        while (completeFighting != null)
        {
            completeFighting();
            yield return new WaitForSeconds(.3f);
        }

        //Wait for all cells to finish reaching destination
        yield return new WaitForSeconds(2f);

        //Generate units for all cells with owners and recalculate next attack units
        playerManager.CompleteCellActions();
        nextTurnBG.color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f, .4f, .4f);

        isAnimationPlaying = false; // > END animation playing flag

        //Show the correct UI after turn is complete
        if (attackController.GetOriginCell() != null) //An origin cell is selected
            ShowDefenceUI(attackController.GetOriginCell().GetComponent<CellIdentity>());
        else                                          //No orign cell selected
            ShowNextTurnUI();
    }


    ///[ACCESORS/MUTATORS]
    public Slider GetUnitAmountInput() { return unitAmountInput; }
    public TMP_Text GetUnitCurrent() { return unitCurrent; }
}
