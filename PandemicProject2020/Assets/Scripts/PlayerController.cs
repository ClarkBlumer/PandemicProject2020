using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditorInternal;
using UnityEngine;
//using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    //Changable variables in the unity editor. 
    //Speed is character movement speed
    //JumpForce is the amount of force added to a jump.  
    //extraJumps will be used to keep track of multiple jumps for double jump and is able to be adjusted with Unity editor.
    //jumpDelay is used to adjust how long to give the player to input a future jump - jumps do not have to be frame perfect with this timer
    
    [Header("Movements")]
    public float Speed;
    public float JumpForce;
    public int extraJumps;
    public float jumpDelay = 0.25f;
    public float wallJumpForce = 10;
    

    public int extraJumpCounter;
    private float jumpTimer;
    Vector2 movement;

    //maxSpeed limits how fast the player can travel since we are adding force for movement.
    //linearDrag applies to the character to slow down when a movement key is not being pressed
    //gravity is the bending of spacetime 
    //fallGravity is a multiplier added to gravity to increase how quickly the player returns to the ground
    [Header("Physics")]
    public float maxSpeed = 7;
    public float linearDrag = 4;
    public float gravity = 1;
    public float fallGravity = 5;

    //Private variables to setup basic parts of the character object so that I can use the different attributes associated to the player. 
    //rb to access the attributes associated to the rigidbody
    //Sprite to access the attributes associated to the sprite renderer
    [Header("Components")]
    public LayerMask groundLayer;
    public GameObject characterHolder;
    public SpriteRenderer Sprite;
    public ParticleSystem jumpDust;
    private Rigidbody2D rb;

    //isGrounded is a check to see if the player is standing on any object with tag Ground. 
    //groundCheckLength is used for the raycast to detect when grounded
    //racastOffset is used to adjust where the raycast is cast to check for the bounds of the colliderbox
    [Header("Collision")]
    public bool isGrounded = false;
    public float groundCheckLength;
    public bool isTouchingWall = false;
    public float touchingWallLength;
    public Vector3 raycastOffset;
    
    


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        extraJumpCounter = extraJumps;
    }

    private void Update()
    {
        bool wasGrounded = isGrounded; 
        isGrounded = Physics2D.Raycast(transform.position + raycastOffset, Vector2.down, groundCheckLength, groundLayer) || Physics2D.Raycast(transform.position - raycastOffset, Vector2.down, groundCheckLength, groundLayer);
        isTouchingWall = Physics2D.Raycast(transform.position + raycastOffset, Vector2.right, touchingWallLength, groundLayer) || Physics2D.Raycast(transform.position - raycastOffset, Vector2.left, touchingWallLength, groundLayer);

        if (!wasGrounded && isGrounded)
        {
            StartCoroutine(JumpSqueeze(0.5f, 1.2f, 0.1f));

            //if player was not on the ground and is now grounded resets the extra jump counter
            extraJumpCounter = extraJumps;
        }

        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        //checked every update to ensure jump is responsive. Let's player attempt jump in future frames to limit the need to hit frame perfect jump responses. 
        if (Input.GetButtonDown("Jump"))
        {
            jumpTimer = Time.time + jumpDelay;
        }
        

        //Checks horizontal input every update to ensure sprite is facing correct way. 
        if (Input.GetAxis("Horizontal") > 0 )
        {
            Sprite.flipX = false;
            
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            Sprite.flipX = true;
            
        }


    }

    // FixedUpdate called at set intervals for physic calculations.  
    void FixedUpdate()
    {
        MoveCharacter(movement.x);
                
        if(jumpTimer > Time.time && isGrounded)
        {
            Jump();
        }
        //double jump check that the player is not grounded and that they have extra jumps to make. 
        if (jumpTimer > Time.time && !isGrounded && extraJumpCounter > 0 && !isTouchingWall)
        {
            Jump();
            extraJumpCounter--;
        }

        //wall jump
        if(jumpTimer > Time.time && isTouchingWall && !isGrounded && Input.GetAxis("Horizontal") != 0)
        {
            WallJump();
        }

        ModifyPhysics();
                
    }

    void Jump()
    {
        //plays JumpDust particle
        CreateJumpDust();


        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
        jumpTimer = 0;

        //Starts the JumpSqueeze method which modifies the sprite during jump
        StartCoroutine(JumpSqueeze(1.2f, 0.5f, 0.1f));
    }

    void WallJump()
    {
        CreateJumpDust();

        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(-Input.GetAxis("Horizontal") * wallJumpForce, JumpForce), ForceMode2D.Impulse);
        jumpTimer = 0;


    }

    void MoveCharacter(float horizontal)
    {
        rb.AddForce(Vector2.right * horizontal * Speed);
        if(Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
        }
    }

    void ModifyPhysics()
    {
        //detects when the player character changes direction
        bool directionChanged = (movement.x > 0 && rb.velocity.x < 0) || (movement.x < 0 && rb.velocity.x > 0);

        //applies drag to the player character when on the ground and moving less than .4 or when direction changes to slow the character.  if the character is currently moving the 
        //linear drag is 0.  
        if (isGrounded) {
            if (Mathf.Abs(movement.x) < 0.4f || directionChanged)
            {
                rb.drag = linearDrag;
            }
            else
            {
                rb.drag = 0f;
            }
            rb.gravityScale = 0;
        }

        //If the player is not grounded the gravity is turned on and 15% value of drag is applied.
        else
        {
            rb.gravityScale = gravity;
            rb.drag = linearDrag * 0.15f;

            //if the up velocity is less than 0 (no upward movement) the gravity is multiplied by fallGravity to increase the amount of gravity
            if (rb.velocity.y < 0)
            {
                rb.gravityScale = gravity * fallGravity;
            }

            //This lets the player make quick jumps if the Jump button is not held.  If there is upward movement and the jump button is pressed 
            //the player will jump higher according to the jumpForce value.  If there is upward movement and the jump button is not held the gravity is increased and pulls the player down 
            //using gravity and half of the fallGravity value.  
            else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                rb.gravityScale = gravity * (fallGravity / 2);
            }
        }
    }
    

    
    
    
    /*****************************************************************************************************************/
    /*                                                 Visual methods                                                */
    /*                                                                                                               */
    /*****************************************************************************************************************/

    
    
    //Squeezes the sprite and transforms using Lerp over seconds time.  Used as a quick animation for jumping/landing. 
    IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds)
    {
        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        float time = 0f; 

        while(time <= 1.0)
        {
            time += Time.deltaTime / seconds;
            characterHolder.transform.localScale = Vector3.Lerp(originalSize, newSize, time);
            yield return null;
        }

        time = 0f;
        while(time <= 1.0)
        {
            time += Time.deltaTime / seconds;
            characterHolder.transform.localScale = Vector3.Lerp(newSize, originalSize, time);
            yield return null;
        }

    }


    //Plays the jumpDust particle effect when called
    void CreateJumpDust()
    {
        jumpDust.Play();
    }


    //Visual lines for the raycast ground check in the gizmos options
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + raycastOffset, transform.position + raycastOffset + Vector3.down * groundCheckLength);
        Gizmos.DrawLine(transform.position - raycastOffset, transform.position - raycastOffset + Vector3.down * groundCheckLength);

        Gizmos.DrawLine(transform.position + raycastOffset, transform.position + raycastOffset + Vector3.right * touchingWallLength);
        Gizmos.DrawLine(transform.position - raycastOffset, transform.position - raycastOffset + Vector3.left * touchingWallLength);
    }
}
