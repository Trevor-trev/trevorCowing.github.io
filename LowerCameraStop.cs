using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowerCameraStop : MonoBehaviour
{/*This script is meant to interact with a game object titled "CameraPosition", which has a trigger collider attached to it
   and is set as a child of the object with the "CameraMovement" script attached to it
   This script is to be placed on an object with a trigger collider, positioned below the character.
   It's purpose is to prevent the camera from moving downward (near)infinitely.

   Set it as a child to another object, along with another object+trigger-collider with the "UpperCameraStop" script.
   Use the "PlaceOnCharacter" script for that parent object to make sure these camera stoppers follow the character on the x/y axis.
   if you set the camera stops as children of the character, they will follow the character without the "PlaceOnCharacter" script,**BUT**
   the colliders will be designated as belonging to the character, so anytime those colliders are touching a pole, the character
   will be able to climb that pole, even if the character sprite is not touching the pole sprite.*/

    public CameraMovement cam;

    private void OnTriggerEnter2D(Collider2D other)//-Execute this code when the specified object's collider enters the collider attached to the same object as this script
    {
        if (other.CompareTag("CameraPosition"))//-----If the object is tagged as "CameraPosition"
        {
            if (Input.GetButton("LookDown"))//-------If the player presses the LookDown button
                cam.cameraSpeed = 0;//---------------Set the camera's speed to 0 so it cannot move any farther up. 
            if (Input.GetButton("LookUp"))//---------If the player presses the LookUp button
                cam.cameraSpeed = 15;//--------------Set the camera's speed to 15 so it may once again move
        }
    }
    private void OnTriggerStay2D(Collider2D other)//-Execute this code when the specified object's collider stays in the collider attached to the same game object as this script
    {
        if (other.CompareTag("CameraPosition"))//----If the oject is tagged as "CameraPosition"
        {
            if (Input.GetButton("LookDown"))//-------If the player presses the LookDown button
                cam.cameraSpeed = 0;//---------------Set the camera's speed to 0 so it cannot move any farther up.
            if (Input.GetButton("LookUp"))//---------If the player presses the LookUp button
                cam.cameraSpeed = 15;//--------------Set the camera's speed to 15 so it may once again move
        }
    }

    private void OnTriggerExit2D(Collider2D other)//Execute this code when the specified object's collider leaves the collider attached to the smae object as this script
    {
        if (other.CompareTag("CameraPosition"))//---If the object is tagged as "Camera Position"
            cam.cameraSpeed = 15;//-----------------Set the camera's speed to 15 so it may move
    }
}
