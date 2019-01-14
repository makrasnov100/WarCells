using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraHandler : MonoBehaviour
{
    //References
    Camera cam;

    //Instance Variables
    Vector3 lastPanPosition;
    int panFingerId;
    bool wasZoomingLastFrame;
    Vector2[] lastZoomPositions;

    //Movement Sensitivity
    float panSpeed = 6f;
    float zoomSpeedTouch = 0.1f;
    float zoomSpeedMouse = 0.5f;

    float[] boundsX = new float[] { -100f, 100f };
    float[] boundsY = new float[] { -100f, 100f };
    float[] zoomBounds = new float[] { 20f, 140f };


    //Get Reference(s)
    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
        {
            HandleTouch();
        }
        else
        {
            HandleMouse();
        }
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0)) //Record mouse position on initial click
        {
            lastPanPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0)) //Proccess pan if still holding mouse
        {
            PanCamera(Input.mousePosition);
        }

        //Check for any zoom action
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll, zoomSpeedMouse);
    }


    //Action Function (these contain code that move the camera)
    void PanCamera(Vector3 newPanPosition)
    {
        //Determine how much to move the camera
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * panSpeed, offset.y * panSpeed, 0);

        //Perform the camera movement
        transform.Translate(move, Space.World);

        //Ensure the camera remains within bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x, boundsX[0], boundsX[1]);
        pos.y = Mathf.Clamp(transform.position.y, boundsY[0], boundsY[1]);
        transform.position = pos;

        //Cache last touch/mosue position (TODO: may be buggy if on bound - check)
        lastPanPosition = newPanPosition;
    }

    void ZoomCamera(float offset, float speed)
    {
        if (offset == 0)
            return;

        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), zoomBounds[0], zoomBounds[1]);
    }

    void HandleTouch()
    {
        switch (Input.touchCount)
        {
            case 1: //One Finger - Panning

                wasZoomingLastFrame = false;

                Touch touch = Input.GetTouch(0);

                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) // No panning while over UI
                {
                    lastPanPosition = touch.position;
                    return;
                }

                if (touch.phase == TouchPhase.Began) //Record touch info if pan touch just began
                {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                }
                else if(touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                {
                    PanCamera(touch.position);
                }
                break;

            case 2: //Two Fingers - Zooming
                Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                if (!wasZoomingLastFrame)
                {
                    lastZoomPositions = newPositions;
                    wasZoomingLastFrame = true;
                }
                else
                {
                    float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                    float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                    float offset = newDistance - oldDistance;

                    ZoomCamera(offset, zoomSpeedTouch);

                    lastZoomPositions = newPositions;
                }
                break;

            default:
                wasZoomingLastFrame = false;
                break;
        }
    }
}