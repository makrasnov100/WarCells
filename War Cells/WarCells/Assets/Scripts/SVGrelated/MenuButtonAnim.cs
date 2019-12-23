using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Animator))]
public class MenuButtonAnim : MonoBehaviour
{
    public delegate void delagatebuttonClicked(bool isEnabled);
    public delagatebuttonClicked buttonClick;

    Animator anim;
    bool isHover;
    public bool isClicked;
    public string btnName;
    public bool isRootBtn;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnMouseEnter()
    {
        isHover = true;
        anim.SetBool("isHowered", isHover);
    }
    private void OnMouseExit()
    {
        isHover = false;
        anim.SetBool("isHowered", isHover);
    }

    public void ClickedOn()
    {
        if (isRootBtn)
        {
            GameObject mainUICont = GameObject.FindGameObjectWithTag("menuUiCont");
            if (mainUICont)
            {
                MenuAnimControl uiControlComp = mainUICont.GetComponent<MenuAnimControl>();
                if (uiControlComp)
                {
                    if (uiControlComp.menuLast != "NA" && uiControlComp.menuLast != btnName)
                    {
                        return;
                    }
                }
            }
        }

        isClicked = !isClicked;
        anim.SetBool("isClicked", isClicked);
        anim.SetBool("isHowered", isClicked);
        if (buttonClick != null)
            buttonClick(isClicked);
    }
}
