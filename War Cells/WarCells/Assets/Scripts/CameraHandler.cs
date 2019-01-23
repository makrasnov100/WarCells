using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraHandler : MonoBehaviour
{
    //Component References
    Camera cam;
    public GameObject bg;

    //Instance Variables
    Vector3 lastPanPosition;
    int panFingerId;
    bool wasZoomingLastFrame;
    Vector2[] lastZoomPositions;

    //Camera Movement Parameters
    // - sensitivity
    public float panSpeed = 10f;
    public float bgPanSpeed = .1f;
    public float zoomSpeedTouch = 0.1f;
    public float zoomSpeedMouse = 100f;
    // - bounds
    float[] boundsX = new float[] { -100f, 100f };
    float[] boundsY = new float[] { -100f, 100f };
    float[] zoomBounds = new float[] { 20f, 140f };

    ///[UNITY DEFAULT]
    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        //Call Apropriate function based on platform type
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
            HandleTouch();
        else
            HandleMouse();
    }

    ///[INPUT HANDLERS]
    //HandleMouse: moves camera based on mouse input
    void HandleMouse()
    {
        //Ignore mouse if over UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0)) //Record mouse position on initial click
            lastPanPosition = Input.mousePosition;
        else if (Input.GetMouseButton(0)) //Proccess pan if still holding mouse
            PanCamera(Input.mousePosition);

        //Check for any zoom action
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll, zoomSpeedMouse);
    }

    //HandleMouse: moves camera based on touch input
    void HandleTouch()
    {
        switch (Input.touchCount)
        {
            case 1: //One Finger - Panning

                wasZoomingLastFrame = false;

                Touch touch = Input.GetTouch(0);

                //Ignore touch if over UI
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    lastPanPosition = touch.position;
                    return;
                }

                if (touch.phase == TouchPhase.Began) //Record touch info if pan touch just began
                {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                }
                else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
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


    ///[CAMERA MOVEMENT]
    //PanCamera: traverses camera on a plane parallel to game world based on pan position change
    void PanCamera(Vector3 newPanPosition)
    {
        //Determine how much to move the camera
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * panSpeed, offset.y * panSpeed, 0);
        Vector3 moveBG = new Vector3(offset.x * bgPanSpeed, offset.y * bgPanSpeed, 0);

        //Perform the camera movement
        transform.Translate(move, Space.World);
        bg.transform.Translate(moveBG, Space.World);

        //Ensure the camera remains within bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x, boundsX[0], boundsX[1]);
        pos.y = Mathf.Clamp(transform.position.y, boundsY[0], boundsY[1]);
        transform.position = pos;


        //Cache last touch/mosue position (TODO: may be buggy if on bound - check)
        lastPanPosition = newPanPosition;
    }

    //ZoomCamera: creates illusion of sooming in/out by changing the depth of feild
    void ZoomCamera(float offset, float speed)
    {
        if (offset == 0)
            return;

        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), zoomBounds[0], zoomBounds[1]);
    }
}