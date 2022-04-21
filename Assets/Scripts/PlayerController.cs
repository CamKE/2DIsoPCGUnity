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
    private readonly Vector2 pivotOffset = new Vector2(0, -0.15f);

    // i use this instead of taking the grid center to world, as the grid center looks off. instead i take the grid cell positon
    // and offset it by 0.25
    private readonly Vector2 tileCenterOffset = new Vector2(0,0.25f);

    // make sure player is sorted properly
    private readonly Vector3 playerZOffset = new Vector3(0, 0, 0.5f);

    private const float tileZIncrement = 0.25f;

    private Vector2 playerWorldPosition;


    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // store a ref to the player's sprite renderer component
        playerSprite = GetComponent<SpriteRenderer>();

        //**note for me: removed the temporary z value of 3 from pivotOffset which was to keep the 
        // player sprite infront of the tiles, but dont need it and it does make sense here

    }

    public Vector2 getWorldPosition()
    {
        return playerWorldPosition;
    }

    public void movePlayer()
    {
        Vector2 movement = getMovement();

        //make character appear as ontop of or behind terrain
        this.transform.Translate(movement.x, movement.y, 0);

        flipSprite();
    }

    public void movePlayer(int zValue)
    {
        Vector2 movement = getMovement();

        playerWorldPosition += movement;

        updatePlayerPosition(zValue);

        flipSprite();
    }

    private Vector2 getMovement()
    {
        //I am putting these placeholder variables here, to make the logic behind the code easier to understand
        //we differentiate the movement speed between horizontal(x) and vertical(y) movement, since isometric uses "fake perspective"
        float horizontalMovement = Input.GetAxisRaw("Horizontal") * movementSpeed * Time.deltaTime;
        //since we are using this with isometric visuals, the vertical movement needs to be slower
        //for some reason, 50% feels too slow, so we will be going with 75%
        float verticalMovement = Input.GetAxisRaw("Vertical") * movementSpeed * 0.5f * Time.deltaTime;

        return new Vector2(horizontalMovement, verticalMovement);
    }

    public void updatePlayerPosition(int zValue)
    {   //zval time tile height difference at each z increment
        Vector3 newPos = new Vector3(playerWorldPosition.x, playerWorldPosition.y + (zValue * tileZIncrement), zValue) + playerZOffset;
        this.transform.position = newPos;
    }

    public void setWorldPosition(Vector2 newWorldPos)
    {
        playerWorldPosition = newWorldPos + tileCenterOffset + pivotOffset;
    }

    //if the player moves left, flip the sprite, if he moves right, flip it back, stay if no input is made
    private void flipSprite()
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
