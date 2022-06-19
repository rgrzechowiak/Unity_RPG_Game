using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    private CharacterController controller;
    
    private float verticalVelocity;
    private float groundedTimer;        // to allow jumping when going down ramps
    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = 9.81f;
    [SerializeField]
    private float characterTurnSpeed = 3.0f;
    Vector3 move;


    Transform cameraTarget;
    float cameraPitch = 40.0f;
    float cameraYaw = 0;
    float cameraDistance = 5.0f;
    bool lerpYaw = false;
    bool lerpDistance = false;
    [SerializeField]
    private float cameraPitchSpeed = 2.0f;
    [SerializeField]
    private float cameraPitchMin = -10.0f;
    [SerializeField]
    private float cameraPitchMax = 80.0f;
    [SerializeField]
    private float cameraYawSpeed = 5.0f;
    [SerializeField]
    private float cameraDistanceSpeed = 5.0f;
    [SerializeField]
    private float cameraDistanceMin = 2.0f;
    [SerializeField]
    private float cameraDistanceMax = 12.0f;
    [SerializeField]

    Vector3 lastPos;
    bool isMoving = false;

    private void Start()
    {
        // always add a controller
        controller = gameObject.AddComponent<CharacterController>();
        cameraTarget = transform;
    }

    public void LateUpdate()
    {
        CameraControl();
    }

    void FixedUpdate()
    {
        PlayerMovement();
    }

    void CameraControl()
    {
        // If mouse button down then allow user to look around
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            cameraPitch += Input.GetAxis("Mouse Y") * cameraPitchSpeed;
            cameraPitch = Mathf.Clamp(cameraPitch, cameraPitchMin, cameraPitchMax);
            cameraYaw += Input.GetAxis("Mouse X") * cameraYawSpeed;
            cameraYaw = cameraYaw % 360.0f;
            lerpYaw = false;
        }
        else
        {
            // If moving then make camera follow
            if (lerpYaw)
                cameraYaw = Mathf.LerpAngle(cameraYaw, cameraTarget.eulerAngles.y, 5.0f * Time.deltaTime);
        }

        // Zoom
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            cameraDistance -= Input.GetAxis("Mouse ScrollWheel") * cameraDistanceSpeed;
            cameraDistance = Mathf.Clamp(cameraDistance, cameraDistanceMin, cameraDistanceMax);
            lerpDistance = false;
        }

        // Calculate camera position
        Vector3 newCameraPosition = cameraTarget.position + (Quaternion.Euler(cameraPitch, cameraYaw, 0) * Vector3.back * cameraDistance);

        // Does new position put us inside anything?
        RaycastHit hitInfo;
        if (Physics.Linecast(cameraTarget.position, newCameraPosition, out hitInfo))
        {
            newCameraPosition = hitInfo.point;
            lerpDistance = true;
        }
        else
        {
            if (lerpDistance)
            {
                float newCameraDistance = Mathf.Lerp(Vector3.Distance(cameraTarget.position, Camera.main.transform.position), cameraDistance, 5.0f * Time.deltaTime);
                newCameraPosition = cameraTarget.position + (Quaternion.Euler(cameraPitch, cameraYaw, 0) * Vector3.back * newCameraDistance);
            }
        }

        Camera.main.transform.position = newCameraPosition;
        Camera.main.transform.LookAt(cameraTarget.position);
    }

    void PlayerMovement()
    {
        var hInput = Input.GetAxis("Horizontal");
        var vInput = Input.GetAxis("Vertical");

        isMoving = IsPlayerMoving(lastPos);

        bool groundedPlayer = controller.isGrounded;
        if (groundedPlayer)
        {
            // cooldown interval to allow reliable jumping even whem coming down ramps
            groundedTimer = 0.2f;
        }
        if (groundedTimer > 0)
        {
            groundedTimer -= Time.deltaTime;
        }

        // slam into the ground
        if (groundedPlayer && verticalVelocity < 0)
        {
            // hit ground
            verticalVelocity = 0f;
        }

        // apply gravity always, to let us track down ramps properly
        verticalVelocity -= gravityValue * Time.deltaTime;

        // Only rotate player if right click held, this allows left click hold to not rotate the player
        // If camera not in use have player turn in place
        if (Input.GetMouseButton(1))
        {
            lerpYaw = false;
            transform.rotation = Quaternion.Euler(0, cameraYaw, 0); // face camera
            // gather lateral input control
            move = new Vector3(hInput, 0, vInput);
        }
        else
        {
            //If we are not moving then do not force the camera to be behind the player
            // and if we start moving force the camera behind the player
            if (!isMoving)
            {
                lerpYaw = false;
            }
            else
            {
                lerpYaw = true;
            }

            transform.Rotate(0, hInput * characterTurnSpeed, 0);
            move = Vector3.forward * vInput;
        }

        // Local to world space
        move = transform.TransformDirection(move);

        // scale by speed
        move *= playerSpeed;

        // only align to motion if we are providing enough input
        if (move.magnitude > 0.15f)
        {
            Vector3 fwd = transform.forward;


            // use the 0.5f through arccos for the 30-degree demarcation
            // angle past which we will consider you moving backwards.
            // Keeps player facing frontwards
            if (Vector3.Dot(move.normalized, fwd) < -0.5f)
            {
                // walking backwards
                gameObject.transform.forward = -move;

            }
            else
            {
                gameObject.transform.forward = move;
            }
        }

        if(controller.isGrounded)
        {
            if(Input.GetButton("Jump"))
            {
                if (groundedTimer > 0)
                {
                    // no more until we recontact ground
                    groundedTimer = 0;

                    // Physics dynamics formula for calculating jump up velocity bas/Ded on height and gravity
                    verticalVelocity += Mathf.Sqrt(jumpHeight * 2 * gravityValue);
                }
            }
        }

        move.y = verticalVelocity;

        lastPos = transform.position;

        // call .Move() once only
        controller.Move(move * Time.deltaTime);
    }

    // Checks if the player is moving
    bool IsPlayerMoving(Vector3 lastPosition)
    {
        if(transform.position != lastPosition)
        {
            Debug.Log("Moving");
            return true;
        }
        else
        {
            return false;
        }
    }
}
