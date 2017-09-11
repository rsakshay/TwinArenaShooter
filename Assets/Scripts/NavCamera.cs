using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavCamera : MonoBehaviour {

    public static NavCamera Instance { get { return _instance; } }
    public enum CameraState
    {
        Static = 0,
        Moving
    }
    public CameraState currentState = CameraState.Static;
    public float moveSpeed = 2;

    private static NavCamera _instance = null;
    private Vector3 targetPos = Vector3.zero;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case CameraState.Static:
                break;

            case CameraState.Moving:
                if ((int)transform.position.magnitude == (int)targetPos.magnitude)
                {
                    transform.position = targetPos;
                    currentState = CameraState.Static;
                    GameManager.Instance.StartGame();
                }
                else
                    transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
                break;
        }
    }

    public void MoveToNewLocation(Vector2 position)
    {
        targetPos = new Vector3(position.x, position.y, transform.position.z);
        currentState = CameraState.Moving;
    }
}
