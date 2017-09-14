using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public int playerNumber = 1;
    public float MAX_SPEED = 10;
    public int HP = 10;
    public GameObject rocket;
    public float aimSpeed = 2;
    public float InvulnerableTime = 2;
    
    enum RocketState
    {
        Available = 0,
        Fired
    }

    enum PlayerState
    {
        Vulnerable = 0,
        Invulnerable
    }
    
    private Rigidbody2D rgb2d;
    private RocketState rocketState = RocketState.Available;
    private PlayerState playerState = PlayerState.Vulnerable;
    private GameObject aim;
    private Vector3 startPos;
    private Color baseColor;
    private float timeHit;

    // Use this for initialization
    void Start () {

        rgb2d = GetComponent<Rigidbody2D>();
        aim = transform.GetChild(0).gameObject;
        startPos = transform.position;
        baseColor = GetComponent<SpriteRenderer>().color;
	}
	
	// Update is called once per frame
	void Update () {
        HandleInputs();

        if (playerState == PlayerState.Invulnerable)
        {
            DoInvulnerableAction();
        }
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

        if (rocketState == RocketState.Available)
        {
            float aimDir = Input.GetAxis("RStickHor_P" + playerNumber);

            aim.transform.RotateAround(transform.position, Vector3.forward, -aimDir * aimSpeed);
        }

        if (Input.GetButtonDown("Fire1_P" + playerNumber) && rocketState == RocketState.Available)
        {
            //Fire
            GameObject laaunchedRocket = Instantiate(rocket, transform.position + (aim.transform.up * (GetComponent<BoxCollider2D>().bounds.extents.y + 0.75f)), aim.transform.rotation);
            laaunchedRocket.GetComponent<Rocket>().LiftOff(playerNumber);

            rocketState = RocketState.Fired;

            aim.SetActive(false);
        }


        vel.x += x;

        if (Mathf.Abs(vel.x) > MAX_SPEED)
        {
            vel.x = MAX_SPEED * (Mathf.Abs(vel.x) / vel.x);
        }

        rgb2d.velocity = vel;
        rgb2d.AddForce(Vector2.up * y, ForceMode2D.Impulse);
    }

    public void TakeDamage(int val, bool bypassInvulState = false)
    {
        if (playerState == PlayerState.Vulnerable || bypassInvulState)
        {
            HP -= val;

            if (HP <= 0)
                Destroy(gameObject);

            timeHit = Time.time;
            playerState = PlayerState.Invulnerable;
        }
    }

    public void OnRocketDestroyed()
    {
        rocketState = RocketState.Available;
        aim.SetActive(true);
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

    void DoInvulnerableAction()
    {
        if ((Time.time - timeHit) >= InvulnerableTime)
        {
            GetComponent<SpriteRenderer>().color = baseColor;
            playerState = PlayerState.Vulnerable;
            return;
        }

        GetComponent<SpriteRenderer>().color = Color.Lerp(baseColor, Color.white, Mathf.PingPong(Time.time * 2, 1));
    }

    private void OnBecameInvisible()
    {
        transform.position = startPos;
        rgb2d.velocity = Vector2.zero;
        TakeDamage(1, true);
    }
}
