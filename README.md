# 2D Isometric Procedural Content Generation System in Unity

This is a procedural generator for producing 2D isometric perspective levels to be used in game projects.

| Click here for WebGL Online Version (Password: 'survery') |
|:----------------------------------:|
| [![PreviewScreenshot](https://user-images.githubusercontent.com/37081579/168493637-656c601f-67c3-4898-a99f-e7e2f9d12891.png)](https://camke.itch.io/2d-isometric-procedural-generation) |

## Level Generation Screen

### Controls

	- Minus button: Used to incrementally zoom out of the level. Can also use mouse scrollwheel or trackpad.
    - Plus button: Used to incrementally zoom into the level. Can also use mouse scrollwheel or trackpad.
    - Recenter: Reposition the camera to have the level in full view.
    - Level Info: Displays the level generation steps.
    - Randomise Level: Generate a level using randomised settings.
    - Generate Level: Generate a level using user settings.
    - Demo Level: Control a player on the level (currently disabled for varying height levels due to collision bound issues).

### Options

*Note: You can also hover over the option heading to see what the it does.*

#### Terrain

- Terrain Size: Specify the size of the terrain in terms of maximum tile count. This value will be rounded to a square value for generating the map. Randomly shaped levels tile count will vary.
- Terrain Type: Choose the type of terrain to be generated. This will change the tiles used in the level.
- Terrain Height: Choose whether to specify the exact height of the terrain or a range of height the terrain should be between.
- Terrain Height: Choose whether to specify the exact height of the terrain or a range of height the terrain should be between.
    - Exact Height: Specify the exact height the terrain should be.
	- Height Range: Specify the minimum and maximum height the terrain should be between.
- Terrain Shape: Choose the shape of the terrain. Terrain size will vary for randomly shaped levels. Higher terrain sizes are more likely to produce larger levels.

#### Water Bodies

    - River Generation: Turn on or off the creation of rivers.
        - Number of Rivers: Select the number of rivers relative to the remaining terrain size.
        - River Intersection: Turn on or off whether rivers are able to cross paths. Also affects whether lakes can combine  with rivers.
    - Lake Generation: Turn on or off the creation of lakes.
        - Number of Lakes: Select the number of lakes relative to the terrain size.
        - Maximum Lake Size: Select the maximumum lake size relative to the terrain size. Lakes up to the selected size will  be generated.
		
#### Walkpath

    - Walkpath Generation: Turn on or off the creation of walkpaths.
        - Number of Walkpaths: Select the number of walkpaths relative to the remaining terrain size.
        - Walkpath Intersection: Turn on or off whether walkpaths are able to cross paths.

## Demo Screen

### Controls

	- Exit level button: Return to the level generation screen
	
Use the arrow or WASD keys to move the character.