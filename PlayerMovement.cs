using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //basic movement input
    public bool Left = false;
    public bool Right = false;
    public bool Front = false;
    public bool Back = false;
    public bool Jump = false;
    public bool Slide = false;

    //referencing the rigidbody component to apply forces
    public Rigidbody rb;
    //referencing the transform component of the camera to get the rotation
    public Transform CamRot;
    //the directional force
    public int dirForce = 100;
    private const int CONSTFORCE = 100;
    public Vector3 wallForce = Vector3.zero;
    //the jump force
    public int jumpF = 700;
    //whether or not the player is on the ground
    public bool OnGround = true;
    public bool OnWallL = false;
    public bool OnWallR = false;
    public bool OnWallF = false;
    //whether or not the player is crouching
    public bool isCrouching = false;
    //the size of the player when not crouching
    public Vector3 playerHeight;
    //the size of the player when crouching
    public Vector3 playerCrouchHeight = Vector3.zero;
    //the force applied on the x and z axes
    public float xForce = 0f;
    public float zForce = 0f;
    //the max speed in a direction
    public float MAXFORCE = 90f;
    //the friction coefficient
    public float FRICTION = 200;
    //forces for the direction when the slide started
    private float slideX = 0f;
    private float slideZ = 0f;
    //multiplier to cancel movement when sliding
    private int cancelM = 1;
    //multiplier to cancel the wallrun force
    private float wallJumpHeight = 0.4f;
    //whether or not the player is ready to walljump
    public bool CanWJump = true;
    public bool ResetWJump = true;
    //if the player jumped from ground into the air
    public bool JumpToAir = false;
    //declaring the variable that stores info about what the raycast hit
    private RaycastHit hitInfo;
    private Vector3 rayDir;


    void Start()
    {
        playerHeight = transform.localScale;
        playerCrouchHeight.x = 1.5f;
        playerCrouchHeight.y = 0.5f;
        playerCrouchHeight.z = 1.5f;
        CanWJump = true;
    }



    void FixedUpdate()
    {
        //calling the get input function to take player input for movement
        player_input();
        detect_ground();
        if (!OnGround)
        {
            //calling the wallrun function
            detect_wall();
        }
        else
        {
            OnWallL = false;
            OnWallR = false;
        }
        //calling the allowwallforce function
        Debug.Log(rb.velocity);
        //decreasing player speed if they are in the air
        if (!OnGround && !OnWallL && !OnWallR)
        {
            dirForce = CONSTFORCE / 2;
        }
        else if (OnGround || OnWallL || OnWallR)
        {
            dirForce = CONSTFORCE;
        }

        //getting the value of the x and z from the function Orientation
        orientation(out xForce, out zForce, dirForce);

        //adding friction
        friction();
        //assigning the force for sliding
        if (slideX == 0 && slideZ == 0)
        {
            slideX = xForce;
            slideZ = zForce;
        }

        //setting the player height to the default height
        transform.localScale = (playerHeight);
        //checking if the slide input key is pressed
        Slide = false;
        player_input();
        if (Slide)
        {
            //making the player smaller and adding downward force
            transform.localScale = playerCrouchHeight;
            rb.AddForce(Vector3.down * Time.deltaTime * 400000 * Time.deltaTime);
            player_input();
            cancelM = 0;
            //checking if the front input key is pressed
            if (Front)
            {
                rb.AddForce(slideX * Time.deltaTime, 0, slideZ * Time.deltaTime, ForceMode.VelocityChange);
            }
        }
        else
        {
            //setting the player height to the default height
            transform.localScale = (playerHeight);
            slideX = 0;
            slideZ = 0;
            cancelM = 1;
        }
        
      

        //extra gravity for better collision detection
        rb.AddForce(Vector3.down * Time.deltaTime * 10);
        
        //keeping the player stuck to the wall or on the wall
        if ((OnWallL && !Right) || (OnWallR && !Left))
        {
            wallForce.y = 0;
            if (rb.velocity.y < 0)
            {
                wallForce.y = -rb.velocity.y * 16 * Time.deltaTime;
            }
            if (!(Front || Left || Right || Back))
            {
                wallForce.y = 0.1f;
            }
            rb.AddForce(wallForce * Time.deltaTime * 200, ForceMode.VelocityChange);
        }
        //checking if left input key is pressed
        if (Left && !OnWallL)
        {
            rb.AddForce(-zForce * Time.deltaTime * cancelM, 0, xForce * Time.deltaTime * cancelM, ForceMode.VelocityChange);
            Left = false;
        }
        //checking if the right input key is pressed
        if (Right && !OnWallR)
        {
            rb.AddForce(zForce * Time.deltaTime * cancelM, 0, -xForce * Time.deltaTime * cancelM, ForceMode.VelocityChange);
            Right = false;
        }
        //checking if the front input key is pressed
        if (Front)
        {
            rb.AddForce(xForce * Time.deltaTime * cancelM, 0, zForce * Time.deltaTime * cancelM, ForceMode.VelocityChange);
            Front = false;
        }
        //walljumping and adjusting for jumping from the front
        wallJumpHeight = 0.4f;
        if (OnWallF)
        {
            wallJumpHeight = 0.7f;
        }

        if (ResetWJump && (OnWallF || OnWallL || OnWallR))
        {
            CanWJump = true;
        }
        //walljump here
        if (Jump && (OnWallL || OnWallR || OnWallF) && !Slide && CanWJump)
        {
            rb.AddForce(-wallForce.x * 50, jumpF * wallJumpHeight * 0.03f, -wallForce.z * 50, ForceMode.VelocityChange);
            CanWJump = false;
            ResetWJump = false;
            Invoke("allow_jump", 0.1f);
        }
        //moving off the wall if touching the ground
        if ((OnWallL || OnWallR) && OnGround)
        {
            rb.AddForce(-wallForce.x * 10, 0f, -wallForce.z * 10, ForceMode.VelocityChange);
        }
        
        if (OnGround)
        {
            wallForce = Vector3.zero;
        }

        

        //checking if the back input key is pressed
        if (Back)
        {
            rb.AddForce(-xForce * Time.deltaTime * cancelM, 0, -zForce * Time.deltaTime * cancelM, ForceMode.VelocityChange);
            Back = false;
        }

        //checking if the jump input key is pressed and if the object is on the ground
        if (Jump && OnGround && !Slide && !OnWallL && !OnWallR && !OnWallF)
        {
            rb.AddForce(0, jumpF, 0);
            Jump = false;
            Slide = false;
            OnGround = false;
            inAir = true;
        }
        left_ground();
        //checking to see if the player is below a certain height and calling the GameOver function if it is
        if (transform.position.y < -1)
        {
            FindObjectOfType<GameManagement>().GameOver();
        }
    }

    void detect_ground()
    {
        //setting OnGround and OnWall to false until proven true
        OnGround = false;
        //shooting raycasts from the centre to detect ground below
        player_input();
        Vector3 offset = Vector3.zero;
            if (!Slide)
            {
                offset.y = -0.75f;
            }
        if (Physics.Raycast(transform.position, Vector3.down, out hitInfo))
        {
            offset += transform.position;
            if (Physics.Raycast(offset, hitInfo.collider.ClosestPoint(offset) - offset, out hitInfo, 0.8f))
            {
                if (hitInfo.collider.tag == "Ground")
                {
                    OnGround = true;
                }
            }
        }
    }
    void detect_wall()
    {
        OnWallL = false;
        OnWallR = false;
        OnWallF = false;
        player_input();
        if (!Slide && !OnGround)
        {

            rayDir = Vector3.zero;
            rayDir.x = -zForce;
            rayDir.z = xForce;
            if (Physics.Raycast(transform.position, rayDir, out hitInfo, 1.2f))
            {
                if (hitInfo.collider.tag == "Ground" && (( ( (hitInfo.collider.ClosestPoint(transform.position).x - transform.position.x) * (hitInfo.collider.ClosestPoint(transform.position).x - transform.position.x) ) + ( (hitInfo.collider.ClosestPoint(transform.position).z - transform.position.z) * (hitInfo.collider.ClosestPoint(transform.position).z - transform.position.z) ) ) < 0.57f ) )
                {
                    OnWallL = true;
                    wallForce = hitInfo.collider.ClosestPoint(transform.position) - transform.position;
                    absolute(ref wallForce);
                }
            }
        }

        if (!Slide && !OnGround)
        {

            rayDir = Vector3.zero;
            rayDir.x = zForce;
            rayDir.z = -xForce;
            if (Physics.Raycast(transform.position, rayDir, out hitInfo, 1.2f))
            {

                    if (hitInfo.collider.tag == "Ground" && ((((hitInfo.collider.ClosestPoint(transform.position).x - transform.position.x) * (hitInfo.collider.ClosestPoint(transform.position).x - transform.position.x)) + ((hitInfo.collider.ClosestPoint(transform.position).z - transform.position.z) * (hitInfo.collider.ClosestPoint(transform.position).z - transform.position.z))) < 0.57f))
                    {
                        OnWallR = true;
                        wallForce = hitInfo.collider.ClosestPoint(transform.position) - transform.position;
                        absolute(ref wallForce);
                }

            }
        }

        if (Front && !Slide && !OnGround)
        {

            rayDir = Vector3.zero;
            rayDir.x = xForce;
            rayDir.z = zForce;
            if (Physics.Raycast(transform.position, rayDir, out hitInfo, 0.8f))
            {
                if (hitInfo.collider.tag == "Ground")
                {
                    rayDir = Vector3.zero;
                    rayDir.x = hitInfo.collider.ClosestPoint(transform.position).x - transform.position.x;
                    rayDir.z = hitInfo.collider.ClosestPoint(transform.position).z - transform.position.z;
                    if (Physics.Raycast(transform.position, rayDir, out hitInfo, transform.localScale.x * 0.6f))
                    {
                        if (hitInfo.collider.tag == "Ground")
                        {
                            OnWallF = true;
                            wallForce = hitInfo.collider.ClosestPoint(transform.position) - transform.position;
                            absolute(ref wallForce);
                        }
                    }
                }
            }
        }
    }
    void orientation(out float fx, out float fz, int force)
    {
        //getting the player's rotation on the y axis in degrees
        float rot = CamRot.localRotation.eulerAngles.y;
        //getting the players rotation on the y axis in degrees
        rot = rot * Mathf.Deg2Rad;

        //declaring calculation variables
        float opp;
        float adj;
        int turns = 0;
        //setting the angle's value to an angle < 90 so the tan and sin can be used
        while (rot >= 90)
        {
            //decreasing the value by 90 and counting the number of times its decreased
            rot -= 90;
            turns++;
        }
        //workaround for the angle being 0
        if (rot == 0)
        {
            fx = 0;
            fz = force;
        }
        else
        {
            //calculating the vectors with the hypotenuse being the force, and using sine and tangent to find the values with the rotation as the angle
            opp = Mathf.Sin(rot) * force;
            adj = opp / (Mathf.Tan(rot));
            //adjusting the values and assigning the fx and fx variables based on how many times the angle's value was reduced by 90
            switch (turns)
            {
                case 0:
                    fx = opp;
                    fz = adj;
                    break;
                case 1:
                    fx = adj;
                    fz = -opp;
                    break;
                case 2:
                    fx = -opp;
                    fz = -adj;
                    break;
                case 3:
                    fx = -adj;
                    fz = opp;
                    break;
                default:
                    fx = 0;
                    fz = force;
                    break;

            }
        }
    }

    void player_input()
    {
        Left = false;
        //taking input for the 'A' key
        if (Input.GetKey(KeyCode.A))
        {
            Left = true;
        }
        Right = false;
        //taking input for the 'D' key
        if (Input.GetKey(KeyCode.D))
        {
            Right = true;
        }
        Front = false;
        //taking input for the 'W' key
        if (Input.GetKey(KeyCode.W))
        {
            Front = true;
        }
        Back = false;
        //taking input for the 'S' key
        if (Input.GetKey(KeyCode.S))
        {
            Back = true;
        }

        Jump = false;
        //taking input for the spacebar
        if (Input.GetKey(KeyCode.Space))
        {
            Jump = true;
        }

        //taking input for the left control
        if (Input.GetKey(KeyCode.LeftControl) && !Jump)
        {
            Slide = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl) || Jump)
        {
            Slide = false;
        }
    }

    //countermovement
    void friction()
    {
        //Debug.Log(rb.velocity);
        //checking if the speed is more than the max force
        if (OnGround && !(OnWallF || OnWallL || OnWallR))
        {
            if (rb.velocity.x > MAXFORCE)
            {
                rb.AddForce(-(rb.velocity.x + MAXFORCE), 0, 0);
            }
            if (rb.velocity.x < -MAXFORCE)
            {
                rb.AddForce(-rb.velocity.x + MAXFORCE, 0, 0);
            }
            if (rb.velocity.z > MAXFORCE)
            {
                rb.AddForce(0, 0, -(rb.velocity.z + MAXFORCE));
            }
            if (rb.velocity.z < -MAXFORCE)
            {
                rb.AddForce(0, 0, -rb.velocity.z + MAXFORCE);
            }
        }
        //static friction
        if (OnGround && !Front && !Back && !Left && !Right && !Slide)
        {
            rb.AddForce(-rb.velocity.x * Time.deltaTime * FRICTION * 4, 0, -rb.velocity.z * Time.deltaTime * FRICTION * 4 );
        }
        if ((OnGround || JumpToAir) && !Slide && !(OnWallL || OnWallR))
        {
            rb.AddForce(-rb.velocity.x * Time.deltaTime * FRICTION, 0, -rb.velocity.z * Time.deltaTime * FRICTION);
        }
        else if (Slide && !Front && OnGround)
        {
            rb.AddForce(-rb.velocity.x * Time.deltaTime * (FRICTION / 6), 0, -rb.velocity.z * Time.deltaTime * (FRICTION / 6));
        }
    }

    void allow_jump()
    {
        CanWJump = true;
        ResetWJump = true;
    }
    void absolute(ref Vector3 var)
    {
        if (var.x > 0.1)
        {
            var.x = 0.75f;
        }
        else if (var.x < -0.1)
        {
            var.x = -0.75f;
        }
        else
        {
            var.x = 0f;
        }
        if (var.z > 0.1)
        {
            var.z = 0.75f;
        }
        else if (var.z < -0.1)
        {
            var.z = -0.75f;
        }
        else
        {
            var.z = 0f;
        }
        var.y = 0f;
    }
    
    public bool inAir;
    void left_ground()
    {
        if (!OnGround && inAir)
        {
            JumpToAir = true;
        }
        else if (!OnGround)
        {
            inAir = false;
        }
        if (OnWallF || OnWallL || OnWallR || OnGround)
        {
            JumpToAir = false;
        }
    }
}