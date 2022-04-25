using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEditor;
using System;

/// <summary>
/// Responsible for all operations to do with creating, managing and destroying the level and elements within it.
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

    public bool rangeHeightEnabled;

    public int getCellZPosition(Vector2 worldPos)
    {
        return level.getCellZPosition(worldPos);
    }

    /// <summary>
    /// Generate the level. temporary setup.
    /// </summary>
    public void generate(TerrainOptions.TerrainSettings terrainSettings, RiverOptions.RiverSettings riverSettings, LakeOptions.LakeSettings lakeSettings)
    {
        rangeHeightEnabled = terrainSettings.heightRangedEnabled;

        level.generate(terrainSettings, riverSettings, lakeSettings);
    }

    // whether or not the player character has been instantiated
    // had a bool var before, but this seems to make more sense, as we cannot
    // store the playercontroller before instatiation as the instance controller 
    // will be different from the one before instantiation, therefore by default
    // we know the player is instantiated if its playercontroller var is set
    private bool isPlayerInstatiated()
    {
        return playerController != null ? true : false;
    }

    /// <summary>
    /// Create the player and put them into the level at the given position.
    /// </summary>
    /// <param name="position">Where the player should be placed on the level</param>
    public void setupPlayer()
    {
        // if the player is not instantiated
        if (!isPlayerInstatiated())
        {
            // Instatiate the player
            player = Instantiate(player, new Vector3(2, 2, 3.01f), Quaternion.identity);

            // store the ref to the player component playerController
            playerController = player.GetComponent<PlayerController>();

            playerController.setLevelManager(this);

            Debug.Log(isPlayerInstatiated());
        }

        playerController.setDoMovement(rangeHeightEnabled);
        // make sure the player object is enabled
        setPlayerActive(true);

        Vector3Int randomCell = level.getRandomCell();

        playerController.setWorldPosition(level.getGridPosition(((Vector2Int)randomCell)));
        playerController.updatePlayerPosition(randomCell.z);
 
    }

    /// <summary>
    /// Used by the UI manager to update the level camera's active status when swapping between
    /// level generation and demo user interfaces
    /// </summary>
    /// <param name="value">The boolean value to set the level camera's active status to.</param>
    public void setLevelCameraActive(bool value)
    {
        // set the level camera's active status to the given value
        level.setCameraActive(value);
    }

    /// <summary>
    /// Used by the UI manager to update the player's active status when swapping between
    /// level generation and demo user interfaces
    /// </summary>
    /// <param name="value">The boolean value to set the player's active status to.</param>
    public void setPlayerActive(bool value)
    {
        // set the player's active status to the given value
        player.SetActive(value);
        
        if (!value)
        {
            playerController.clearDoMovement();
        }
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
