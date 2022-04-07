using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 1;

    private SpriteRenderer characterSprite;

    private Vector3 offset;

    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        characterSprite = GetComponent<SpriteRenderer>();
        // extent y is the distance from the sprite center pivot, to the bottom of the sprite. z set to 3 to keep player infront
        // playerFeetOffset ensures the offset value moves the player relative to its feet position.-0.15f
        offset = new Vector3(0, -0.15f, 3);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MoveCharacter(int newZValue)
    {
        checkCharacterHeight();
        //I am putting these placeholder variables here, to make the logic behind the code easier to understand
        //we differentiate the movement speed between horizontal(x) and vertical(y) movement, since isometric uses "fake perspective"
        float horizontalMovement = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        //since we are using this with isometric visuals, the vertical movement needs to be slower
        //for some reason, 50% feels too slow, so we will be going with 75%
        float verticalMovement = Input.GetAxisRaw("Vertical") * speed * 0.5f * Time.deltaTime;

        //make character appear as ontop of or behind terrain
        int heightDiff = newZValue - ((int)this.transform.position.z);
        this.transform.Translate(horizontalMovement, verticalMovement, heightDiff);
        FlipSpriteToMovement();

    }

    void checkCharacterHeight()
    {

    }

    public void setPosition(Vector3 position)
    {
        //player pivot in center of sprite, so offset corrects position relative to players feet as pivot
        this.transform.position = position + offset;
        Debug.Log("position before offset: " + position + ". offset: " + offset + ". After offset: " + this.transform.position);
    }

    //if the player moves left, flip the sprite, if he moves right, flip it back, stay if no input is made
    void FlipSpriteToMovement()
    {
        if (characterSprite != null)
        {
            if (Input.GetAxisRaw("Horizontal") < 0)
                characterSprite.flipX = true;
            else if (Input.GetAxisRaw("Horizontal") > 0)
                characterSprite.flipX = false;
        }
    }
}
