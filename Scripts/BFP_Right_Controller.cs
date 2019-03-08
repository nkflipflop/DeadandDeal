﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BFP_Right_Controller : MonoBehaviour {

    private PlayerController player;
    private Rigidbody2D body;

    void Start() {
        player = FindObjectOfType<PlayerController>();
        body = GetComponentInParent<Rigidbody2D>();
        body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.name == "Player") {
            player.rightPushingZone = true;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.name == "Player")
            player.rightPushingZone = true;
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.name == "Player") {
            player.rightPushingZone = false;
            player.maxSpeed = 3;
            body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
    }
}
