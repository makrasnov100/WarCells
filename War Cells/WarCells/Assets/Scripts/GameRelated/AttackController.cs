﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttackController : MonoBehaviour
{
    //References
    public TurnUIController turnController;
    public PlayerManager playerManager;

    //Instance Variables
    public GameObject originCell;

    ///[UNITY DEFAULT]
    private void Update()
    {
        //Call Apropriate function based on platform type
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
            HandleTouch();
        else
            HandleMouse();
    }

    ///[INPUT HANDLERS]
    //HandleTouch: called on a touch based platform every frame - performs attack input
    void HandleTouch()
    {
        //Attack stage determined when a single finger touches the screen
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            //Find the object that is being touched
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            //Ignore touch if over UI
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;

            HandleInput(hit);
        }
    }

    //HandleMouse: called on a mosue based platform every frame - performs attack input
    void HandleMouse()
    {
        //TODO: Implement
        if (!Input.GetMouseButtonDown(0))
            return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        //Ignore touch if (TODO:scrolling) or menu click
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        HandleInput(hit);
    }

    //Carries out the cell attack commands for all platforms based on hit information
    void HandleInput(RaycastHit2D hit)
    {
        //No attacks declaration allowed while attack animations are carried out
        if (turnController.isAnimationPlaying)
            return;

        if (hit.collider != null && hit.collider.gameObject.tag != "cell") //Hit a cell
        {
            if (originCell == null) ///No origin cell selected yet
            {
                //Ignore selection if cell is not owned by a current player
                if (hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner() == -1 ||
                    playerManager.GetPlayers()[hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner()].GetIsBot() ||
                    hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner() != turnController.GetCurPlayerId())
                    return;

                originCell = hit.collider.gameObject;
                originCell.GetComponent<CellIdentity>().ChangeActivationState(true, true, 1);
                turnController.ShowDefenceUI(originCell.GetComponent<CellIdentity>());
            }
            else ///Origin cell has already been selected
            {
                //Selected origin cell second time
                if (originCell.GetInstanceID() == hit.collider.gameObject.GetInstanceID())
                {
                    originCell.GetComponent<CellIdentity>().ChangeActivationState(false, true, 0);
                    originCell = null;
                    turnController.ShowNextTurnUI();
                }
                //Selected cell thats connected to the current origin cell       
                else if (originCell.GetComponent<CellIdentity>().IsConnectedTo(hit.collider.gameObject.GetComponent<CellIdentity>().GetId()))
                {
                    GameObject destinationCell = hit.collider.gameObject;
                    destinationCell.GetComponent<CellIdentity>().SwitchActivationColor(originCell.GetComponent<CellIdentity>().GetId());
                    originCell.GetComponent<CellIdentity>().SwitchAttackOption(destinationCell.GetComponent<CellIdentity>().GetId());
                }//Selected new origin cell
                else
                {
                    if (hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner() == -1 ||
                        playerManager.GetPlayers()[hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner()].GetIsBot() ||
                        hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner() != turnController.GetCurPlayerId())
                        return;

                    originCell.GetComponent<CellIdentity>().ChangeActivationState(false, true, 0);
                    originCell = hit.collider.gameObject;
                    originCell.GetComponent<CellIdentity>().ChangeActivationState(true, true, 1);
                    turnController.ShowDefenceUI(originCell.GetComponent<CellIdentity>());
                }

                //Save any changes before new UI is shown or hidden
                ConfirmDefence();
            }
        }
        else //Selected an empty space (resets all)
        {
            if (originCell != null)
            {
                ConfirmDefence();
                originCell.GetComponent<CellIdentity>().ChangeActivationState(false, true, 0);
                originCell = null;
            }

            turnController.ShowNextTurnUI();
        }
    }


    ///[UI UPDATES]
    //EditUnitsSent: called on any slider value change
    public void EditUnitsSent()
    {
        //Ignore slider value changes that were done by a script
        if (turnController.ignoreSliderEdits)
            return;

        //Set the correct text value of slider position
        turnController.GetUnitCurrent().text = "" + (int)turnController.GetUnitAmountInput().value;

        //Set the correct origin cell reserve text
        if (originCell != null)
            originCell.GetComponent<CellIdentity>().UpdateReserveIndicator((int)turnController.GetUnitAmountInput().value);
    }


    ///[BACKEND UPDATES]
    //ConfirmDefence: called to finalize a reserve unit amount for the origin cell
    void ConfirmDefence()
    {
        if (originCell != null)
        {
            originCell.GetComponent<CellIdentity>().SetReserveUnits((int)turnController.GetUnitAmountInput().value);
            originCell.GetComponent<CellIdentity>().RecalculateAttackUnits();
        }
    }
    //ResetCellSelection: called to remove selection over an origin cell if present
    public void ResetCellSelection()
    {
        if (originCell != null)
        {
            ConfirmDefence();
            originCell.GetComponent<CellIdentity>().ChangeActivationState(false, true, 0);
            originCell = null;
        }
    }


    ///[ACCESORS/MUTATORS]
    public GameObject GetOriginCell() { return originCell; }
}
