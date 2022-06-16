using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [SerializeField]
    private Transform _Camera;
    private CharacterController controller;
    private float verticalVelocity;
    private float groundedTimer;        // to allow jumping when going down ramps
    private float playerSpeed = 5f;
    private float jumpHeight = 1.0f;
    private float gravityValue = 9.81f;
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private Vector3 moveDir;
    private Quaternion rotation;
 
    private void Start()
    {
        // always add a controller
        controller = gameObject.AddComponent<CharacterController>();
        moveDir = new Vector3();
    }
 
    void Update()
    {
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
 
        // gather lateral input control
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
 
        // scale by speed
        move *= playerSpeed;


        // Rotates the player based on input and camera via Quaterion; 

        float cameraAngle = _Camera.eulerAngles.y;
        float playerMovementAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
        float angle = playerMovementAngle + cameraAngle;
        rotation = Quaternion.Euler(0f, angle, 0f);
        transform.rotation = rotation;
        

        // only align to motion if we are providing enough input
        if (move.magnitude > 0.05f)
        {
            //Move player in the direction of the current rotation
            moveDir = rotation * Vector3.forward;
            move = moveDir;
        }
 
        // allow jump as long as the player is on the ground
        if (Input.GetButtonDown("Jump"))
        {
            // must have been grounded recently to allow jump
            if (groundedTimer > 0)
            {
                // no more until we recontact ground
                groundedTimer = 0;
 
                // Physics dynamics formula for calculating jump up velocity bas/Ded on height and gravity
                verticalVelocity += Mathf.Sqrt(jumpHeight * 2 * gravityValue);
            }
        }
 
        // inject Y velocity before we use it
        move.y = verticalVelocity;
 
        // call .Move() once only
        controller.Move(move * Time.deltaTime * playerSpeed);
    }
}
