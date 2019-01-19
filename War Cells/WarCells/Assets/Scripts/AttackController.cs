using System.Collections;
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

    private void Update()
    {
        //TODO: add check if turn allowed rn

        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
        {
            HandleTouch();
        }
        else
        {
            HandleMouse();
        }
    }

    void HandleTouch()
    {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            //Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 100f);

            //Ignore touch if (TODO:scrolling) or menu click
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;

            if (hit.collider != null && hit.collider.gameObject.tag != "cell") //if hit a cell
            {
                if (originCell == null)
                {
                    if (hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner() == -1 ||
                        playerManager.GetPlayers()[hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner()].GetIsBot())
                        return;

                    originCell = hit.collider.gameObject;
                    originCell.GetComponent<CellIdentity>().ChangeActivationState(true, true, 1);
                    turnController.ShowDefenceUI(originCell.GetComponent<CellIdentity>());
                }
                else
                {
                    ConfirmDefence();
                    if (originCell.GetInstanceID() == hit.collider.gameObject.GetInstanceID()) //Origin selected second time
                    {
                        originCell.GetComponent<CellIdentity>().ChangeActivationState(false, true, 0);
                        originCell = null;
                        turnController.HideUI();
                        return;
                    }

                    if (originCell.GetComponent<CellIdentity>().IsConnectedTo(hit.collider.gameObject.GetComponent<CellIdentity>().GetId()))
                    {
                        GameObject destinationCell = hit.collider.gameObject;
                        destinationCell.GetComponent<CellIdentity>().SwitchActivationColor(originCell.GetComponent<CellIdentity>().GetId());
                        originCell.GetComponent<CellIdentity>().SwitchAttackOption(destinationCell.GetComponent<CellIdentity>().GetId());
                    }
                    else //TODO: modularize (refactor for simpler code) (some of below is repeated)
                    {
                        if (hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner() == -1 ||
                            playerManager.GetPlayers()[hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner()].GetIsBot())
                            return;

                        originCell.GetComponent<CellIdentity>().ChangeActivationState(false, true, 0);
                        originCell = hit.collider.gameObject;
                        originCell.GetComponent<CellIdentity>().ChangeActivationState(true, true, 1);
                        turnController.ShowDefenceUI(originCell.GetComponent<CellIdentity>());
                    }
                }
            }
            else
            {
                if (originCell != null)
                {
                    ConfirmDefence();
                    originCell.GetComponent<CellIdentity>().IndicateAttackConnection();
                }

                turnController.HideUI();
                if (originCell != null)
                {
                    originCell.GetComponent<CellIdentity>().ChangeActivationState(false, true, 0);
                    originCell = null;
                }
            }
        }
    }

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

        if (hit.collider != null && hit.collider.gameObject.tag != "cell") //if hit a cell
        {
            if (originCell == null)
            {
                if (hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner() == -1 ||
                    playerManager.GetPlayers()[hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner()].GetIsBot())
                    return;

                originCell = hit.collider.gameObject;
                originCell.GetComponent<CellIdentity>().ChangeActivationState(true, true, 1);
                turnController.ShowDefenceUI(originCell.GetComponent<CellIdentity>());
            }
            else
            {
                ConfirmDefence();
                if (originCell.GetInstanceID() == hit.collider.gameObject.GetInstanceID()) //Origin selected second time
                {
                    originCell.GetComponent<CellIdentity>().ChangeActivationState(false, true, 0);
                    originCell = null;
                    turnController.HideUI();
                    return;
                }

                if (originCell.GetComponent<CellIdentity>().IsConnectedTo(hit.collider.gameObject.GetComponent<CellIdentity>().GetId()))
                {
                    GameObject destinationCell = hit.collider.gameObject;
                    destinationCell.GetComponent<CellIdentity>().SwitchActivationColor(originCell.GetComponent<CellIdentity>().GetId());
                    originCell.GetComponent<CellIdentity>().SwitchAttackOption(destinationCell.GetComponent<CellIdentity>().GetId());
                }
                else //TODO: modularize (refactor for simpler code) (some of below is repeated)
                {
                    if (hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner() == -1 ||
                        playerManager.GetPlayers()[hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner()].GetIsBot())
                        return;

                    originCell.GetComponent<CellIdentity>().ChangeActivationState(false, true, 0);
                    originCell = hit.collider.gameObject;
                    originCell.GetComponent<CellIdentity>().ChangeActivationState(true, true, 1);
                    turnController.ShowDefenceUI(originCell.GetComponent<CellIdentity>());
                }
            }
        }
        else
        {
            if (originCell != null)
            {
                ConfirmDefence();
                originCell.GetComponent<CellIdentity>().IndicateAttackConnection();
            }

            turnController.HideUI();
            if (originCell != null)
            {
                originCell.GetComponent<CellIdentity>().ChangeActivationState(false, true, 0);
                originCell = null;
            }
        }
    }


    public void EditUnitsSent()
    {
        if (turnController.ignoreSliderEdits)
            return;

        turnController.getUnitCurrent().text = "" + (int)turnController.getUnitAmountInput().value;

        if (originCell != null)
            originCell.GetComponent<CellIdentity>().UpdateReserveIndicator((int)turnController.getUnitAmountInput().value);
    }

    void ConfirmDefence()
    {
        originCell.GetComponent<CellIdentity>().SetReserveUnits((int)turnController.getUnitAmountInput().value);
        originCell.GetComponent<CellIdentity>().RecalculateAttackUnits();
    }
}
