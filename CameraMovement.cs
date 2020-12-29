using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraMovement : MonoBehaviour
{/* This script is intended to be used with a Cinemachine virtual camera.
    This script should be placed on a game object, possibly titled "CameraController".
    That object should be set as the virtual camera's "Follow" target.
    The "CameraController" object should have a child object attached to it, it should be tagged "CameraPosition" becuase that
    is the tagged referenced in the upper and lower camera stop scripts.
    The "CameraPosition" object should have a trigger collider attached to it
    The "CameraPosition" object should also have a rigid body attached to it, set to dynamic, with gravity scale set to zero.*/
   
    Vector2 tempPos;//--------------------------------A temporary variable for the position of the game object that this script is attached to

    public MovingPlatformCheck movPlatCheck;
    public NeuralGun neuralGun;
    public Rigidbody2D rb;//--------------------------A reference to the rigid body attached to the same game object as this script
    public Rigidbody2D playerRb;//--------------------A reference to the rigid body attached to the character
    public RecenterTriggerGround recenterTrigger;//---A reference to the Recenter Trigger Ground script
    public GroundCheck groundCheck;//--------A reference to the GroundCheck script
    public SlopeCheck slopeCheck;
    public PoleClimbController poleClimb;
    public DoorwaySide1 doorway1;
    public DoorwaySide2 doorway2;
    public DoorwayCheck doorwayCheck;
    public Playermovement pmov;

    public GameObject closestOpenDoorway;
    
    public Transform recenterPointFromDown;//-------------A reference to the recenter point positioned above the character
    public Transform recenterPointFromUp;//---------------A reference to the recenter point positioned below the character
    public Transform character;//-------------------------A reference to the whichever game object you designate as "character"
 
    private float timer;//----------------------------A timer that makes the camera wait a specified time after player input to start moving
    private float timerStart = .5f;//-----------------The starting point for the timer  
    public float cameraSpeed = 15;//------------------How fast the camera moves upon player input
    public float recenterSpeed;//---------------------How fast the camera recenters itself
    public float recenterTime;//----------------------The amount of time the camera is given to recenter itself
    
    public bool lookUpRecenter;//---------------------Weather or not the camera should recenter itself from the player looking up
    public bool lookDownRecenter;//-------------------Weather or not the camera should recenter itself from the player looking down
    public bool walkedThroughDoor;
   
    /*The two different recenter points make sure that the camera centers itself to the same position
     whether it's recentering from the player looking up or recentering from the player looking down
     The reason this is needed is because the "CameraController" object (which is set as the follow target
     in the Cinemachine Virtual Camera) that moves the camera interacts with the virtual camera's deadzones, 
     meaning that it will only move the camera when it reaches a deadzone.*/

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();//--------------------------------------------------------Secifiy rb as the rigidbody that is attached to the same object as this script
        recenterPointFromDown = GameObject.Find("RecenterPointFromDown").transform;//--------------Specify recenterPointDown as the game object titled "RecenterPointFromDown
        recenterPointFromUp = GameObject.Find("RecenterPointFromUp").transform;//------------------Specify recenterPointUp as the game object titled "RecenterPointFromUp"
        tempPos = gameObject.transform.position;//-------------------------------------------------Specify the tempPos as the position of the game object that this script is attached to
        transform.position = new Vector2(character.position.x, recenterPointFromDown.position.y);//When the game starts, set the position of this game object at the character's x position and the recenterPointDown's y position
        timer = timerStart;//----------------------------------------------------------------------When the game starts, set the timer equal to the value of the timer start
    }
    IEnumerator LookUpRecenter()//------------------------------A coroutine labeled "LookUpRecenter". Has the ability to pause and resume execution according to specifications
    {
        lookUpRecenter = true;//--------------------------------Tell the camera's follow target to move to the recenterpoint
        yield return new WaitForSeconds(recenterTime);//--------Wait for the specified amount of time
        lookUpRecenter = false;//-------------------------------Stop telling the camera's follow target to move too the recenterpoint
    }

    IEnumerator LookDownRecenter()//----------------------------A coroutine labeled "LookDownRecenter". Has the ability to pause and resume execution according to specifications
    {
        lookDownRecenter = true;//------------------------------Tell the camera's follow target to move to the recenterpoint
        yield return new WaitForSeconds(recenterTime);//--------Wait for the specified amount of time
        lookDownRecenter = false;//-----------------------------Stop telling the camera's follow target to move too the recenterpoint
    }

    IEnumerator JustLanded()//----------------------------------A coroutine labeled "LookDownRecenter". Has the ability to pause and resume execution according to specifications
    {
        yield return new WaitForSeconds(.1f);//-----------------Wait for the specified amount of time
        transform.position = Vector2.Lerp(transform.position, recenterPointFromDown.position, Time.deltaTime * recenterSpeed);//Move the camera's follow target to the recenterpoint
    }

    IEnumerator WalkedThroughDoor()
    {
        walkedThroughDoor = true;
        yield return new WaitForSeconds(.2f);
        walkedThroughDoor = false;
    }

     private void Update()
     {
        closestOpenDoorway = GameObject.FindGameObjectWithTag("ClosestOpenDoorway");

        if (closestOpenDoorway.GetComponent<DoorwaySide1>().arrived)
            transform.position = new Vector2(character.position.x, recenterPointFromDown.position.y);
    }
    void LateUpdate()
    {
        if (!(pmov.lookup || pmov.lookDown))
        {
            if (slopeCheck.onSlope || movPlatCheck.onMovingPlatform)
                transform.position = new Vector2(character.position.x, character.position.y);//If the character is on a slope or a moving platform, make sure the camera's follow target has the same x and y value as the character.
        }
        if (((slopeCheck.onSlope || movPlatCheck.onMovingPlatform) && (pmov.lookup || pmov.lookDown)) || !(slopeCheck.onSlope || movPlatCheck.onMovingPlatform))
            transform.position = new Vector2(character.position.x, transform.position.y);//Otherwise, make sure the camera's follow target always has the same x value as the character        

        if (pmov.lookup){//---------------------------------If the player holds down the look up button   
            timer -= Time.deltaTime;//----------------------Start the timer
            if (timer <= 0){//------------------------------When the timer reaches zero
                tempPos.y = transform.position.y;//---------Set the y-axis of the temporary position variable to the camera's y-axis position
                tempPos.x = character.position.x;//---------Make sure the camera's follow target continues to follow the character
                tempPos.y += Time.deltaTime * cameraSpeed;//Move the temporary variable upward according to the specified speed variable
                transform.position = tempPos;}}//-----------Set the position of the camera's follow target equal to the position of the temporary variable

        if (pmov.lookDown){//-------------------------------If the player holds down the look down button
            timer -= Time.deltaTime;//----------------------Start the timer
            if (timer <= 0){//------------------------------When the timer reaches zero
                tempPos.y = transform.position.y;//---------Set the y-axis of the temporary position variable to the camera's y-axis position
                tempPos.x = character.position.x;//---------Make sure the camera's follow target continues to follow the character
                tempPos.y -= Time.deltaTime * cameraSpeed;//Move the temporary variable downward according to the specified speed variable
                transform.position = tempPos;}}//-----------Set the position of the camera's follow target equal to the position of the temporary variable

        if (Input.GetButtonUp("LookUp")){//---------------------If the player releases the look up button
            timer = timerStart;//-------------------------------Reset the timer
            LookUpRecenter();
            StartCoroutine("LookUpRecenter");}//----------------Start the look up recenter coroutine

        if (neuralGun.shoot)//----------------------------------If the character shoots
            timer = timerStart;//-------------------------------Reset the timer

        if (Input.GetButtonUp("LookDown")){//-------------------If the player releases the look down button or goes through a side 1 doorway
            timer = timerStart;//-------------------------------Reset the timer
            LookDownRecenter();
            StartCoroutine("LookDownRecenter");}//--------------Start the look down recenter coroutine

        if (poleClimb.onPole || poleClimb.jumpedFromPole)//---------------If the character is on a pole or has jumped off of a pole
            rb.velocity = new Vector2(0, playerRb.velocity.y);//Make the camera's follow target follow the character by moving it's rigid body with the character

        if (!poleClimb.onPole)//-------------------------------------If the character is not on a pole
            rb.velocity = new Vector2(0, 0);//------------------Dont move the camera's follow target through it's rigidbody

        if (lookUpRecenter)//-----------------------------------If the lookUpRecenter variable is true
            transform.position = Vector2.Lerp(transform.position, recenterPointFromUp.position, Time.deltaTime * recenterSpeed);//Move the camera's follow target to the appropriate recenter point

        if (lookDownRecenter)//---------------------------------If the lookDownRecenter variable is true
            transform.position = Vector2.Lerp(transform.position, recenterPointFromDown.position, Time.deltaTime * recenterSpeed);//Move the camera's follow target to the appropriate recenter point


        if (!groundCheck.grounded)//-----------------------------If the character is not grounded
            transform.position = new Vector2(transform.position.x, character.position.y);//Set the camera's follow target to have the same y value as the character
            
        if (recenterTrigger.justLanded){//-----------------------When the character lands        
            JustLanded();
            StartCoroutine("JustLanded");}//---------------------Start the just landed coroutine     


        if (closestOpenDoorway.GetComponent<DoorwaySide2>().arrived && doorwayCheck.inDoorway)
            transform.position = new Vector2(character.position.x, character.position.y);

    }
}