using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public int playerNumber = 1;
    public float MAX_SPEED = 10;
    public int HP = 10;
    public GameObject rocket;
    
    private bool isJumping = false;
    private Rigidbody2D rgb2d;

    // Use this for initialization
    void Start () {

        rgb2d = GetComponent<Rigidbody2D>();

	}
	
	// Update is called once per frame
	void Update () {
        HandleInputs();
	}

    void HandleInputs()
    {
        Vector2 vel = rgb2d.velocity;

        if (vel.y > 0)
        {
            vel.y *= 0.9f;

            if (vel.y < 0.001f)
                isJumping = false;
        }

        float x = Input.GetAxis("Horizontal_P" + playerNumber);
        float y = 0;

        if (Input.GetButtonDown("Jump_P" + playerNumber) && !isJumping)
        {
            y = 10;
            isJumping = true;
        }

        if (Input.GetButtonDown("Fire1_P" + playerNumber))
        {
            //Fire
            GameObject laaunchedRocket = Instantiate(rocket, transform.position + (Vector3.up * 2), Quaternion.identity);
            laaunchedRocket.GetComponent<Rocket>().LiftOff(playerNumber);
        }


        vel.x += x;
        vel.y += y;

        if (Mathf.Abs(vel.x) > MAX_SPEED)
        {
            vel.x = MAX_SPEED * (Mathf.Abs(vel.x) / vel.x);
        }

        rgb2d.velocity = vel;
    }

    public void TakeDamage(int val)
    {
        HP -= val;

        if (HP <= 0)
            Destroy(gameObject);
    }
}
