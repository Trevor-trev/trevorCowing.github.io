using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

public class Playermovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    public GroundCheck groundCheck;
    public LedgeClimb ledgeClimb;
    public Pole pole;
    public NeuralGun neuralGun;
    public RecenterTriggerGround recenterTrigger;
    public SlopeCheck slopeCheck;    
    public PoleClimbController poleClimb;
    public PogoController pogo;
    public MovingPlatformCheck movPlatCheck;
    public DoorwayCheck doorwayCheck;

    public GameObject closestmovPlat;

    //MOVEMENT//////
    public bool facingRight;//-------------------Whether or not the character is facing right
    public bool facingLeft;//--------------------Whether or not the character is facing left 
   
    public float xdirection = 0f;//--------------Direction check used for animation and pogo stick acceleration
    public float xdirForTransitionToPogo = 0f;//-Direction check used when activating the pogo stick while running      
    public float horizontalMove = 0f;//----------Velocity variable used for non pogo stick horizontal movement
    public float topSpeedR = 37.5f;//------------Set speed limit for horizontal movement to the right
    public float topSpeedL = -37.5f;//-----------Set speed limit for horizontal movement to the left
    
    //JUMPING//
    public bool isJumping;//---------------------Whether or not the character is jumping
    public bool rising;//------------------------Whether or not the character is moving upward 
    public bool falling;//-----------------------Whether or not the character is moving downward
   
    public float jumpForce;//--------------------The amount of vertical force applied when the player presses the jump button   
    public float jumpTimer;//--------------------Counts down to 0, at which point the player can no longer jump
    private float jumpTimerStart = 0.31f;//------The starting point for the jumpTimeCounter
    public float thrust = -.5f;

    //LOOKING//
    public bool lookDown = false;//--------------Whether or not the character is looking down
    public bool lookup = false;//----------------Whether or not the character is looking up
    public bool uncrouching;//-------------------Whether or not the character is looking back up from the crouching(lookingdown) position
    public bool touchingCeiling;//---------------Whether or not the character's head is touching a ceiling

    public const float ceilingRadius = .2f; //---Radius of the overlap circle to determine if the player can stand up
   
    public Transform ceilingCheck;//-------------A position marking where to check for ceilings
    public Collider2D lookDownDisableCollider;//-A collider that will be disabled when looking down




    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator.SetBool("FacingLeft", true);//Make sure the animator registers the player as facing left when the game starts
        facingLeft = true;//Make sure the character is facing left when the game starts
    }

