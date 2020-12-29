using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoleClimbController : MonoBehaviour
{
    public Rigidbody2D rb;
    public Playermovement pmov;
    public NeuralGun neuralGun;
    public Pole pole;
    public Animator animator;
    public LedgeClimb ledgeClimb;
    public SlopeCheck slopeCheck;

    public bool jumpedFromPole;//----------------Whether or not the character jumped off of a pole
    public bool onPole = false;//----------------Determine whether or not the character is on a pole

    public float climbingTopSpeed = 1.75f;//-----Set speed limit for vertical movement while on a pole
    public float verticalMove = 0f;//------------Velocity variable used for vertical movement while climbing a pole

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ///////////////////////////////////Pole Climbing//////////////////////////////////////   

        if (jumpedFromPole)//------------------------------------------------If the character has jumped off a pole
            onPole = false;//------------------------------------------------Prevent the character from immediately grabbing the pole if the player is holding the up button

        if (onPole && (pmov.lookDown || pmov.lookup) && neuralGun.shoot)//---If the character is on a pole, and the player is pressing the look up or look down buttons while shooting
            verticalMove = 0;//----------------------------------------------Prevent the character from moving up or down 
        else if (!neuralGun.shoot)//-----------------------------------------Otherwise, if the character is not shooting
            verticalMove = Input.GetAxisRaw("Vertical") * climbingTopSpeed;//Allow the character to move up and down poles        

        if (verticalMove > 0)//----------------------------------------------When the character is moving up the pole
            climbingTopSpeed = 2.65f;//--------------------------------------Set the speed a little slower
        else if (verticalMove < 0)//-----------------------------------------When the character is moving down the pole
            climbingTopSpeed = 7.5f;//---------------------------------------Set the speed a little faster      

        if (onPole)//--------------------------------------------------------If the character is on a pole 
        {
            pmov.horizontalMove = 0;//---------------------------------------Prevent the character from moving horizontally
            transform.position = new Vector2(GameObject.FindWithTag("ClosestPole").transform.position.x, transform.position.y);//Align character sprite with the closest pole

            if ((pole.makeshiftGrounded && pole.makeshiftGrounded2) && pmov.xdirection != 0)//If the character's feet are touching the ground while on a pole and the player presses the left or right buttons
                onPole = false;//--------------------------------------------------------Allow the character to get off the pole and walk away

            animator.SetBool("IsOnPole", true);//----------------------------------------Play the idle pole animation
            rb.gravityScale = 0;//-------------------------------------------------------Set gravity to zero to prevent the character from sliding down the pole
            pmov.jumpForce = 10f;//------------------------------------------------------Decrease the jump force
            rb.velocity = new Vector2(0, verticalMove);//--------------------------------Only allow the character to move vertically

            if (Input.GetButtonDown("Jump"))//-------------------------------------------If the player presses the jump button
            {
                pmov.horizontalMove = 0;//-----------------------------------------------Prevent the character from moving forward without player input
                rb.velocity = Vector2.up * pmov.jumpForce;//-----------------------------Allow for a seperate jump height when jumping off of a pole
                jumpedFromPole = true;//-------------------------------------------------Recognize that the player has jumped off a pole
            }
        }
        else if (!onPole && !ledgeClimb.ledgeClimb && !slopeCheck.onSlope)
        {//------------------------------------------------------------------------------When the character is no longer on a pole
            animator.SetBool("IsOnPole", false);//---------------------------------------Make sure to stop playing any pole climbing animations
            rb.gravityScale = 3.5f;//----------------------------------------------------Set gravity back to it's default value
            pmov.jumpForce = 12f;
        }//------------------------------------------------------------------------------Set the jump force back to it's default value


        if (rb.velocity.y <= 0)//--------------------------------------------------------When the character is no longer jumping upward
            jumpedFromPole = false;//----------------------------------------------------Allow the character to once again grab the pole
    }
}
