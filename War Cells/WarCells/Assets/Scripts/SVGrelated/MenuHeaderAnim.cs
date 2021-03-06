﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Animator))]
public class MenuHeaderAnim : MonoBehaviour
{
    public delegate void delagatebuttonClicked(bool isEnabled);
    public delagatebuttonClicked buttonClick;



    Animator anim;
    bool isHover;
    public bool isClicked;

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
        if (!isClicked)
            isClicked = true;
        anim.SetBool("isClicked", isClicked);
        if (buttonClick != null)
            buttonClick(isClicked);
    }

    public void Unselect()
    {
        isClicked = false;
        anim.SetBool("isClicked", isClicked);
    }
}
