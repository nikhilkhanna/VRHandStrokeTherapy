using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Score : MonoBehaviour {
	private float lastTime = 0;
	public int score = 0;
	TextMesh instruction;
	// Use this for initialization
	void Start () {
		instruction = GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - lastTime >= 1) {
			instruction.text = ((int) Time.time) + " sec - " + score + " pts";
			lastTime = Time.time;
		}

	}
}
