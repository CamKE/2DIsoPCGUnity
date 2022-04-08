using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// code adapted from GoldenSkullStudio's simple_char_move
// could change character size to sell the illusion of level depth
// and distance from camera
/// <summary>
/// This class is responsible for controlling the player character on the level.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    // the speed of movemenet for the player
    [SerializeField]
    private float movementSpeed = 2;

    // the component responsible for handling the player sprite
    private SpriteRenderer playerSprite;

    // the offset for the player sprite pivot to position the sprite relative
    // to the players feet instead of the sprite center
    private Vector3 pivotOffset;

    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // store a ref to the player's sprite renderer component
        playerSprite = GetComponent<SpriteRenderer>();

        //**note for me: removed the temporary z value of 3 from pivotOffset which was to keep the 
        // player sprite infront of the tiles, but dont need it and it does make sense here

        // set the pivot offset to be 0.15 down on the y axis
        pivotOffset = new Vector3(0, -0.15f, 0);
    }

    public void MoveCharacter(int newZValue)
    {
        checkCharacterHeight();
        //I am putting these placeholder variables here, to make the logic behind the code easier to understand
        //we differentiate the movement speed between horizontal(x) and vertical(y) movement, since isometric uses "fake perspective"
        float horizontalMovement = Input.GetAxisRaw("Horizontal") * movementSpeed * Time.deltaTime;
        //since we are using this with isometric visuals, the vertical movement needs to be slower
        //for some reason, 50% feels too slow, so we will be going with 75%
        float verticalMovement = Input.GetAxisRaw("Vertical") * movementSpeed * 0.5f * Time.deltaTime;

        //make character appear as ontop of or behind terrain
        int heightDiff = newZValue - ((int)this.transform.position.z);
        this.transform.Translate(horizontalMovement, verticalMovement, heightDiff);
        FlipSpriteToMovement();

    }

    void checkCharacterHeight()
    {

    }

    /// <summary>
    /// Sets the position of the player on the level.
    /// </summary>
    /// <param name="newPosition">The new position of the player.</param>
    public void setPosition(Vector3 newPosition)
    {
        // set the player position to be at the new position, offset by the pivotOffset
        this.transform.position = newPosition + pivotOffset;
    }

    //if the player moves left, flip the sprite, if he moves right, flip it back, stay if no input is made
    private void FlipSpriteToMovement()
    {
        // if there is a player sprite
        if (playerSprite != null)
        {
            // if the horizontal input axis is to the left
            if (Input.GetAxisRaw("Horizontal") < 0)
                // flip the sprite
                playerSprite.flipX = true;
            // if the horizontal input axis is to the right
            else if (Input.GetAxisRaw("Horizontal") > 0)
                // flip the sprite back to original position
                playerSprite.flipX = false;
        }
    }
}
