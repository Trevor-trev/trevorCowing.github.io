using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeClimb : MonoBehaviour
{
	private Rigidbody2D rb;
	public Playermovement pmov;
	public GroundCheck groundCheck;
	public PogoController pogo;
	public PoleClimbController poleClimb;

	public Animator animator;
	
	CapsuleCollider2D capCollider;
	PolygonCollider2D polyCollider;

	public LayerMask whatIsClimbable;//--------------------------A layermask that lets you determine what layers are considered climbable
	
	public Transform wallCheck;//--------------------------------A transform on the players head that detects if the player is touching something designated as climbable
    public Transform ledgeCheck;//-------------------------------A transform above the players head that-when used with the wallCheck-detects if the player is touching a ledge

	public float ledgeClimbXROffset1 = 0f;//---Offsets the character sprite at the X axis when the ledge is grabbed and character is facing right
	public float ledgeClimbYROffset1 = 0f;//---Offsets the character sprite at the Y axis when the ledge is grabbed and character is facing right

	public float ledgeClimbXROffset2 = 0f;//---Offsets the character sprite at the X axis in the last frame of the climbing right animation
	public float ledgeClimbYROffset2 = 0f;//---Offsets the character sprite at the Y axis in the last frame of the climbing right animation

	public float ledgeClimbXLOffset1 = -1.1f;//Offsets the character sprite at the X axis when the ledge is grabbed and character is facing left
	public float ledgeClimbYLOffset1 = 0.3f;//-Offsets the character sprite at the Y axis when the ledge is grabbed and character is facing left

	public float ledgeClimbXLOffset2 = 1.3f;//-Offsets the character sprite at the X axis in the last frame of the climbing left animation
	public float ledgeClimbYLOffset2 = 2.1f;//-Offsets the character sprite at the Y axis in the last frame of the climbing left animation


	[SerializeField] public float wallCheckLength;//-------------The length of the raycast used to check for walls 
	[SerializeField] private bool isTouchingLedge;//-------------1st variable for weather or not the character is touching something designated as climbable
	public bool isTouchingWall;//--------------------------------2nd variable for weather or not the character is touching something designated as climbable
	public bool ledgeDetected;//---------------------------------Weather or not the character is touching a ledge. Becomes true when isTouchingWall is true but isTouchingLedge is false
	public bool ledgeHang = false;//-----------------------------Weather or not the character is hanging from a ledge	
	public bool ledgeClimb;//------------------------------------Weather or not the character is climbing a ledge

	private Vector2 ledgePosBot;//-------------------------------The position of the bottom of the ledge which is currently being grabbed
	private Vector2 ledgePos1;//---------------------------------The position of the character sprite when the ledge is grabbed
	private Vector2 ledgePos2;//---------------------------------The position of the character sprite when the climbing animation is finished


	private void Start()
	{
		polyCollider = GetComponent<PolygonCollider2D>();//---------A polygon collider component attached to the character
		capCollider = GetComponent<CapsuleCollider2D>();//----------A capsule collider component attached to the character
		rb = GetComponent<Rigidbody2D>();//-------------------------The rigidbody component attached to the character
	}

	private void CheckSurroundings()//------------------------------A function for checking weather or not the character is touching a wall or a ledge
	{
		isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckLength, whatIsClimbable);//--A raycast that extends out from the wallCheck position and checks if something designated as climbable is within reach
		isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckLength, whatIsClimbable);//Another raycast that extends out from the ledgeCheck position and checks if something designated as climbable is within reach

		if (isTouchingWall && !isTouchingLedge && !ledgeDetected){//If ledgeDetected is false, and the wallcheck raycast detects something climbable but the ledgecheck raycast doesnt		
			ledgeDetected = true;//--------------------------------A climbable ledge is within reach
			ledgePosBot = wallCheck.position;//--------------------The bottom of the grabbed ledge is set to the position where the wallcheck raycast hits the ledge
			pmov.horizontalMove = 0;}
		else if ((isTouchingWall && isTouchingLedge) || (!isTouchingWall && !isTouchingLedge))//If both wallcheck and ledgecheck raycasts detect something climbable, or neither of them detect something climbable
			ledgeDetected = false;//------------------------------------------------------------There is no climbable ledge within reach 
	}

	private void CheckLedgeClimb()//-----------------------------------A function which executes a ledge hang and ledge climb when the specified conditions are met
	{
		if (ledgeDetected && !ledgeHang && !pogo.onPogo && !ledgeClimb)//If not on the pogo stick, not hanging or climbing a ledge, but a ledge is detected
		{
			pogo.acceleration = 1;//-----------------------------------Lower the horizontal acceleration variable

			if (pmov.facingRight){//-----------------------------------If the character is facing right
				ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckLength) - ledgeClimbXROffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYROffset1);//-Set the character sprite to the appropriate position while hanging
				ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckLength) + ledgeClimbXROffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYROffset2);}//Set the character sprite to the appropriate position when the ledge climb is finished
			else if (pmov.facingLeft){//-------------------------------If the character is facing left
				ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckLength) + ledgeClimbXLOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYLOffset1);//-Set the character sprite to the appropriate position while hanging
				ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckLength) - ledgeClimbXLOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYLOffset2);}//Set the character sprite to the appropriate position when the ledge climb is finished

			if (pmov.facingRight && !groundCheck.grounded && pmov.xdirection > 0)//-------------------------If the character is not grounded and the player is moving the character toward the ledge
				ledgeHang = true;//-------------------------------------------------------------------------The character grabs onto the ledge	

			if (pmov.facingLeft && !groundCheck.grounded && pmov.xdirection < 0 && !poleClimb.onPole)
				ledgeHang = true;
		}

		if (ledgeHang)//-----------------------------------------------------If the character is hanging from a ledge
		{
			animator.SetBool("IsFalling", false);

			polyCollider.enabled = false;//----------------------------------Turn off the character's polygon collider
			capCollider.enabled = false;//-----------------------------------Turn off the character's capsule collider

			transform.position = ledgePos1;//--------------------------------Set the character's position to the appropriate spot on the ledge

			rb.velocity = new Vector2(0, 0);//-------------------------------Make sure the character's rigidbody cannot move due to outside forces		

			animator.SetBool("IsHanging", true);//---------------------------Play the ledge hang animation

			if (poleClimb.verticalMove > 0)//--------------------------------If the player presses the up button
				ledgeClimb = true;//-----------------------------------------The character climbs the ledge

			if (poleClimb.verticalMove < 0){//-------------------------------If the player presses the down button			
				ledgeHang = false;//-----------------------------------------The character drops off the ledge
				polyCollider.enabled = true;//-------------------------------Turn on the polygon collider
				capCollider.enabled = true;}//-------------------------------Turn on the capsule collider

			if (pmov.xdirection != 0)//--------------------------------------If The player presses the left or right buttons
				pmov.horizontalMove += pogo.acceleration * pmov.xdirection;//Increase the horizontal move value according to the acceleration variable in the direction pressed
			//^^^^^^^^^^^^^^^^^This allows for a brief pause between grabbing the ledge and climbing the ledge if the forward button is continually held down^^^^^^^^^^^^^^^^^^

			if (pmov.xdirection == 0)//--------------------------------------If the player is not pressing the left or right buttons
				pmov.horizontalMove = 0;//-----------------------------------Reset the horizontal move value to zero

			if (pmov.facingRight){//-----------------------------------------If the character is facing right
				if (pmov.horizontalMove < 0)//-------------------------------If the player is pressing the left button
					pmov.horizontalMove = 0;//-------------------------------Keep the horizontal move value at zero

				if (pmov.horizontalMove >= 40)//-----------------------------If the horizontal move value is greater than or equal to twenty
					ledgeClimb = true;}//------------------------------------The character climbs the ledge

			if (pmov.facingLeft){//------------------------------------------If the character is facing left
				if (pmov.horizontalMove > 0)//-------------------------------If the player is pressing the right button
					pmov.horizontalMove = 0;//-------------------------------Keep the horizontal move value at zero

				if (pmov.horizontalMove <= -40 && !poleClimb.onPole)//-------If the horizontal move value is less than or equal to twenty
					ledgeClimb = true;}//------------------------------------The character climbs the ledge

		}
		if (!ledgeHang){//---------------------------------------------------If the character is not hanging from a ledge

			animator.SetBool("IsHanging", false);//--------------------------Make sure the ledge hang animation isn't playing

			if (pmov.facingRight)//------------------------------------------If the charactter is facing right
				wallCheckLength = .3f;//-------------------------------------Set the wallcheck raycast to the appropriate length

			if (pmov.facingLeft)//-------------------------------------------If the character is facing left
				wallCheckLength = -.5f;}//-----------------------------------Set the wallcheck raycast to the appropriate length

		if (ledgeClimb){//---------------------------------------------------If the character is climbing a ledge
			pogo.onPogo = false;//-------------------------------------------Make sure the pogo stick cannot be activated
			rb.velocity = new Vector2(0, 0);//-------------------------------Make sure the character's rigidbody cannot move due to outside forces
			rb.gravityScale = 0;//-------------------------------------------Make sure gravity cannot affect the character's rigidbody
			animator.SetBool("IsClimbingLedge", true);//---------------------Play the ledge climb animation
			ledgeHang = false;}//--------------------------------------------Make sure the character is no longer hanging from the ledge
	}
	public void IncreaseAccelForLedgeClimb()//-------------------------------A function which is set as an event on a frame of the ledge hang animation clips
	{
		pogo.acceleration = 30;//--------------------------------------------Increase the acceleration for horizontal move so that the player only has to tap the forward button once to climb the ledge
	}
	public void FinishLedgeClimb()//-----------------------------------------A function which is set as an event on a frame of the ledge climb animation clips
	{
		ledgeClimb = false;//------------------------------------------------Make sure the character doesn't continually climb
		transform.position = ledgePos2;//------------------------------------Set the character's sprite to the appropriate position when the climb is finished
		animator.SetBool("IsClimbingLedge", false);//------------------------Make sure the ledge climb animation is no longer playing
		polyCollider.enabled = true;//---------------------------------------Enable the character's polygon collider
		capCollider.enabled = true;//----------------------------------------Enable the character's capsule collider
	}

	private void OnDrawGizmos()//--------------------------------------------A function that allows us to see the wallcheck and ledgecheck raycasts in the scene view
	{
		Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckLength, wallCheck.position.y, wallCheck.position.z));
		Gizmos.DrawLine(ledgeCheck.position, new Vector3(ledgeCheck.position.x + wallCheckLength, ledgeCheck.position.y, ledgeCheck.position.z));
	}

	private void Update()
	{
		CheckLedgeClimb();//------------------------------------------------Keep checking weather or not the player is telling the character to grab or climb a ledge
	}

	private void FixedUpdate()
	{
		CheckSurroundings();//----------------------------------------------Keep checking weather or not the character is next to a wall or ledge
	}
}
