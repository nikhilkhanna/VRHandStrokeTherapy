using UnityEngine;
using System.Collections;

public class Motivation : MonoBehaviour {
	private float threshold = 2.0f;
	private float buffer = 0.5f;
	private bool above = false;
	private GameObject cyl;
	private float timer = 1.0f;
	private float preTime = 0.0f;
	private bool timerOn = false;
	private float preTime2 = 0.0f;
	private bool timerOn2 = false;
	// Use this for initialization
	void Start () {
		cyl = GameObject.FindGameObjectWithTag ("GameController");
	}
	
	// Update is called once per frame
	void Update () {
		if (timerOn && Time.time - preTime >= timer)
		{
			GetComponent<TextMesh>().text = "Twist That Weight!";
			timerOn = false;
		}
		if (timerOn2 && Time.time - preTime2 >= timer)
		{
			GetComponent<TextMesh>().text = "Lift That Weight!";
			timerOn2 = false;
		}
		if (!above && cyl.transform.localPosition.y > threshold) 
		{
			above = true;
			GetComponent<TextMesh>().text = "Twist That Weight!\nHigh as the Sky!";
			timerOn = true;
			preTime = Time.time;
		}
		if (above && cyl.transform.localPosition.y < threshold - buffer) 
		{
			above = false;
			GetComponent<TextMesh>().text = "Lift That Weight!\nShow Off Those Guns!";
			timerOn2 = true;
			preTime2 = Time.time;
		}
	}
}
