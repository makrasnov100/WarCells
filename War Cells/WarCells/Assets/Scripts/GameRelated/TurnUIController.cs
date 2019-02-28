using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TurnUIController : MonoBehaviour
{
    //Fighting Delegate (HUGE TODO: make private )
    public delegate void CompleteFighting();
    public CompleteFighting completeFighting;
    public delegate void PostTurnRecalculation();
    public CompleteFighting postTurnRecalculation;
    //Script References
    public PlayerManager playerManager;
    public AttackController attackController;
    public BotManager botManager;

    //UI References
    [Header("Mobile UI")]
    public Canvas mobileCanvas;
    public Image mobileNextTurnBG;
    public Slider mobileUnitAmountInput;
    public Image mobileUnitInputBG;
    public TMP_Text mobileUnitCurrent;
    public TMP_Text mobileUnitMax;
    public GameObject mobileNextTurnBtn;
    public GameObject mobileNextPlayerBtn;

    [Header("PC UI")]
    public Canvas pcCanvas;
    public Image pcNextTurnBG;
    public Slider pcUnitAmountInput;
    public Image pcUnitInputBG;
    public TMP_Text pcUnitCurrent;
    public TMP_Text pcUnitMax;
    public GameObject pcNextTurnBtn;
    public GameObject pcNextPlayerBtn;

    [Header("Winner UI")]
    public GameObject winnerMenuUI;
    public GameObject winnerUnitText;
    public GameObject winnerColor;

    [Header("InfluenceBar")]
    public GameObject influenceBarCanvas;
    public Image influenceBarImage;
    private List<Image> curBar = new List<Image>();

    private Canvas canvas;
    private Image nextTurnBG;
    private Slider unitAmountInput;
    private Image unitInputBG;
    private TMP_Text unitCurrent;
    private TMP_Text unitMax;
    private GameObject nextTurnBtn;
    private GameObject nextPlayerBtn;

    //Pass and play variables
    private Player curPlayer;
    private bool isDoneSelectingAttacks = false;

    //Instance variables (used for correct logic)
    public bool ignoreSliderEdits = false;  //TRUE - when setting slider value by script | FALSE - otherwise
    public bool isAnimationPlaying = false; //TRUE - while unit sending animation is playing | FALSE - when the animation is fully complete


    ///[UNITY DEFAULT]
    public void Start()
    {
        winnerMenuUI.SetActive(false);
        //Sets correct UI refernces based of current platform
        //if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
        //{
        //    //Mobile - Touch Input
        //    canvas = mobileCanvas;
        //    nextTurnBG = mobileNextTurnBG;
        //    unitAmountInput = mobileUnitAmountInput;
        //    unitInputBG = mobileUnitInputBG;
        //    unitCurrent = mobileUnitCurrent;
        //    unitMax = mobileUnitMax;
        //    nextTurnBtn = mobileNextTurnBtn;
        //    nextPlayerBtn = mobileNextPlayerBtn;
        //}
        //else
        //{
            //PC - Mouse Input
            canvas = pcCanvas;
            nextTurnBG = pcNextTurnBG;
            unitAmountInput = pcUnitAmountInput;
            unitInputBG = pcUnitInputBG;
            unitCurrent = pcUnitCurrent;
            unitMax = pcUnitMax;
            nextTurnBtn = pcNextTurnBtn;
            nextPlayerBtn = pcNextPlayerBtn;
        //}

        canvas.gameObject.SetActive(true);
        curPlayer = playerManager.GetNextAlivePlayer(-1);

        CreateInfluenceBar();
        ShowNextTurnUI();
    }

    ///[UI UPDATES]
    //Hides all UI components (used when animation is playing)
    public void HideUI()
    {
        //Enable/Disable correct UI components
        nextTurnBtn.SetActive(false);
        nextPlayerBtn.SetActive(false);
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

        //Enable/Disable correct general UI components
        nextTurnBG.gameObject.SetActive(true);
        unitAmountInput.gameObject.SetActive(false);
        unitCurrent.gameObject.SetActive(false);
        unitMax.gameObject.SetActive(false);
        //Enable/Disable correct pass and play components
        if (isDoneSelectingAttacks)
        {
            nextTurnBtn.SetActive(true);
            nextPlayerBtn.SetActive(false);
            nextTurnBG.color = Color.red;
            //When every player done test for any humans
            bool hasHumanPlayer = false;
            foreach (Player p in playerManager.GetPlayers())
                if (p.GetOwnedCells().Count > 0 && !p.GetIsBot())
                    hasHumanPlayer = true;

            //If no humans, auto roll next turn.
            if (!hasHumanPlayer)
                NextTurnClick();
        }
        else
        {
            Color pCol = curPlayer.GetPlayerColor();
            nextTurnBG.color = new Color(pCol.r, pCol.g, pCol.b, .8f);
            nextTurnBtn.SetActive(false);
            //Checks if player is a bot
            if (!curPlayer.GetIsBot())
                nextPlayerBtn.SetActive(true);
            else//if bot do bot stuff
            {
                curPlayer.BotMove();
                nextPlayerBtn.SetActive(false);
                NextPlayerClick();
            }
               
        }

    }


    //Shows only the UI neccessary to set reserve units and sets correct slider values
    public void ShowDefenceUI(CellIdentity origin)
    {
        if (isAnimationPlaying)
            return;

        ignoreSliderEdits = true;  // > START slider ignore flag

        unitCurrent.text = "" + origin.GetReserveUnits();           //SET Current Units in Reserve Text
        unitMax.text = "/" + origin.GetCapacity();                  //SET Max avaliable reserve units
        unitAmountInput.maxValue = origin.GetCapacity();            //SET Max slider value
        unitAmountInput.value = (float)origin.GetReserveUnits();    //SET Slider position to current value

        //Enable/Disable correct UI components
        nextTurnBG.gameObject.SetActive(false);
        nextTurnBtn.SetActive(false);
        nextPlayerBtn.SetActive(false);
        unitAmountInput.gameObject.SetActive(true);
        unitCurrent.gameObject.SetActive(true);
        unitMax.gameObject.SetActive(true);

        ignoreSliderEdits = false; // > END slider ignore flag
    }

    public void CreateInfluenceBar()
    {
        //Delete previous turns bar
        if(curBar != null && curBar.Count != 0)
            foreach (Image bar in curBar)
            {
                Destroy(bar.gameObject);
            }

        //Set vars
        List<Image> newBar = new List<Image>();
        float influenceBarCurrent = 0;
        int totalUnits = 0;
        //Calculate total units owned by players
        foreach(Player p in playerManager.GetPlayers())//Start with each player
        {
            foreach(GameObject cell in p.ownedCells) // Find owned cells and count unit amounts
            {
                totalUnits += cell.GetComponent<CellIdentity>().GetOccupancy();
            }
        }

        //Generate bar
        int screenWidth = Screen.width;
        foreach(Player p in playerManager.GetPlayers())
        {
            int playerUnits = 0;
            foreach (GameObject cell in p.ownedCells) // Find owned cells and count unit amounts
            {
                playerUnits += cell.GetComponent<CellIdentity>().GetOccupancy();
            }

            float playerPercentage = (float)playerUnits / (float)totalUnits;
            //Get bar with new color for player
            Image playerColorBar = Instantiate(influenceBarImage);
            newBar.Add(playerColorBar);
            playerColorBar.transform.SetParent(influenceBarCanvas.transform);
            playerColorBar.color = p.GetPlayerColor();
            playerColorBar.gameObject.SetActive(true);
            //Resize bar and move it to location
            RectTransform playerColorBarTrans = playerColorBar.GetComponent<RectTransform>();
            playerColorBarTrans.sizeDelta = new Vector2(playerPercentage*screenWidth, 20);
            playerColorBarTrans.anchorMin = new Vector2(0,1);
            playerColorBarTrans.anchorMax = new Vector2(0,1);
            playerColorBarTrans.position = new Vector2(influenceBarCurrent + .5f * playerPercentage * screenWidth, Screen.height);
            influenceBarCurrent += playerPercentage * screenWidth;

        }
        //After each bar is created, upload them curBar
        curBar = newBar;
    }

    ///[UI INPUTS]
    //Performs the next turn click functions
    public void NextTurnClick()
    {
        attackController.ResetCellSelection();
        HideUI();

        StartCoroutine(CellAttackAnimation());
    }

    //Performs the next player click functions
    public void NextPlayerClick()
    {
        attackController.ResetCellSelection();

        //Get the next player to declare attacks
        int curPlayerId = curPlayer.GetId();
        curPlayer = playerManager.GetNextAlivePlayer(curPlayerId);
        int nextPlayerId = curPlayer.GetId();

        //Check if all players already declared attacks
        if (nextPlayerId <= curPlayerId)
        {
            playerManager.ShowAllAttackDeclarations();
            isDoneSelectingAttacks = true;
        }
        if(!isDoneSelectingAttacks)
        {
            playerManager.ShowCurPlayerAttackDeclarations(nextPlayerId);
        }

        ShowNextTurnUI();
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

        //Generate units for all cells with owners
        playerManager.CompleteCellActions();
        nextTurnBG.color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f, .4f, .4f);

        //Recalculate next attack units
        if(postTurnRecalculation != null)
            postTurnRecalculation();
        postTurnRecalculation = null;

        //Set any players without owned cells as dead
        foreach (Player p in playerManager.GetPlayers())
            if (p.GetOwnedCells().Count <= 0)
                p.SetIsDead(true);

        //Notify user about win if the only one left (GAMEOVER)
        if(playerManager.GetTotalAlivePlayers() <= 1)
        {
            winnerMenuUI.SetActive(true);
            //find that winning player
            Player winningPlayer = null;
            foreach (Player p in playerManager.GetPlayers())
            {
                if (p.GetOwnedCells().Count >= 1)
                {
                    winningPlayer = p;
                    break; // once found last player (winner) break out of loop
                }
            }
            int playerUnits = 0;
            foreach (GameObject cell in winningPlayer.ownedCells) // Find owned cells and count unit amounts
            {
                playerUnits += cell.GetComponent<CellIdentity>().GetOccupancy();
            }
            winnerUnitText.GetComponent<TextMeshProUGUI>().text = "Army Count: " + playerUnits;
            winnerColor.GetComponent<Image>().color = winningPlayer.GetPlayerColor();
        }

        //If preselcted cur player was already destroyed choose the next avaliable
        if (curPlayer.GetIsDead())
            curPlayer = playerManager.GetNextAlivePlayer(curPlayer.GetId());
        playerManager.ShowCurPlayerAttackDeclarations(curPlayer.GetId());

        isAnimationPlaying = false; // > END animation playing flag
        isDoneSelectingAttacks = false;

        //Show the correct UI after turn is complete
        CreateInfluenceBar();
        ShowNextTurnUI();
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    ///[ACCESORS/MUTATORS]
    public Slider GetUnitAmountInput() { return unitAmountInput; }
    public TMP_Text GetUnitCurrent() { return unitCurrent; }
    public int GetCurPlayerId() { return curPlayer.GetId(); }
}
