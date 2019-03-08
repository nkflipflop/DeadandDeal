using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackTrigger : MonoBehaviour {
	void OnTriggerEnter2D(Collider2D other) {
		if(!other.isTrigger && other.CompareTag("Enemy")) {
			other.gameObject.GetComponent<ZombieController>().health--;
		}
		else if(!other.isTrigger && other.CompareTag("Breakable")){
			other.gameObject.GetComponent<BoxController>().is_broke = true;
		}
	}
}
