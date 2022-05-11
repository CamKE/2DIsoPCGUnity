using System;
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

    private Vector2 playerWorldPosition;

    private const float tileZIncrement = 0.25f;

    // needs ref to level manager for movement calculations
    private LevelManager levelManager;

    private static Action doMovement;

    private Rigidbody2D body;

    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // store a ref to the player's sprite renderer component
        playerSprite = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();

        //**note for me: removed the temporary z value of 3 from pivotOffset which was to keep the 
        // player sprite infront of the tiles, but dont need it and it does make sense here

    }

    // update is called every frame when the script is enabled
    private void Update()
    {
        doMovement();
    }

    public void setDoMovement(bool rangeHeightEnabled)
    {
        if (rangeHeightEnabled)
        {
            doMovement += movePlayerWithHeightVariation;
        } 
        else
        {
            doMovement += movePlayer;
        }
    }

    public void clearDoMovement()
    {
        doMovement = null;
    }

    public void setLevelManager(LevelManager levelManager)
    {
        this.levelManager = levelManager;
    }

    private void movePlayer()
    {
        Vector2 movement = getMovement();

        //make character appear as ontop of or behind terrain
        this.transform.Translate(movement.x, movement.y, 0);

        flipSprite();
    }

    private void movePlayerWithHeightVariation()
    {
        Vector2 movement = getMovement();

        float currentCellZValue = levelManager.getCellZPosition(getWorldPositionOnGrid());

        float previousZValue = transform.position.z - playerZOffset.z;

        if (currentCellZValue == previousZValue)
        {
            this.transform.Translate(movement.x, movement.y, 0);
        } else
        {
            float zDifference = currentCellZValue - previousZValue;
            Vector3 zChange = new Vector3(0, zDifference * tileZIncrement, zDifference);

            this.transform.Translate(movement.x, movement.y + zChange.y, zChange.z);
        }

        flipSprite();
    }

    private Vector2 getWorldPositionOnGrid()
    {
        Vector2 playerWorldPositionOnGrid = transform.position;

        float zValue = transform.position.z - playerZOffset.z;

        playerWorldPositionOnGrid.y -= zValue * tileZIncrement;

        return playerWorldPositionOnGrid;
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

    // update the players position at the height given based on the world pos
    public void updatePlayerPosition(int zValue)
    {   //zval time tile height difference at each z increment
        Vector3 newPos = new Vector3(playerWorldPosition.x, playerWorldPosition.y + (zValue * tileZIncrement), zValue) + playerZOffset;
        this.transform.position = newPos;
    }

    public void setWorldPosition(Vector2 newWorldpos)
    {
        playerWorldPosition = newWorldpos + tileCenterOffset + pivotOffset;
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
