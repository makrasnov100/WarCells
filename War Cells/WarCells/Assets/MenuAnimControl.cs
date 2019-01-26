using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public GameObject playCampaignButton;
    public GameObject playPassNPlayButton;
    public GameObject playProgramingButoon;
    public GameObject playCreateGameButton;
    public GameObject playJoinLobbyButton;
    public GameObject playRandomMatchButton;
    public GameObject optionsMenu;


    //Support UI
    // - main
    public LineRenderer lineRenderer;

    //General Settings
    public Color menuColor;

    //Instance variables
    bool isLocationSelected = true;
    bool isLocal = true;

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
        }
    }


    //Button Events
    private void PlayButtonSelect(bool isEnabled)
    {
        Animator anim = playMenu.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("isActivated", isEnabled);

        if (isEnabled)
        {
            if (playLocalButton)
                playLocalButton.GetComponent<MenuHeaderAnim>().ClickedOn();
            else
                playWebButton.GetComponent<MenuHeaderAnim>().ClickedOn();
        }
        else
        {
            if (playLocalButton)
                playLocalButton.GetComponent<MenuHeaderAnim>().Unselect();
            else
                playWebButton.GetComponent<MenuHeaderAnim>().Unselect();
        }

    }
    private void OptionsButtonSelect(bool isEnabled)
    {
        Animator anim = optionsMenu.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("isActivated", isEnabled);
    }
    private void QuitButtonSelect(bool isEnabled)
    {
        Application.Quit();
    }

    private void PlayLocalSelect(bool isEnabled)
    {
        Animator anim = playMenu.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("isLocal", true);

        playLocalButton.GetComponent<MenuHeaderAnim>().Unselect();
    }
    private void PlayWebSelect(bool isEnabled)
    {
        Animator anim = playMenu.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("isLocal", false);

        playLocalButton.GetComponent<MenuHeaderAnim>().Unselect();
    }

    //Cleanup
    private void OnDestroy()
    {
        play.GetComponent<MenuButtonAnim>().buttonClick -= PlayButtonSelect;
        options.GetComponent<MenuButtonAnim>().buttonClick -= OptionsButtonSelect;
        quit.GetComponent<MenuButtonAnim>().buttonClick -= QuitButtonSelect;
    }
}
