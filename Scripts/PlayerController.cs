using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    public float speed = 50f;
    public float maxSpeed = 3;
    [Range(1, 10)]
    public float jumpVelocity;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float climbSpeed;
    private float climbVelocity;
    private float gravityStore;
    public float vertical_movement;
    public float horizontal_movement;
    // State
    public int currHealth;  // current health
    public int maxHealth = 100;

    public bool rightPushingZone;
    public bool leftPushingZone;      // 1 This four are related with pushing state
    public bool pushed;           // 2
    public bool grounded;
    public bool climbed;
    public bool onLadder;
    public bool came_from_air;
    public bool dead;
    private bool gasLambOn;
    private bool activateLamb;

    private Rigidbody2D rb2d;
    private Animator animator;
    public Collider2D ladderCollider;

	public Vector3 ladderPos;

    // Use this for initialization
    public void Start() {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();

        currHealth = maxHealth; // at the beginning, set the healt of te player to the max health

        gravityStore = rb2d.gravityScale;

		ladderPos = Vector3.zero;

        gasLambOn = false;
        activateLamb = true;    // TODO: this line will be changed
        dead = false;
    }

    // Update is called once per frame
    void Update() {
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Climbed", climbed);
        animator.SetBool("GasLambOn", gasLambOn);
        animator.SetBool("Pushed", pushed);
        animator.SetFloat("SpeedX", Mathf.Abs(rb2d.velocity.x));
        animator.SetFloat("SpeedY", vertical_movement);

        if (!dead)
            PlayerMovement();

        // Die state
        if (currHealth < 1)
            animator.SetBool("Died", true);

        if (currHealth > maxHealth)
            currHealth = maxHealth; // making sure that current health never gets bigger than max health
    }

    void FixedUpdate() {
        if (!dead) {
            Vector3 easeVelocity = rb2d.velocity;
            easeVelocity.x *= 0.70f;
            easeVelocity.y = rb2d.velocity.y;
            easeVelocity.z = 0.0f;

            bool attackState = animator.GetBool("Attacked");

            // Fake friction / easing the x speed of the player
            if (grounded) {
                rb2d.velocity = easeVelocity;
            }
            // Moving the player
            horizontal_movement = Input.GetAxis("Horizontal");    // Getting horizontal movement (-1,1)
			if (climbed)
				horizontal_movement = 0;
            if (!attackState)
                rb2d.AddForce((Vector2.right * speed) * horizontal_movement);
            else
                rb2d.AddRelativeForce(-rb2d.velocity);

            // Limiting the speed of the player
            if (rb2d.velocity.x > maxSpeed) {
                rb2d.velocity = new Vector2(maxSpeed, rb2d.velocity.y);
            }
            if (rb2d.velocity.x < -maxSpeed) {
                rb2d.velocity = new Vector2(-maxSpeed, rb2d.velocity.y);
            }
        }
    }

    void PlayerMovement() {
        bool jump_movement = false;
        if (!pushed)
            jump_movement = Input.GetButtonDown("Jump");
        vertical_movement = Input.GetAxisRaw("Vertical");
        animator.enabled = true;
        ladderCollider.enabled = true;

        // Lighting the gas lamb
        if (Input.GetButtonDown("Fire2")) {
            if (activateLamb && !gasLambOn)
                gasLambOn = true;
            else if (activateLamb && gasLambOn)
                gasLambOn = false;
        }

        // Flipping the sprite vertically with respect to the player's direction
        if (Input.GetAxis("Horizontal") < -0.1f) {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        if (Input.GetAxis("Horizontal") > 0.1f) {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // Jump state
        if (jump_movement && (grounded || onLadder)) {
            if (onLadder) { // If the player jump off from the ladder, disable the ladder collider
				ladderCollider.enabled = false;
            }
            rb2d.velocity = Vector2.up * jumpVelocity;
            came_from_air = true;
        }
        if (!climbed) {
            if (rb2d.velocity.y < 0) {  // Falling
                rb2d.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            else if (rb2d.velocity.y > 0 && !Input.GetButton("Jump")) {
                rb2d.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }

        // Climb state
        if (onLadder) {
			rb2d.gravityScale = 0f; // setting gravity to 0
            if (vertical_movement > 0f) {
				transform.position = new Vector3(ladderPos.x + 0.09f, transform.position.y, transform.position.z);
                transform.Translate(0, (vertical_movement > 0f ? 1 : -1) * Time.deltaTime, 0);
                climbVelocity = climbSpeed * vertical_movement;
                rb2d.velocity = new Vector2(0f, climbVelocity);
                climbed = true;
                grounded = false;
            }
			else {
                climbed = false;
            }
        }
        else {
            climbed = false;
            rb2d.gravityScale = gravityStore;   // setting gravity to its previous value
        }
			
        // Push state
        if (leftPushingZone) {
            if (Input.GetAxis("Horizontal") > 0)	{
                animator.enabled = true;
                pushed = true;
                maxSpeed = 0;
            }
			else if(Input.GetAxis("Horizontal") == 0) {
                pushed = false;
            }
        } else if (rightPushingZone) {
            if (Input.GetAxis("Horizontal") < 0) {
                animator.enabled = true;
                pushed = true;
                maxSpeed = 0;
            }
            else if (Input.GetAxis("Horizontal") == 0) {
                pushed = false;
            }
        }
		else {
            pushed = false;
        }

    }

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Ladder")) {
			ladderPos = other.transform.position;
		}
	}

	private void OnTriggerStay2D(Collider2D other) {
		if (other.CompareTag("Ladder")) {
			ladderPos = other.transform.position;
		}
	}

	// Knockback function
	public IEnumerator Knockback(float knockbackDuration, float knockbackPower, Vector3 knockbackDir) {
		float timer = 0;

		while (knockbackDuration > timer) {
			timer += Time.deltaTime;

			rb2d.AddForce(new Vector3(knockbackDir.x * 1000, knockbackDir.y * knockbackPower, transform.position.z));
		}
		
		yield return 0;
	}
	// the function that makes player's sprite red when he gets damage
	public IEnumerator ChangeColor() {
		GetComponent<SpriteRenderer>().color = Color.red;
		yield return new WaitForSeconds(0.3f);
		GetComponent<SpriteRenderer>().color = Color.white;
		yield return new WaitForSeconds(0.3f);
	}

	// When the player is dead, set his body type as Static in order to prevent him from moving
	void MakePlayerStatic() {
		rb2d.bodyType = RigidbodyType2D.Static;
		dead = true;
	}

	// TODO change this function when the "YOU DIED" scene is ready
	void Die() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);	// for now, we are restarting the game when we die
	}
}
