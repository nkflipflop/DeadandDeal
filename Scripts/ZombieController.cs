using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonoBehaviour {

    public float minX;
    public float maxX;

	public int health = 2;

    public GameObject player;

	public bool grounded;
    public bool idle;
    public bool attacked;
	public bool attack_started;
	public bool walking;
	public bool died;

    public Rigidbody2D physic;
    private Animator animator;

    Vector3 distance_among;
	private RaycastHit2D hit;
	bool hh = true;


	void Start ()
    {
        physic = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        physic.position = new Vector3(Mathf.Clamp(physic.position.x, minX, maxX), physic.position.y, 0.0f); // interval of walking
		attack_started = false;
    }
	
    void FixedUpdate () {
		if (grounded) {	// if the zombie is in the air, it will not walk, attack, or chase the player. He'll just fall.
			float distance_btw_player_and_zombie = Vector3.Distance(transform.position, player.transform.position);
			Vector3 distance = transform.position - player.transform.position;
			Vector3 rayToPlayer = player.transform.position - transform.position;
			hit = GetFirstRaycastHit(rayToPlayer);
			animator.SetBool("Attacked", attacked);
			animator.SetBool("Idle", idle);
			animator.SetBool("Walking", walking);
			animator.SetBool("Died", died);

			attacked = false;
			idle = false;

			distance_among = (player.transform.position - transform.position).normalized;

			if (hit.collider != null && hit.collider.name != "Player" && hit.collider.name != "AttackTrigger") {
				if (walking)
					ZombieWalk();
			}
			else if (distance_btw_player_and_zombie < 10 && !died) {  //when player gets close
				transform.localScale = new Vector3((distance_among.x < 0) ? -1 : 1, 1, 1);  //direction adjustment

				if (distance_btw_player_and_zombie > 1 && !attack_started) {   //go towards to player
					walking = true;
					attacked = false;
					idle = false;
					distance_among.y = 0;
					distance_among.z = 0;
					physic.velocity = new Vector2(0, 0);
					if (Mathf.Abs(distance_among.x) > 0.1f)
						transform.position += distance_among * Time.deltaTime;
					else {
						idle = true;
						walking = false;
					}
				}
				if (Mathf.Abs(distance.x) < 1.3f && Mathf.Abs(distance.y) < 1.5f && player.GetComponent<PlayerController>().dead == false) {    // Attack state
					idle = false;
					walking = false;
					attacked = true;
					attack_started = true;
                    physic.velocity = new Vector2(0, 0);
                }
			}
			else if (!died && !idle && walking) {   //ordinary walking of zombie
				ZombieWalk();
			}
		}

		if (health <= 0) {
			died = true;
		}

    }

	void ZombieWalk() {
		attacked = false;

		if (maxX - physic.position.x > 0.5f && hh) {
			transform.position += new Vector3(1, 0, 0) * Time.deltaTime * 0.5f;
			transform.localScale = new Vector3(1, 1, 1);
		}
		else if (physic.position.x - maxX < 0.5f && hh) {
			hh = false;
			transform.localScale = new Vector3(-1, 1, 1);
		}
		else if (physic.position.x - minX > 0.5f && !hh) {
			transform.position += new Vector3(-1, 0, 0) * Time.deltaTime * 0.5f;
			transform.localScale = new Vector3(-1, 1, 1);
		}
		else if (physic.position.x - minX < 0.5f && !hh) {
			hh = true;
			transform.localScale = new Vector3(1, 1, 1);
		}
	}

	void SetStates() {
		int state = Random.Range(1, 3);

		if (state == 1) {
			idle = true;
			walking = false;
		}
		else {
			idle = false;
			walking = true;
		}
	}

	public RaycastHit2D GetFirstRaycastHit(Vector2 direction) {
		//Check "Queries Start in Colliders" in Edit > Project Settings > Physics2D
		RaycastHit2D[] hits = new RaycastHit2D[2];
		Physics2D.RaycastNonAlloc(transform.position, direction, hits);
		//hits[0] will always be the Collider2D you are casting from.
		return hits[1];
	}

	private void OnTriggerEnter2D(Collider2D other) {
		PlayerController playerScript = player.GetComponent<PlayerController>();
		if (other.CompareTag("Player")) {
			Vector3 zombie_to_player = player.transform.position - gameObject.transform.position;
			StartCoroutine(playerScript.Knockback(0.02f, 100, zombie_to_player));	// when the player takes damage, apply a knockback
			StartCoroutine(playerScript.ChangeColor());		// when the player takes damage, change its sprite's color to red for a short time
			playerScript.currHealth -= 20;
		}
	}

	void AttackFinishes() {
		attack_started = false;
	}

	void StartDying() {     // some changes should be made when the zombie dies. This function exists because of that
		transform.localScale = new Vector3((distance_among.x < 0) ? -1 : 1, 1, 1);  //direction adjustment
		physic.bodyType = RigidbodyType2D.Static;
		physic.GetComponent<PolygonCollider2D>().enabled = false;
		physic.GetComponentInChildren<BoxCollider2D>().enabled = false;	// Disable zombie's ground checker collider
	}

	void DestroyZombie() {
		Destroy(gameObject);
	}
}