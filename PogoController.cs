using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PogoController : MonoBehaviour
{
    public Rigidbody2D rb;
    public LedgeClimb ledgeClimb;
    public PoleClimbController poleClimb;
    public Playermovement pmov;
    public GroundCheck groundCheck;
    public Animator animator;
    public DoorwayCheck doorwayCheck;

    public bool onPogo = false;//----------------Determine whether or not the character is on the pogo stick

    private float pogoTopSpeedR = 6.65f;//-------Set speed limit for horizontal movement to the right while on pogo stick
    private float pogoTopSpeedL = -6.65f;//------Set speed limit for horizontal movement to the left while on pogo stick
    public float bounceForce;//------------------The amount of vertical force applied when the pogo stick bounces off of a surface    
    public float acceleration = .25f;//----------Acceleration variable used for horizontal movement while on pogo stick or transitioning from hanging to climbing a ledge
    public float pogoSpeed = 0;//----------------Velocity variable used for pogo stick horizontal movement
    public float impossiblePogoTimer;//----------A timer that determines the button press timing required to execute the impossible pogo trick
    public float impPogoTimerStart = .25f;//-----The starting point for the impossible pogo timer.
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ///////////////////ANIMATION/////////////////

        if (onPogo && !ledgeClimb.ledgeClimb)//-----------If the character is on the pogo stick and not climbing a ledge
        {
            animator.SetBool("LookingDown", false);
            pmov.lookDown = false;//-------------------------Make sure the character doesnt automatically look down if the lookdown button is pressed and pogo is activated before it is let go
            animator.SetBool("IsOnPogo", true);//-------Play the pogo stick animation

            if (groundCheck.grounded)//------------------If the character hits the ground
                animator.SetBool("IsBouncing", true);//-Play the bouncing animation
            else if (!groundCheck.grounded)//------------If the characcter is not touching ground
                animator.SetBool("IsBouncing", false);//Do not play the bouncing animation
        }
        if (!onPogo)//-------------------------------Otherwise, if the character is not on the pogostick
            animator.SetBool("IsOnPogo", false);//------Don't play the pogo stick animation

        ///////////////////CONTROLLER/////////////////
        
        if (!ledgeClimb.ledgeHang && !ledgeClimb.ledgeClimb && !poleClimb.onPole && !doorwayCheck.walkingThroughDoor && Input.GetButtonDown("Pogo"))//If the character is not on a pole and the player presses the pogo button  
            onPogo = !onPogo;//-----------------------------------------------Activate and deactivate the pogo stick

        if (!onPogo)
        {//-------------------------------------------------------------------If the character is not on the pogo stick
            impossiblePogoTimer = impPogoTimerStart;
            pmov.xdirForTransitionToPogo = Input.GetAxisRaw("Horizontal");}//-Set an independent horizontal variable for use with activating the pogo stick

        if (onPogo)//---------------------------------------------------------If the character is on the pogo stick
        {
            if (pmov.xdirForTransitionToPogo != 0)//--------------------------If the character is moving when the pogo stick is activated
                acceleration = 600;//-----------------------------------------Increase acceleration to keep the horizontal velocity constant
            else//------------------------------------------------------------If the character is not moving when the pogo stick is activated
                acceleration = 9.75f;//---------------------------------------Create slow acceleration

            pogoSpeed += acceleration * pmov.xdirection * Time.deltaTime;//--accelerate the character in the appropriate direction

            pmov.jumpTimer = 0;//--------------------------------------------Set the jump timer equal to zero

            rb.gravityScale = 2.5f;//----------------------------------------Set a specific gravity scale for the pogo stick

            rb.velocity = new Vector2(pogoSpeed, rb.velocity.y);//-----------Move the character according to the pogoSpeed variable            

            pmov.xdirForTransitionToPogo = 0;//------------------------------Reset this variable after it is used

            if (groundCheck.grounded)//--------------------------------------When the pogo stick hits the ground
                rb.velocity = new Vector2(rb.velocity.x, bounceForce);//-----Create a vertical force to bouce the character upward

            if (Input.GetButton("Jump"))
            {//---------------------------------------If the player presses or holds the jump button while the character is on the pogo stick
                if (impossiblePogoTimer <= 0)//-------If the impossible pogo trick timer is not greater than zero
                    bounceForce = 22.5f;
            }//---------------------------------------Increase the bounce force 
            else//------------------------------------If the player is not pressing the jump button while on the pogo stick
                bounceForce = 12f;//------------------Set the bounce force to default value

            if (pogoSpeed > pogoTopSpeedR)//----------If the horizontal speed to the right is about to go over the speed limit
                pogoSpeed = pogoTopSpeedR;//----------Keep the speed at the limit

            if (pogoSpeed < pogoTopSpeedL)//----------If the horizontal speed to the left is about to go over the speed limit
                pogoSpeed = pogoTopSpeedL;//----------Keep the speed at the limit

            pmov.horizontalMove = pogoSpeed * 5.64f;//Maintain horizontal speed when transitioning off the pogo stick

        }
    }

    private void FixedUpdate()
    {
        if (onPogo)//-------------------------------------------------------If the character is on the pogo stick
        {
            impossiblePogoTimer -= Time.fixedDeltaTime;//-------------------Make the impossible pogo trick timer count down

            if (impossiblePogoTimer < 0)//----------------------------------If the timer is less than 0
                impossiblePogoTimer = 0;//----------------------------------Keep the timer at zero

            if (Input.GetButton("Jump"))//----------------------------------If the player presses the jump button
                if (impossiblePogoTimer > 0f && impossiblePogoTimer < .5f)//if the timer is greater than 0 but less than 0.5
                    bounceForce = 24f;//------------------------------------increase the bounce force
        }
    }
}
