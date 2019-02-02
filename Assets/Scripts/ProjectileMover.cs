using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMover : MonoBehaviour {
    public Vector2 velocity;
    public float gravity = 15;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        velocity.y -= gravity * Time.deltaTime;

        transform.position = transform.position + (Vector3)velocity * Time.deltaTime;
	}
}
