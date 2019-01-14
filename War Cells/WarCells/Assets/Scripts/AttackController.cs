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
    public GameObject destinationCell;

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

            Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 100f);

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
                    originCell.GetComponent<CellIdentity>().ChangeActivationState(true, 1);
                }
                else if (destinationCell == null)
                {
                    if (originCell.GetInstanceID() == hit.collider.gameObject.GetInstanceID()) //Origin selected second time
                    {
                        originCell.GetComponent<CellIdentity>().ChangeActivationState(false, 0);
                        originCell = null;
                        return;
                    }

                    if (originCell.GetComponent<CellIdentity>().IsConnectedTo(hit.collider.gameObject.GetComponent<CellIdentity>().GetId()))
                    {
                        destinationCell = hit.collider.gameObject;
                        destinationCell.GetComponent<CellIdentity>().ChangeActivationState(true, 2);
                        destinationCell.GetComponent<CellIdentity>().SetSingleOpenConnectionColor(originCell.GetComponent<CellIdentity>().GetId(), Color.red);
                        turnController.ShowAttackUI(true);
                        turnController.ChangeMaxUnitText(originCell.GetComponent<CellIdentity>().GetOutboundUnits(destinationCell.GetComponent<CellIdentity>().GetId()), originCell.GetComponent<CellIdentity>().GetCurUnusedUnits(destinationCell.GetComponent<CellIdentity>().GetId()));
                    }
                    else //TODO: modularize (refactor for simpler code) (some of below is repeated)
                    {
                        if (hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner() == -1 ||
                            playerManager.GetPlayers()[hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner()].GetIsBot())
                            return;

                        originCell.GetComponent<CellIdentity>().ChangeActivationState(false, 0);
                        originCell = hit.collider.gameObject;
                        originCell.GetComponent<CellIdentity>().ChangeActivationState(true, 1);
                    }
                }
                else //both origin and destination selected
                {
                    ConfirmAttack();
                    if (originCell.GetInstanceID() == hit.collider.gameObject.GetInstanceID())
                    {
                        destinationCell.GetComponent<CellIdentity>().ChangeActivationState(false, 0);
                        destinationCell.GetComponent<CellIdentity>().SetSingleOpenConnectionColor(originCell.GetComponent<CellIdentity>().GetId(), Color.yellow);
                        destinationCell = null;
                        originCell.GetComponent<CellIdentity>().ChangeActivationState(false, 0);
                        originCell = null;
                        turnController.ShowAttackUI(false);
                    }
                    else if (destinationCell.GetInstanceID() == hit.collider.gameObject.GetInstanceID())
                    {
                        destinationCell.GetComponent<CellIdentity>().ChangeActivationState(false, 0);
                        destinationCell.GetComponent<CellIdentity>().SetSingleOpenConnectionColor(originCell.GetComponent<CellIdentity>().GetId(), Color.yellow);
                        destinationCell = null;
                        turnController.ShowAttackUI(false);
                    }
                    else if (originCell.GetComponent<CellIdentity>().IsConnectedTo(hit.collider.gameObject.GetComponent<CellIdentity>().GetId()))
                    {
                        destinationCell.GetComponent<CellIdentity>().ChangeActivationState(false, 0);
                        destinationCell.GetComponent<CellIdentity>().SetSingleOpenConnectionColor(originCell.GetComponent<CellIdentity>().GetId(), Color.yellow);
                        destinationCell = hit.collider.gameObject;
                        destinationCell.GetComponent<CellIdentity>().ChangeActivationState(true, 2);
                        destinationCell.GetComponent<CellIdentity>().SetSingleOpenConnectionColor(originCell.GetComponent<CellIdentity>().GetId(), Color.red);
                        turnController.ChangeMaxUnitText(originCell.GetComponent<CellIdentity>().GetOutboundUnits(destinationCell.GetComponent<CellIdentity>().GetId()), originCell.GetComponent<CellIdentity>().GetCurUnusedUnits(destinationCell.GetComponent<CellIdentity>().GetId()));

                        //turnController.unitMax.text = "/" + originCell.GetComponent<CellIdentity>().GetCurUnusedUnits(destinationCell.GetComponent<CellIdentity>().GetId());
                    }
                    else //selected a cell thats not connected to origin and not being the origin itself
                    {
                        if (hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner() == -1 ||
                            playerManager.GetPlayers()[hit.collider.gameObject.GetComponent<CellIdentity>().GetOwner()].GetIsBot())
                            return;

                        originCell.GetComponent<CellIdentity>().ChangeActivationState(false, 0);
                        originCell = hit.collider.gameObject;
                        originCell.GetComponent<CellIdentity>().ChangeActivationState(true, 1);
                        destinationCell.GetComponent<CellIdentity>().ChangeActivationState(false, 0);
                        destinationCell.GetComponent<CellIdentity>().SetSingleOpenConnectionColor(originCell.GetComponent<CellIdentity>().GetId(), Color.yellow);
                        destinationCell = null;
                        turnController.ShowAttackUI(false);
                    }
                }
                
                //print("Hit a Cell - " + hit.collider.gameObject.name + "!");
            }
            else
            {
                if (originCell != null && destinationCell != null)
                {
                    ConfirmAttack();
                }

                turnController.ShowAttackUI(false);
                if (destinationCell != null)
                {
                    destinationCell.GetComponent<CellIdentity>().ChangeActivationState(false, 0);
                    destinationCell.GetComponent<CellIdentity>().SetSingleOpenConnectionColor(originCell.GetComponent<CellIdentity>().GetId(), Color.yellow);
                    destinationCell = null;
                }
                if (originCell != null)
                {
                    originCell.GetComponent<CellIdentity>().ChangeActivationState(false, 0);
                    originCell = null;
                }
            }
        }
    }

    void HandleMouse()
    {
        //TODO: Implement
    }

    public void EditUnitsSent()
    {
        turnController.unitCurrent.text = "" + (int)turnController.unitAmount.value;
        originCell.GetComponent<CellIdentity>().UpdateAttackArrows((int)turnController.unitAmount.value, destinationCell.GetComponent<CellIdentity>().GetId());
    }

    public void ConfirmAttack() //TODO: handle memory deletion after each turn
    {
        int[] outAttack;
        int[] inAttack;
        bool isAllowed = turnController.UnitTransferInfo(originCell.GetComponent<CellIdentity>(),
                                                         destinationCell.GetComponent<CellIdentity>(),
                                                         out outAttack, out inAttack);
        if (!isAllowed)
        {
            return;
        }

        originCell.GetComponent<CellIdentity>().AddAttack(outAttack, true);
        destinationCell.GetComponent<CellIdentity>().AddAttack(inAttack, false);


        //Reset Destination Selection
        /*turnController.ShowAttackUI(false);
        if (destinationCell != null)
        {
            destinationCell.GetComponent<CellIdentity>().ChangeActivationState(false, 0);
            destinationCell.GetComponent<CellIdentity>().SetSingleOpenConnectionColor(originCell.GetComponent<CellIdentity>().GetId(), Color.yellow);
            destinationCell = null;
        }
        if (originCell != null)
        {
            originCell.GetComponent<CellIdentity>().ChangeActivationState(false, 0);
            originCell = null;
        }*/
    }
}
