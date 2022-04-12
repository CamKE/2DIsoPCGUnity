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
    private readonly Vector3 pivotOffset = new Vector3(0, -0.15f, 0);


    // i use this instead of taking the grid center to world, as the grid center looks off. instead i take the grid cell positon
    // and offset it by 0.25
    private readonly Vector3 tileCenterOffset = new Vector3(0,0.25f,0);

    // make sure player is sorted properly
    private readonly Vector3 playerZOffset = new Vector3(0, 0, 0.5f);

    private Vector3 movement;

    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        movement = Vector3.zero;
        // store a ref to the player's sprite renderer component
        playerSprite = GetComponent<SpriteRenderer>();

        //**note for me: removed the temporary z value of 3 from pivotOffset which was to keep the 
        // player sprite infront of the tiles, but dont need it and it does make sense here

    }

    public Vector3 getMovement()
    {
        return movement;
    }

    public Vector3 getPosition()
    {
        return this.transform.position;// -pivotOffset - tileCenterOffset - playerZOffset;
    }

    public void MoveCharacter(float newZValue)
    {
        checkCharacterHeight();
        //I am putting these placeholder variables here, to make the logic behind the code easier to understand
        //we differentiate the movement speed between horizontal(x) and vertical(y) movement, since isometric uses "fake perspective"
        float horizontalMovement = Input.GetAxisRaw("Horizontal") * movementSpeed * Time.deltaTime;
        //since we are using this with isometric visuals, the vertical movement needs to be slower
        //for some reason, 50% feels too slow, so we will be going with 75%
        float verticalMovement = Input.GetAxisRaw("Vertical") * movementSpeed * 0.5f * Time.deltaTime;

        
        movement = new Vector2(horizontalMovement, verticalMovement);
        setPosition(movement, newZValue);
        //make character appear as ontop of or behind terrain
        //float heightDiff = newZValue - ((int)this.transform.position.z);
        //this.transform.Translate(horizontalMovement, verticalMovement, heightDiff);
        FlipSpriteToMovement();

    }

    public void MoveCharacter()
    {
        checkCharacterHeight();
        //I am putting these placeholder variables here, to make the logic behind the code easier to understand
        //we differentiate the movement speed between horizontal(x) and vertical(y) movement, since isometric uses "fake perspective"
        float horizontalMovement = Input.GetAxisRaw("Horizontal") * movementSpeed * Time.deltaTime;
        //since we are using this with isometric visuals, the vertical movement needs to be slower
        //for some reason, 50% feels too slow, so we will be going with 75%
        float verticalMovement = Input.GetAxisRaw("Vertical") * movementSpeed * 0.5f * Time.deltaTime;

        //make character appear as ontop of or behind terrain
        this.transform.Translate(horizontalMovement, verticalMovement, 0);
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
        this.transform.position = newPosition + pivotOffset + tileCenterOffset + playerZOffset;
    }

    public void setPosition(Vector2 movement, float newZValue)
    {
        // set the player position to be at the new position, offset by the pivotOffset
        Vector3 newPosition = new Vector3(movement.x + this.transform.position.x, movement.y + this.transform.position.y, newZValue);
        this.transform.position = newPosition;


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
