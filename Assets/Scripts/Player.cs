using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public int playerNumber = 1;
    public float MAX_SPEED = 10;
    public Vector2 velocity;

    private bool m_isAxisInUse = false;
    private Rigidbody2D rgb2d;

    // Use this for initialization
    void Start () {

        rgb2d = GetComponent<Rigidbody2D>();

	}
	
	// Update is called once per frame
	void Update () {
        velocity = rgb2d.velocity;
        HandleInputs();
	}

    void HandleInputs()
    {
        Vector2 vel = rgb2d.velocity;

        if (vel.y > 0)
            vel.y *= 0.9f;

        float x = Input.GetAxis("Horizontal_P" + playerNumber);
        float y = 0;

        if (Input.GetButtonDown("Jump_P" + playerNumber))
        {
            y = 10;
        }

        if (Input.GetAxisRaw("Fire1_P" + playerNumber) != 0)
        {
            if (m_isAxisInUse == false)
            {
                m_isAxisInUse = true;

                //Fire
            }
        }
        else
        {
            m_isAxisInUse = false;
        }


        vel.x += x;
        
        if (vel.y <= 0.001f && vel.y >= 0)
        {
            vel.y += y;
        }

        if (vel.magnitude > MAX_SPEED)
        {
            vel = vel.normalized * MAX_SPEED;
        }

        rgb2d.velocity = vel;
    }
}
