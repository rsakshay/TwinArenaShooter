using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public int playerNumber = 1;
    public float MAX_SPEED = 10;
    public int HP = 10;
    public GameObject rocket;

    enum RocketState
    {
        Available = 0,
        Fired
    }
    
    private Rigidbody2D rgb2d;
    private RocketState rocketState = RocketState.Available;

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
        vel.x *= 0.9f;

        float x = Input.GetAxis("Horizontal_P" + playerNumber);
        float y = 0;

        if (Input.GetButtonDown("Jump_P" + playerNumber) && IsGrounded())
        {
            y = 5;
        }

        if (Input.GetButtonDown("Fire1_P" + playerNumber) && rocketState == RocketState.Available)
        {
            //Fire
            GameObject laaunchedRocket = Instantiate(rocket, transform.position + (Vector3.up * 2), Quaternion.identity);
            laaunchedRocket.GetComponent<Rocket>().LiftOff(playerNumber);

            rocketState = RocketState.Fired;
        }


        vel.x += x;

        if (Mathf.Abs(vel.x) > MAX_SPEED)
        {
            vel.x = MAX_SPEED * (Mathf.Abs(vel.x) / vel.x);
        }

        rgb2d.velocity = vel;
        rgb2d.AddForce(Vector2.up * y, ForceMode2D.Impulse);
    }

    public void TakeDamage(int val)
    {
        HP -= val;

        if (HP <= 0)
            Destroy(gameObject);
    }

    public void OnRocketDestroyed()
    {
        rocketState = RocketState.Available;
    }

    private bool IsGrounded()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + new Vector3(0, GetComponent<BoxCollider2D>().bounds.extents.y), Vector2.down, GetComponent<BoxCollider2D>().bounds.extents.y + 0.5f);

        foreach (RaycastHit2D hitGO in hits)
        {
            if (hitGO.collider.gameObject.GetHashCode() != gameObject.GetHashCode())
                return true;
        }

        return false;
    }
}
