using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuAnimControl : MonoBehaviour
{
    //Button (GameObject) References
    public GameObject play;
    public GameObject options;
    public GameObject quit;

    //Play Menu
    public GameObject playMenu;
    public GameObject playLocalButton;
    public GameObject playWebButton;
    public GameObject playOptionOneButton;
    public GameObject playOptionTwoButton;
    public GameObject playOptionThreeButton;

    //Options Menu
    public GameObject optionsMenuCanvas;

    //Tutorial Related
    public GameObject tutorialObject;

    //Music Related
    public GameObject musicPlayer;

    public List<GameObject> LocalComponents;
    public List<GameObject> WebComponents;
    public GameObject oldMenu;
    public GameObject curInputField;


    //Support UI
    // - main
    public LineRenderer lineRenderer;

    //General Settings
    public Color menuColor;

    //Instance variables
    bool isLocationSelected = true;
    public bool isLocal = true;

    public string menuLast = "NA";

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer.positionCount = 4;
        lineRenderer.startWidth = .2f;
        lineRenderer.endWidth = .2f;
        lineRenderer.sortingOrder = -3;
        lineRenderer.startColor = menuColor;
        lineRenderer.endColor = menuColor;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        //Main Menu Components Setups
        play.GetComponent<SpriteRenderer>().color = menuColor;
        options.GetComponent<SpriteRenderer>().color = menuColor;
        quit.GetComponent<SpriteRenderer>().color = menuColor;

        play.GetComponent<LerpInRadius>().Construct(.4f, 8f, 10f);
        options.GetComponent<LerpInRadius>().Construct(.4f, 8f, 10f);
        quit.GetComponent<LerpInRadius>().Construct(.4f, 8f, 10f);

        play.GetComponent<MenuButtonAnim>().buttonClick += PlayButtonSelect;
        options.GetComponent<MenuButtonAnim>().buttonClick += OptionsButtonSelect;
        quit.GetComponent<MenuButtonAnim>().buttonClick += QuitButtonSelect;
        playLocalButton.GetComponent<MenuHeaderAnim>().buttonClick += PlayLocalSelect;
        playWebButton.GetComponent<MenuHeaderAnim>().buttonClick += PlayWebSelect;
        playOptionOneButton.GetComponent<MenuButtonAnim>().buttonClick += PlayOptionOneSelect;
        playOptionTwoButton.GetComponent<MenuButtonAnim>().buttonClick += PlayOptionTwoSelect;
        playOptionThreeButton.GetComponent<MenuButtonAnim>().buttonClick += PlayOptionThreeSelect;
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(0, play.transform.position);
        lineRenderer.SetPosition(1, options.transform.position);
        lineRenderer.SetPosition(2, quit.transform.position);
        lineRenderer.SetPosition(3, play.transform.position);

        //Call Apropriate function based on platform type
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
            HandleTouch();
        else
            HandleMouse();
    }

    void HandleTouch()
    {
        if (Input.touchCount >= 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleInput(touch.position);
            }
        }
    }
    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0)) //Record mouse position on initial click
            HandleInput(Input.mousePosition);
    }

    //HandleInput: handles a single press in the menu
    void HandleInput(Vector3 pressPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(pressPos);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit)
        {
            if (hit.collider.gameObject.tag == "UIButton")
                hit.collider.gameObject.GetComponent<MenuButtonAnim>().ClickedOn();
            else if (hit.collider.gameObject.tag == "UIHeader")
                hit.collider.gameObject.GetComponent<MenuHeaderAnim>().ClickedOn();
            else if (hit.collider.gameObject.tag == "UIInputField")
            {
                if (hit.collider.gameObject != curInputField)
                {
                    if (curInputField != null)
                    {
                        curInputField.GetComponent<EditText>().SwitchEdit();
                    }
                    curInputField = hit.collider.gameObject;
                    hit.collider.gameObject.GetComponent<EditText>().SwitchEdit();
                }
            }
        }
    }


    //Button Events
    private void PlayButtonSelect(bool isEnabled)
    {
        if (menuLast == "Play")
            menuLast = "NA";
        else if (menuLast == "NA")
            menuLast = "Play";
        else
            return;

        Animator anim = playMenu.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("isActivated", isEnabled);

        if (isEnabled)
        {
            if (isLocal)
                playLocalButton.GetComponent<MenuHeaderAnim>().ClickedOn();
            else
                playWebButton.GetComponent<MenuHeaderAnim>().ClickedOn();
        }
        else
        {
            if (isLocal)
                playLocalButton.GetComponent<MenuHeaderAnim>().Unselect();
            else
                playWebButton.GetComponent<MenuHeaderAnim>().Unselect();
        }
    }
    private void OptionsButtonSelect(bool isEnabled)
    {
        if (menuLast == "Options")
            menuLast = "NA";
        else if (menuLast == "NA")
            menuLast = "Options";
        else
            return;

        OptionsManager.Instance.ShowMenu();

    }
    private void QuitButtonSelect(bool isEnabled)
    {
        if (menuLast != "NA")
        {
            return;
        }

        Application.Quit();
    }

    private void PlayLocalSelect(bool isEnabled)
    {
        Animator anim = playMenu.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("isLocal", true);

        playWebButton.GetComponent<MenuHeaderAnim>().Unselect();
        if(!isLocal)
        {
            isLocal = true;
            LocalVsWebQuickDisable();
        }
    }
    private void PlayWebSelect(bool isEnabled)
    {
        Animator anim = playMenu.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("isLocal", false);

        playLocalButton.GetComponent<MenuHeaderAnim>().Unselect();
        if (isLocal)
        {
            isLocal = false;
            LocalVsWebQuickDisable();
        }
    }

    private void PlayOptionOneSelect(bool isEnabled)
    {
        if (isLocal)
        {
            SceneManager.LoadScene("LocalPlaySettings");
        }
    }

    private void PlayOptionTwoSelect(bool isEnabled)
    {
        if (isLocal)
        {
            SceneManager.LoadScene("LocalPlaySettings");
        }
    }

    private void PlayOptionThreeSelect(bool isEnabled)
    {
        if (isLocal)
        {
            tutorialObject.SetActive(true);
        }
    }

    //Disable UI Components
    public void LocalVsWebQuickDisable()
    {
        foreach (GameObject g in WebComponents)
            g.SetActive(!isLocal);
        foreach (GameObject g in LocalComponents)
            g.SetActive(isLocal);
    }

    public void DisableTutorial()
    {
        tutorialObject.SetActive(false);
    }
    public void DisableOptionsMenu()
    {
        optionsMenuCanvas.SetActive(false);
    }

    //Cleanup
    private void OnDestroy()
    {
        play.GetComponent<MenuButtonAnim>().buttonClick -= PlayButtonSelect;
        options.GetComponent<MenuButtonAnim>().buttonClick -= OptionsButtonSelect;
        quit.GetComponent<MenuButtonAnim>().buttonClick -= QuitButtonSelect;
    }
}