#region UNCROUCHING COROUTINE
    IEnumerator UnCrouching()
    {
        if (!poleClimb.onPole){//------------------------------If the character is not on a pole        
            uncrouching = true;//--------------------Set the uncrouching variable to true
            topSpeedL = 0f;//------------------------Prevent the character from moving to the left
            topSpeedR = 0f;//------------------------Prevent the character from moving to the right
            animator.SetBool("UnCrouching", true);//-Play the uncrouching animation
            yield return new WaitForSeconds(.08f);//-Wait until the uncrouching animation finishes
            animator.SetBool("UnCrouching", false);//Prevent the uncrouching parameter from always being true
            lookDown = false;//----------------------Set the looking down variable back to false
            topSpeedL = -37.5f;//--------------------Allow the character to move to the left
            topSpeedR = 37.5f;//---------------------Allow the character to move to the right
            uncrouching = false;}//------------------Set the uncroucing variable to false
    }
    #endregion
    void Update()
    {
        closestmovPlat = GameObject.FindGameObjectWithTag("ClosestMovingPlatform");
        #region JUMP CONTROLLER
        ///////////////////////////////////JUMPING//////////////////////////////////////


        if ((groundCheck.grounded || recenterTrigger.touchingDroppablePlatform) && (lookDown || lookup)){//If the chracter is on the ground or a droppable platform and is looking up or down            
            jumpForce = 0;//---------------------------------------------------Prevent the character from jumping
            isJumping = false;}//----------------------------------------------Prevent isJumping from being true, this prevents the character from slowly falling if the jump button is held when dropping through a platform        
        else if (groundCheck.grounded && !lookDown && !lookup)//---------------If the character is grounded, not looking up, and not looking down
            jumpForce = 12f;//-------------------------------------------------Allow the character to jump

        if (!pogo.onPogo && !ledgeClimb.ledgeClimb)//--------------------------If the character is not on the pogo stick and not climbing a ledge
        {
            if (!neuralGun.shoot && (groundCheck.grounded == true) && Input.GetButtonDown("Jump")){//When the character is grounded and the player presses the jump button            
                rb.velocity = Vector2.up * jumpForce;//------------------------Create vertical force
                isJumping = true;//--------------------------------------------Set the isJumping variable to true
                jumpTimer = jumpTimerStart;}//---------------------------------Make sure the counter resets when the character is no longer airborne
            
            if (Input.GetButton("Jump") && isJumping == true){//---------------If the jump button is held down and the isJumping variable is true            
                if (jumpTimer > 0){//------------------------------------------If the counter is greater than zero                
                    rb.velocity = Vector2.up * jumpForce;//--------------------Allow the character to jump
                    jumpTimer -= Time.deltaTime;}}//---------------------------Make the timer starts counting down                                
        }
        if (Input.GetButtonUp("Jump") || rb.velocity.y < 0 || touchingCeiling || neuralGun.shoot)//-----------------------------------When the player releases the jump button
            isJumping = false;//-----------------------------------------------Set the isJumping variable to false 
        #endregion

#region LOOKING DOWN CONTROLLER
        ///////////////////////////////////LOOKING DOWN//////////////////////////////////////      
        if (!poleClimb.onPole)
        {//-------------------------------------------------------------------------------------------------If the character is not on a pole                  
            if (Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, groundCheck.whatIsCeiling))//-If there is an object directly above the characters head
                touchingCeiling = true;
            else if (!Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, groundCheck.whatIsCeiling))
                touchingCeiling = false;
           
            if (lookDown && groundCheck.grounded && touchingCeiling)
                if (lookDownDisableCollider != null)//------------------------------------------------------If there is one or more colliders set to disable when crouching
                        lookDownDisableCollider.enabled = false;}//----------------------------------------------------------Disable the collider(s)  

            if ((!lookDown && !ledgeClimb.ledgeClimb && !touchingCeiling) || !groundCheck.grounded)//-------If the chracter is not looking down and there is no cieling above the character's head
                if (lookDownDisableCollider != null)//------------------------------------------------------If there is one or more colliders set to disable when crouching
                    lookDownDisableCollider.enabled = true;//-----------------------------------------------Enable the collider when not crouching
             
            if (groundCheck.grounded && lookDown){//--------------------------------------------------------If the character is looking down             
                topSpeedL = 0;//----------------------------------------------------------------------------Prevent the character from moving to the left
                topSpeedR = 0;}//---------------------------------------------------------------------------Prevent the character from moving to the right        
        #endregion

#region ANIMATION CONTROLLER
        ///////////////////////////////////ANIMATION//////////////////////////////////////

#region RUNNING ANIMATION
        ///////////////////RUNNING ANIMATION///////////////////

        if (!ledgeClimb.ledgeClimb)//--------------------------------------------------If the character is not hanging onto or climbing a ledge
            xdirection = Input.GetAxisRaw("Horizontal");//-----------------------------Check which direction input is being pressed
        else if (ledgeClimb.ledgeClimb)//----------------------------------------------Otherwise if the character is climbing a ledge
            xdirection = 0;//----------------------------------------------------------Dont acknowledge any horizontal input

        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));//------------------------Set the speed parameter for animations to be equal to the absolute value of the character's horizontal velocity

        if (!ledgeClimb.isTouchingWall && groundCheck.grounded && horizontalMove != 0)//If the chaaracter is not touching a wall, is grounded, and is moving horizontally
            animator.SetBool("IsRunning", true);//-------------------------------------Allow the running animation to play
        else if (ledgeClimb.isTouchingWall || horizontalMove == 0)//-------------------If the character is touching a wall or not moving horizontally
            animator.SetBool("IsRunning", false);//------------------------------------Prevent the running animation from playing

        if (!pogo.onPogo && !ledgeClimb.ledgeHang && !neuralGun.shoot && !ledgeClimb.ledgeDetected)//If the character is not on the pogo stick, not hanging from a ledge, not shooting, and there is no ledge detected
        {
            if (xdirection > 0){//-----------------------------------------------------If the the player presses the right button      
                facingRight = true;//--------------------------------------------------The character is facing right
                facingLeft = false;//--------------------------------------------------The character is not facing left
                animator.SetBool("FacingRight", true);//-------------------------------Only play right facing animations
                animator.SetBool("FacingLeft", false);}//------------------------------Do not play left facing animations

            if (xdirection < 0){//-----------------------------------------------------If the player presses the left button
                facingRight = false;//-------------------------------------------------The character is not facing right
                facingLeft = true;//---------------------------------------------------The character is facing left
                animator.SetBool("FacingLeft", true);//--------------------------------Only play left facing animations
                animator.SetBool("FacingRight", false);}//-----------------------------Do not play right facing animations
        }

        if (pogo.onPogo)//-------------------------------------------------------------If the character is on the pogo stick
        {
            if (pogo.pogoSpeed > 0)//--------------------------------------------------If the character is moving to the right 
            {
                facingRight = true;//--------------------------------------------------The character is facing right
                facingLeft = false;//--------------------------------------------------The character is not facing left
                animator.SetBool("FacingRight", true);//-------------------------------Only play right facing animations
                animator.SetBool("FacingLeft", false);//-------------------------------Do not play the left facing animations
            }

            if (pogo.pogoSpeed < 0)//--------------------------------------------------If the character is moving to the left
            {       
                facingRight = false;//-------------------------------------------------The character is not facing right
                facingLeft = true;//---------------------------------------------------The character is facing left
                animator.SetBool("FacingLeft", true);//--------------------------------Only play left facing animations
                animator.SetBool("FacingRight", false);//------------------------------Do not play right facing animations
            }
        }
        #endregion

