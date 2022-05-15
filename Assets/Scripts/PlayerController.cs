using System;
using UnityEngine;

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
    private readonly Vector2 pivotOffset = new Vector3(0, -0.15f);

    // use this instead of taking the grid center to world, as the grid center
    // method result looks off. instead i take the grid cell positon
    private readonly Vector2 tileCenterOffset = new Vector3(0,0.25f);

    // make sure player is above any tile is it on
    private readonly float playerZOffset = 0.5f;

    // represents the world measurement of a single (1) z increment on  the grid
    private const float tileZIncrement = 0.25f;

    // needs ref to level manager for movement calculations
    private LevelManager levelManager;

    // holds the movement action to be done depending on the terrain height
    private static Action doMovement;

    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // store a ref to the player's sprite renderer component
        playerSprite = GetComponent<SpriteRenderer>();
    }

    // update is called every frame when the script is enabled
    private void Update()
    {
        // check for player movement
        doMovement();
    }

    /// <summary>
    /// Sets the doMovement action in the controller based on the state
    /// of the rangeHeightEnabled flag.
    /// </summary>
    /// <param name="rangeHeightEnabled">Whether or not height variation is on.</param>
    public void setDoMovement(bool rangeHeightEnabled)
    {
        // if range height is on
        if (rangeHeightEnabled)
        {
            // use this method to calculate movemnt
            doMovement += movePlayerWithHeightVariation;
        } 
        else
        // otherwise
        {
            // use the default method
            doMovement += movePlayer;
        }
    }

    /// <summary>
    /// Set a referene to the level manager in the controller.
    /// </summary>
    /// <param name="levelManager">The level manager to be referenced.</param>
    public void setLevelManager(LevelManager levelManager)
    {
        this.levelManager = levelManager;
    }

    /// <summary>
    /// Moves the player according to user input. Used for terrain without height
    /// variation.
    /// </summary>
    private void movePlayer()
    {
        // get the movement from the user input
        Vector2 movement = getMovement();
        // move the character by the amount specified in the movement vector
        this.transform.Translate(movement);
        // flip the sprite if necessary
        flipSprite();
    }

    /// <summary>
    /// Moves the player according to user input. Used for terrain with height
    /// variation.
    /// </summary>
    private void movePlayerWithHeightVariation()
    {
        // get the movement from the user input
        Vector2 movement = getMovement();

        // get the players previous depth (without the offset)
        float previousZValue = transform.position.z - playerZOffset;

        // move the player according to the user input
        this.transform.Translate(movement);

        // get the depth at the players current position on the map
        float currentCellZValue = levelManager.getMapZPosition(getWorldPositionOnGrid());

        // if the z value is different
        if (currentCellZValue != previousZValue)
        {
            // update the players depth and position in the world
            // z in world space on the tilemap affects the sorting order of
            // the sprite

            // work out the difference in depth
            float zDifference = currentCellZValue - previousZValue;

            // offset the players y position by the depth converted to world space
            Vector3 zChange = new Vector3(0, zDifference * tileZIncrement, zDifference);

            // apply the update
            this.transform.Translate(0,zChange.y, zChange.z);
        }
        // flip the sprite if necessary
        flipSprite();
    }

    // returns the players world position relative to the grid
    // (without the offset for tile depth, or the z offset to ensure
    // proper sprite sorting)
    private Vector2 getWorldPositionOnGrid()
    {
        Vector2 playerWorldPositionOnGrid = transform.position;

        float zValue = transform.position.z - playerZOffset;

        playerWorldPositionOnGrid.y -= zValue * tileZIncrement;

        return playerWorldPositionOnGrid;
    }

    // returns a movement vector from the user input. adapted from GoldenSkullStudios simple_char_move
    private Vector2 getMovement()
    {
        // calulate the horizontal and vertical movement relative to the framerate and the movement speed
        float horizontalMovement = Input.GetAxisRaw("Horizontal") * movementSpeed * Time.deltaTime;
        // vertical movement 50% slower for isometric persepective
        float verticalMovement = Input.GetAxisRaw("Vertical") * movementSpeed * 0.5f * Time.deltaTime;

        return new Vector2(horizontalMovement, verticalMovement);
    }

    /// <summary>
    /// Place the place at the given position and at the given depth.
    /// </summary>
    /// <param name="newWorldPosition">The new position to place the player.</param>
    /// <param name="zValueOnMap">The new depth of the player on the map.</param>
    public void setPlayerPosition(Vector2 newWorldPosition, int zValueOnMap)
    {
        // apply the offsets
        Vector2 playerWorldPosition = newWorldPosition + tileCenterOffset + pivotOffset;
        
        // calculate the new position
        Vector3 newPosition = new Vector3(playerWorldPosition.x, playerWorldPosition.y + (zValueOnMap * tileZIncrement), zValueOnMap + playerZOffset);
        // set the players posititon to the new position
        this.transform.position = newPosition;
    }

    // called when the script is disabled (when the player's active status is changed to false in this case)
    private void OnDisable()
    {
        // clear the movement action
        doMovement = null;
    }

    // code below from GoldenSkullStudio
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
