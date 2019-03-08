using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundChecker : MonoBehaviour {

	private PlayerController player;

	private void Start() {
		player = gameObject.GetComponentInParent<PlayerController>();
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (null == player) {
			Start();
		}
		if (other.CompareTag("Ground") || other.CompareTag("Box") || other.CompareTag("Breakable")) {
			player.grounded = true;
			player.came_from_air = false;
		}
	}

	void OnTriggerStay2D(Collider2D other) {
		if (other.CompareTag("Ground") || other.CompareTag("Box") || other.CompareTag("Breakable")) {
			if(other.name == "PlatformEffector" && player.vertical_movement < 0f) {		// exception for ladders
				other.gameObject.GetComponent<BoxCollider2D>().enabled = false;
			}
			else
				player.grounded = true;
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if(other.CompareTag("Ground") || other.CompareTag("Box") || other.CompareTag("Breakable"))
			player.grounded = false;
	}
}
