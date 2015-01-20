using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Parse;

public class Frisbee : MonoBehaviour {
	private bool live;
	private Vector3 init;
	private float limit;
	private float startTime;
	private float distance;
	private float buffer;
	private float xzero;
	private float yzero;
	private int quadrant;
	private static Dictionary<string, int> dict = new Dictionary<string, int>();
	private static float startMin = 1000;
	private static float fastest = 0;
	public Score score;
	public bool first = false;
	public void Start()
	{
		score = (Score) Score.FindObjectOfType (typeof(Score));
		live = true;
		init = rigidbody.velocity;
		buffer = 1000.0f;
		limit = distance / init.magnitude + buffer;
		startTime = Time.time;
		xzero = 0.0f;
		yzero = 0.7f;
		fastest = init.magnitude;
		Vector3 pos = gameObject.transform.position;
		if (pos.x>0)
		{
			if(pos.y>0)
			{
				quadrant = 1;
			}
			else
			{
				quadrant = 4;
			}
		}
		else
		{
			if (pos.y>0)
			{
				quadrant = 2;
			}
			else
			{
				quadrant = 3;
			}
		}
		if (Time.time<startMin)
		{
			first = true;
			startMin = Time.time;
			dict.Add ("hit1", 0);
			dict.Add ("hit2", 0);
			dict.Add ("hit3", 0);
			dict.Add ("hit4", 0);
			dict.Add ("miss1", 0);
			dict.Add ("miss2", 0);
			dict.Add ("miss3", 0);
			dict.Add ("miss4", 0);
		}
	}
	void Update()
	{
//		rigidbody.velocity = new Vector3(0, 0, 0);
		if (!rigidbody.velocity.Equals(init) && live)
	    {
			live = false;
			dict["hit"+quadrant.ToString ()]+=1;
			//say succeeded, send data
			score.score += 1;
		}
		if (Time.time > startTime + limit && live)
		{
			live = false;
			dict["miss"+quadrant.ToString ()]+=1;
			//say failed, send data
		}
	}
	void onApplicationQuit()
	{
		if (first)
		{
			ParseObject quadrantsObject = new ParseObject ("PatientFrisbeeSession");
			quadrantsObject ["quadrantsDict"] = dict; 
			quadrantsObject ["frisbeesSwatted"] = dict["hit1"]+dict["hit2"]+dict["hit3"]+dict["hit4"];
			quadrantsObject ["maxFrisbeeSpeed"] = fastest;
			quadrantsObject.SaveAsync ();
		}
	}
}
