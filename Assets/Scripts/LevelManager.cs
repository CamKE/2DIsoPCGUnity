using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for all operations to do with the level and the player. Used by the UIManager (generation in project build) and the LevelEditor (generation in unity editor).
/// </summary>
public class LevelManager : MonoBehaviour
{
    // '[SerializeField] private' show up in the inspector but are not accessible by other scripts

    // player character to be used in demo mode
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private Level level;

    // the playerController component of the player
    private PlayerController playerController;

    // whether or not specifying a range of height is on
    private bool rangeHeightEnabled;
    
    /// <summary>
    /// Calls the setup function from the level as the inspector (by setting isFromInspector bool to true).
    /// </summary>
    public void setupLevelFromInspector()
    {
        level.setup(true);
    }

    /// <summary>
    /// Get the z position from the map at the given world position from level.
    /// </summary>
    /// <param name="worldPos">The world position to get the map z value for.</param>
    /// <returns>The z position at the world position on the map.</returns>
    public int getMapZPosition(Vector2 worldPos)
    {
        return level.getMapZPosition(worldPos);
    }

    /// <summary>
    /// Generate the level with the given settings. Passes the settings onto level.
    /// </summary>
    /// <param name="terrainSettings">The settings for the terrain generator.</param>
    /// <param name="riverSettings">The settings for the river generator.</param>
    /// <param name="lakeSettings">The settings for the lake generator.</param>
    /// <param name="walkpathSettings">The settings for the walkpath generator</param>
    public void generate(TerrainSettings terrainSettings, RiverSettings riverSettings, LakeSettings lakeSettings, WalkpathSettings walkpathSettings)
    {
        // set the range height enabled to the terrain setting value
        rangeHeightEnabled = terrainSettings.heightRangeEnabled;

        level.generate(terrainSettings, riverSettings, lakeSettings, walkpathSettings);
    }

    // whether or not the player character has been instantiated
    // we know the player is instantiated if its playercontroller var is set
    private bool isPlayerInstatiated()
    {
        return playerController != null ? true : false;
    }

    /// <summary>
    /// Instantiate the player (if they are not already) and put them into the level at a random position.
    /// </summary>
    public void setupPlayer()
    {
        // if the player is not instantiated
        if (!isPlayerInstatiated())
        {
            // Instatiate the player
            player = Instantiate(player, new Vector3(2, 2, 3.01f), Quaternion.identity);

            // store the ref to the player component playerController
            playerController = player.GetComponent<PlayerController>();

            // give the playercontroller a reference to the level manager
            playerController.setLevelManager(this);
        }

        // set the movement action based on the height type of the terrain
        playerController.setDoMovement(rangeHeightEnabled);
        // make sure the player object is enabled
        setPlayerActive(true);
        // get a random terrain cell to place the player on
        Vector3Int randomCell = level.getRandomTerrainCellPosition();

        // get the players x,y world position on the level grid at the random cell x,y position
        Vector2 playerWorldPositionOnGrid = level.getGridWorldPosition(((Vector2Int)randomCell));
        // place the player at the world position, and give it the z value of the random cell
        playerController.setPlayerPosition(playerWorldPositionOnGrid, randomCell.z);
    }

    /// <summary>
    /// Used by the UI manager to update the level camera's active status when swapping between
    /// level generation and demo user interfaces.
    /// </summary>
    /// <param name="value">The boolean value to set the level camera's active status to.</param>
    public void setLevelCameraActive(bool value)
    {
        // set the level camera's active status to the given value
        level.setCameraActive(value);
    }

    /// <summary>
    /// Used by the UI manager to update the player's active status when swapping between
    /// level generation and demo user interfaces.
    /// </summary>
    /// <param name="value">The boolean value to set the player's active status to.</param>
    public void setPlayerActive(bool value)
    {
        // set the player's active status to the given value
        player.SetActive(value);
    }

    /// <summary>
    /// Get the level generation information from the level.
    /// </summary>
    /// <returns>The level generation steps.</returns>
    public List<string> getGenerationInfo()
    {
        return level.getGenerationInfo();
    }

    /// <summary>
    /// Clear the level.
    /// </summary>
    public void clearLevel()
    {
        level.clear();
    }

    // runs before the application is quit 
    private void OnApplicationQuit()
    {

        // if the player exists
        if (isPlayerInstatiated())
        {
            // delete the player
            Destroy(player);
        }
    }
}