#region RISING AND FALLING ANIMATION

        ///////////////////RISING AND FALLING ANIMATION/////////////////        
        if ((!groundCheck.grounded && rb.velocity.y > 0) || Input.GetButtonDown("Jump")){//If the character is not grounded and is moving upward or if the player presses the jump button
            rising = true;//---------------------------------------------------------The character is rising
            falling = false;
            animator.SetBool("IsRising", true);//------------------------------------Set the IsRising animation parameter to true
            animator.SetBool("IsFalling", false);}//---------------------------------Set the IsFalling animation parameter false
        else if (!groundCheck.grounded && rb.velocity.y < 0){//----------------------If the character is not grounded and is moving downward
            rising = false;//--------------------------------------------------------The character is not rising
            falling = true; 
            animator.SetBool("IsRising", false);//-----------------------------------Set the IsRising animation parameter to false
            animator.SetBool("IsFalling", true);}//----------------------------------Set the IsFalling animation parameter to true
        else if (groundCheck.grounded || !isJumping || rb.velocity.y == 0 && !(lookup || lookDown)){//If the character is grounded, or not jumping, or has a vertical velocity of 0, and not looking up or down
            rising = false;//--------------------------------------------------------The character is not rising
            falling = false;//-------------------------------------------------------The character is not falling
            animator.SetBool("IsRising", false);//-----------------------------------Set the IsRising animation parameter to false
            animator.SetBool("IsFalling", false);}//---------------------------------Set the IsFalling animation parameter to false
        #endregion

#region LOOK DOWN ANIMATION

        ///////////////////LOOK DOWN ANIMAITON////////////////      

        if (!pogo.onPogo)//-----------------------------------If the character is not on the pogo stick
        {
            if (!uncrouching && Input.GetButton("LookDown"))//If the player presses the down button
            {
                lookDown = true;//----------------------------The character is looking down
                if (!poleClimb.onPole)//----------------------If the character is not on a pole
                    animator.SetBool("LookingDown", true);//--Play the looking down (crouching) animation
            }

            if (!Input.GetButton("LookDown") && lookDown == true){//If the player releases the look down button          
                lookDown = false;//---------------------------------The lookdown bool becomes false
                StartCoroutine("UnCrouching");//--------------------Start the uncrouching coroutine
                animator.SetBool("LookingDown", false);}//----------Stop playing the looking down animation
        }

        if (poleClimb.onPole){//------------------------------------If the character is on a pole           
            animator.SetBool("LookingDown", false);}//--------------Prevent the looking down animation from playing
        #endregion

