using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderZone : MonoBehaviour {

	private PlayerController player;
	public Collider2D platformEffector;

	private void Start() {
		player = FindObjectOfType<PlayerController>();
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if(other.name == "GroundChecker") {
			player.onLadder = true;
		}
	}

	private void OnTriggerStay2D(Collider2D other) {
		if (other.name == "GroundChecker") {
			player.onLadder = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.name == "GroundChecker") {
			player.onLadder = false;
		}
	}
}
