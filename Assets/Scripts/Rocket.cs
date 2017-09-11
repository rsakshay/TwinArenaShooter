using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour {

    public float moveSpeed = 3;
    public float rotAngle = 2;
    public int damage = 2;
    public float MAX_SPEED = 8;

    enum RocketMode
    {
        Normal = 0,
        Boosting,
        Braking
    }

    private int playerNumber;
    private Rigidbody2D rgb2d;
    private bool launched = false;
    private RocketMode currentMode = RocketMode.Normal;

    // Use this for initialization
    void Start () {
        rgb2d = GetComponent<Rigidbody2D>();
        GetComponent<SpriteRenderer>().color = GameManager.Instance.PlayerColors[playerNumber - 1];
    }

    // Update is called once per frame
    void Update () {

        if (launched)
            HandleInputs();

        // Keep accelerating in the forward direction
        Vector2 vel = transform.up * moveSpeed;
        vel += rgb2d.velocity;

        float topSpeed = 0;

        switch (currentMode)
        {
            case RocketMode.Normal:
                topSpeed = MAX_SPEED;
                break;

            case RocketMode.Boosting:
                // Boost to twice the max speed
                topSpeed = 2 * MAX_SPEED;
                break;

            case RocketMode.Braking:
                // Brake to half the max speed
                topSpeed = 0.5f * MAX_SPEED;
                break;

            default:
                break;
        }

        if (vel.magnitude > topSpeed)
            vel = vel.normalized * topSpeed;

        rgb2d.velocity = vel;
    }

    void HandleInputs()
    {
        float steer = Input.GetAxis("RStickHor_P" + playerNumber);

        if (Mathf.Abs(steer) > 0)
        {
            transform.Rotate(new Vector3(0, 0, -rotAngle * steer));

            rgb2d.velocity = transform.up.normalized * rgb2d.velocity.magnitude * 0.99f;
        }

        float boost = Input.GetAxis("Boost_P" + playerNumber);

        if (boost > 0 && currentMode != RocketMode.Boosting)
        {
            rgb2d.velocity = Vector2.Lerp(rgb2d.velocity, rgb2d.velocity.normalized * MAX_SPEED * (1 + boost), 0.25f);
            currentMode = RocketMode.Boosting;
        }      

        float brake = Input.GetAxis("Brake_P" + playerNumber);

        if (brake > 0 && currentMode != RocketMode.Braking)
        {
            rgb2d.velocity = Vector2.Lerp(rgb2d.velocity, rgb2d.velocity * (1 - brake) / 2, 0.25f);
            currentMode = RocketMode.Braking;
        }

        if (boost == 0 && brake == 0)
            currentMode = RocketMode.Normal;

        if (rgb2d.velocity.magnitude > MAX_SPEED && currentMode == RocketMode.Normal)
            rgb2d.velocity = Vector2.Lerp(rgb2d.velocity, rgb2d.velocity.normalized * MAX_SPEED, 1);

    }

    public void LiftOff(int num)
    {
        playerNumber = num;
        launched = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Player playerHit = collision.gameObject.GetComponent<Player>();
        if (playerHit)
            playerHit.TakeDamage(damage);
        DestroyRocket();
    }

    void DestroyRocket()
    {
        GameManager.Instance.GetPlayer(playerNumber).OnRocketDestroyed();
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        DestroyRocket();
    }
}