#region LOOK UP ANIMATION
        ///////////////////LOOKING UP ANIMATION////////////////       

            if (Input.GetButton("LookUp") && !doorwayCheck.inDoorway){//If the player is pressing the look up button while not standing in an open doorway               
                lookup = true;//----------------------------------------The character looks up
                if (!poleClimb.onPole)
                    animator.SetBool("LookingUp", true);}//-------------Play the looking up animation
            
            if (!Input.GetButton("LookUp")){//--------------------------If the player releases the up button
                lookup = false;//---------------------------------------The character is no longer looking up
                animator.SetBool("LookingUp", false);}//----------------Stop the looking up animation       

        if (poleClimb.onPole){//----------------------------------------If the character is on a pole          
            animator.SetBool("LookingUp", false);}//--------------------Prevent the looking up animation from playing
        #endregion
        #endregion
    }
    void FixedUpdate()
    {
#region Movement
        ///////////////////////////////////Movement//////////////////////////////////////               
        if (!pogo.onPogo && !poleClimb.onPole && !ledgeClimb.ledgeHang && !ledgeClimb.ledgeClimb)//----If the character is not on a pogo stick, not on a pole, not hanging from or climbing a ledge
            {
            if (groundCheck.grounded && (neuralGun.stopMovement || lookDown || doorwayCheck.walkingThroughDoor))//If the character is grounded and is shooting, looking down, or walking through a doorway
                rb.velocity = new Vector2(0, 0);//-----------------------------------------------------Prevent the character from moving
            else//-------------------------------------------------------------------------------------Otherwise
                rb.velocity = new Vector2(horizontalMove * Time.fixedDeltaTime * 10f, rb.velocity.y);//Set the characters horizontal movement according to the horizontal move variable, and vertical movement according to the vertical forces applied to their rigidbody

            pogo.acceleration = 2f;//------------------------------------------------------------------Set the acceleration variable           
            pogo.pogoSpeed = 0;//----------------------------------------------------------------------Make sure the pogo stick speed is reset to zero

            if (movPlatCheck.onMovingPlatform)
                {
                    if (closestmovPlat.GetComponent<PlatformDrop>().platformFlip && closestmovPlat.GetComponent<VPlatformMovement>().movingDown)
                        rb.AddForce(transform.up * thrust, ForceMode2D.Impulse);
                }

            if (groundCheck.grounded)
                {//-----------------------------------------------------------------------------------------if the character is grounded                                                     
                    if (facingRight)
                        horizontalMove = Input.GetAxisRaw("Horizontal") * topSpeedR;//----------------------Make snappy movement controls
                    if (facingLeft)
                        horizontalMove = Input.GetAxisRaw("Horizontal") * topSpeedL * -1f;//----------------Make snappy movement controls
                    if (movPlatCheck.onMovingPlatform)//----------------------------------------------------If the character is on a moving platform
                    {
                        if (closestmovPlat.GetComponent<HPlatfomMovement>().movingRight && facingRight)
                            topSpeedR = 50f;//--------------------------------------------------------------Set the top speed when moving right
                        if (closestmovPlat.GetComponent<HPlatfomMovement>().movingRight && facingLeft)
                            topSpeedL = -15f;//-------------------------------------------------------------Set the top speed when moving left

                        if (closestmovPlat.GetComponent<HPlatfomMovement>().movingLeft && facingLeft)
                            topSpeedL = -50f;
                        if (closestmovPlat.GetComponent<HPlatfomMovement>().movingLeft && facingRight)
                            topSpeedR = 15f;
                    }
                    if (!movPlatCheck.onMovingPlatform)
                    {
                        topSpeedR = 35f;//------------------------------------------------------------Set the top speed when moving right
                        topSpeedL = -35f;//-----------------------------------------------------------Set the top speed when moving left
                    }
                }

                if (!groundCheck.grounded)//-----------------------------------------------------------When the character is not grounded                                                                            
                {
                    topSpeedR = 35f;//-----------------------------------------------------------------Slightly decrease the top speed when moving right
                    topSpeedL = -35f;//----------------------------------------------------------------Slightly decrease the top speed when moving left
                    if (xdirection != 0)//-------------------------------------------------------------When the player is pressing the left or right buttons
                        horizontalMove += pogo.acceleration * xdirection;//----------------------------Make horizontal movement less snappy by incorporating the acceleration variable

                    if (horizontalMove > topSpeedR)//--------------------------------------------------Prevent the player from exceeding the top speed in the right direction
                        horizontalMove = topSpeedR;

                    if (horizontalMove < topSpeedL)//--------------------------------------------------Prevent the player from exceeding the top speed in the left direction
                        horizontalMove = topSpeedL;

                    if (xdirection == 0)//-------------------------------------------------------------If the player is not inputting a left or right direction
                    {
                        if (horizontalMove < 0)//------------------------------------------------------If the character was moving to the left  
                            horizontalMove += pogo.acceleration * .7f;//-------------------------------Decelerate the character's movement back to zero by incrementing the horizontal move variable to the right
                        if (horizontalMove > 0)//------------------------------------------------------If the character was moving to the right
                            horizontalMove -= pogo.acceleration * .7f;//-------------------------------decelerate the character's movement back to zero by incrementing the horizontal move variable to the left
                    }
                }
            }
        #endregion
    }
}