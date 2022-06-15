using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    //getting player position
    public Transform PlayerPos;
    //declaring the variables that will store the rotation of the camera in x and y in 3D
    private float yRot;
    private float xRot;
    private float zRot;
    //declaring the variables that will be calculated from values in 2D to add to the 3D rotation to rotate the player
    private float yChange;
    private float xChange;
    //declaring the variables that store the location of the mouse pointer on the 2D screen
    private float mouseY;
    private float mouseX;
    //declaring the sensitivity of the mouse
    public float sensitivity = 100;
    //declaring the camera offset
    public Vector3 offset;
    //declaring the values that store whether or not the player is on the left or right wall
    public bool onWallL = false;
    public bool onWallR = false;
    private bool OnGround = true;
    private bool prevOnGround = true;
    public bool landed = false;
    public bool Slide = false;
    public bool Jump = false;
    //declaring the variable that will be incremented to help smooth camera turning
    public float smooth = 0;
    public int smoothFac = 4;
    public float cushion = 0;
    public bool cushioning = false;
    private int multSmooth = 90;

    // Start is called before the first frame update
    void Start()
    {
        //locks and hides the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        //initialises the 'Rot' values
        yRot = transform.localRotation.y;
        xRot = transform.localRotation.x;
    }

    // Update is called once per frame
    void Update()
    {
        OnGround = FindObjectOfType<PlayerMovement>().OnGround;
        Slide = FindObjectOfType<PlayerMovement>().Slide;
        Jump = FindObjectOfType<PlayerMovement>().Jump;
        landed = false;
        if (!prevOnGround && OnGround && !Slide)
        {
            landed = true;
            if (!Jump)
            {
                cushioning = true;
            }
            cushion += smoothFac * 3 * Time.deltaTime * smooth;
        }
        if (cushioning)
        {
            CushionLanding();
        }

        Debug.Log(landed);
        if (!cushioning)
        {
            cushion = 0;
            offset.y = 1;
        }

        if (!Slide)
        {
            //teleporting the camera to the player and adding an offset
            transform.position = PlayerPos.position + offset;
        }
        else
        {
            transform.position = PlayerPos.position;
        }
        //getting the location of the cursor on the x and y axes
        mouseY = Input.GetAxis("Mouse Y");
        mouseX = Input.GetAxis("Mouse X");
        //calculating how much to change the rotation by and multiplying it by the sensitivity
        yChange = mouseY * sensitivity * Time.fixedDeltaTime;
        xChange = mouseX * sensitivity * Time.fixedDeltaTime;
        //assigning the 2D values to the 3D values and adjusting for the inverse on the x rotation
        xRot += -yChange;
        yRot += xChange;
        if (xRot < -80)
        {
            xRot = -80;
        }
        else if (xRot > 80)
        {
            xRot = 80;
        }
        //getting the values for onwallr and onwalll and applying rotation
        onWallL = FindObjectOfType<PlayerMovement>().OnWallL;
        onWallR = FindObjectOfType<PlayerMovement>().OnWallR;
        
        if (onWallL && !Jump)
        {
            zRot = 15 * Mathf.Sin(Mathf.Deg2Rad * smooth);
            if (smooth > -90)
            {
                smooth -= smoothFac * Time.deltaTime * multSmooth;
            }
            Debug.Log("tilting for left wall");
        }
         if (onWallR && !Jump)
        {
            zRot = 15 * Mathf.Sin(Mathf.Deg2Rad * smooth);
            if (smooth < 90)
            {
                smooth += smoothFac * Time.deltaTime * multSmooth;
            }
            Debug.Log("tilting for right wall");
        }
       
        if (!onWallL && !onWallR || Jump || OnGround)
        {
            if (smooth > 0)
            {
                smooth -= smoothFac * Time.deltaTime * multSmooth;
            }
            else if (smooth < 0)
            {
                smooth += smoothFac * Time.deltaTime * multSmooth;
            }
            if ((smooth < 0 && smooth > -smoothFac) || (smooth > 0 && smooth < smoothFac))
            {
                smooth = 0;
            }
            zRot = 15 * Mathf.Sin(Mathf.Deg2Rad * smooth);
        }

        //setting the rotation of the player to the 'Rot' values
        transform.rotation = Quaternion.Euler(xRot, yRot, zRot);
        prevOnGround = OnGround;
    }
    private void CushionLanding()
    {
        cushioning = false;
        if (cushion < 150)
        {
            cushioning = true;
            cushion += smoothFac * Time.deltaTime * 200f;
            offset.y = 1;
            offset.y -= 0.5f * Mathf.Sin(Mathf.Deg2Rad * cushion);
        }
        else if (cushion < 180)
        {
            cushioning = true;
            cushion += (smoothFac + 1) * Time.deltaTime * 200f;
            offset.y = 1;
            offset.y -= 0.5f * Mathf.Sin(Mathf.Deg2Rad * cushion);
        }
        Debug.Log(offset.y);
    }
}