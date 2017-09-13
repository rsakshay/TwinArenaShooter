using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillator : MonoBehaviour {

    public Vector3 finalPos;
    public float slowdownFactor = 1;

    Vector3 startPos;

	// Use this for initialization
	void Start () {
        startPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.Lerp(startPos, finalPos, Mathf.Abs(Mathf.Sin(Time.timeSinceLevelLoad / slowdownFactor)));
	}
}
