using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class RecenterTriggerGround : MonoBehaviour
{
    /*This script is to be placed on an object along with a box collider that extends just below the character's feet.
      It uses a coroutine to execute code only for a brief moment after the character hits the ground.
      It's purpose is to recenter the camera when the character lands on anything designated as solid*/
   
    public LedgeClimb ledgeClimb;
    public PogoController pogo;

    public bool touchingDroppablePlatform;
    public bool justLanded;//---------------------------A bool that returns true when the box collider hits the ground

    BoxCollider2D groundTrigger;//----------------------A box collider to be placed just below the character's feet that detects when they hit anything designated as ground


    private void Start()
    {
        groundTrigger = GetComponent<BoxCollider2D>();//The box collider attached to the same object as this script
    }

    IEnumerator Recenter()//----------------------------A coroutine labeled "Recenter". Has the ability to pause and resume execution according to specifications
    {
        justLanded = true;//----------------------------The character has just landed
        yield return new WaitForSeconds(.75f);//--------Wait for the specified time (in seconds)
        justLanded = false;//---------------------------The character has no longer just landed
    }

    private void OnTriggerEnter2D(Collider2D other)//---Execute this code when the specified object's collider enters the collider attached to the same object as this script
    {
        if (other.CompareTag("Solid") || other.CompareTag("MakeshiftGround")){//---------------If the object is tagged as "Solid"        
            if (!pogo.onPogo){//---------------------------If the character is not on the pogo stick
                Recenter();
                StartCoroutine("Recenter");}}//-----------Start the Recenter coroutine     

        if (other.CompareTag("MakeshiftGround") || other.CompareTag("MovingPlatform") || other.CompareTag("ClosestMovingPlatform"))
            touchingDroppablePlatform = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("MakeshiftGround") || other.CompareTag("MovingPlatform") || other.CompareTag("ClosestMovingPlatform"))
            touchingDroppablePlatform = false;
    }
    private void Update()
    {
        if (!ledgeClimb.ledgeHang)//--------------------If the character is not hanging from a ledge
            groundTrigger.enabled = true;//-------------The enable the ground trigger collider

        if (ledgeClimb.ledgeHang)//---------------------If the character is hanging from a ledge
            groundTrigger.enabled = false;//------------Disable the ground trigger collider
    }
}
