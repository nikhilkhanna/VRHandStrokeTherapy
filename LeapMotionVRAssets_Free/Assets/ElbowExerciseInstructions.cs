using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ElbowExerciseInstructions : MonoBehaviour {
	TextMesh instruction;
	double maxFlexion;
	double maxExtension = Mathf.PI /2 ;
	double maxSupination;
	double maxPronation;
	string instructionText;
	void Start () {
		instruction = GetComponent<TextMesh>();
		instructionText= "Welcome to your VR Range of Motion Diagnosis Session.\nRemember to move slowly and keep your hand firm.\nPlace upper arm against your side with your lower arm parallel to\nthe floor and your hand vertical. Then press R to begin\nreading pronation/flexion data.";
		updateText ();

	}
	public void updateFlexion(double maxFlexion)
	{
		this.maxFlexion = maxFlexion;
		updateText ();
	}
	public void updateInstruction (string instructionText)
	{
			this.instructionText = instructionText;
			updateText ();
	}
	public void updateExtension (double maxExtension)
	{
		this.maxExtension = maxExtension;
		updateText ();
	}
	public void updatePronation (double maxPronation)
	{
		this.maxPronation = maxPronation;
		updateText ();
	}
	public void updateSupination (double maxSupination)
	{
		this.maxSupination = maxSupination;
		updateText ();
	}

	public void updateText()
	{
		instruction.text = instructionText +  "\nPronation: " + Mathf.Rad2Deg * maxPronation + "\nSupination: " + Mathf.Rad2Deg * maxSupination + "\nFlexion: " + Mathf.Rad2Deg * maxFlexion + "\nExtension: " + Mathf.Rad2Deg * maxExtension ;
	}
}
