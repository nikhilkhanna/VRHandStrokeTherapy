using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WristExerciseInstructions : MonoBehaviour {
	public TextMesh instruction;
	double maxDorsiflexion;
	double maxPalmarFlexion;
	double maxRadialDeviation;
	double maxUlnarDeviation;
	string instructionText;
	void Start () {
		instruction = GetComponent<TextMesh>();
		instructionText= "Welcome to your VR Range of Motion Diagnosis Session. \nRemember to move slowly and keep your hand firm.";

		
	}
	public void updateDorsiflexion(double maxFlexion)
	{
		this.maxDorsiflexion = maxFlexion;
		updateText ();
	}
	public void updateInstruction (string instructionText)
	{
		this.instructionText = instructionText;
		updateText ();
	}
	public void updatePalmarFlexion (double maxExtension)
	{
		this.maxPalmarFlexion = maxExtension;
		updateText ();
	}
	public void updateRadialDeviation (double maxPronation)
	{
		this.maxRadialDeviation = maxPronation;
		updateText ();
	}
	public void updateUlnarDeviation (double maxSupination)
	{
		this.maxUlnarDeviation = maxSupination;
		updateText ();
	}
	
	public void updateText()
	{
		instruction.text = instructionText +  "\nRadial Deviation: " + Mathf.Rad2Deg * maxRadialDeviation + "\nUlnar Deviation: " + Mathf.Rad2Deg * maxUlnarDeviation + "\nDorsiflexion: " + Mathf.Rad2Deg * maxDorsiflexion + "\nPalmar Flexion: " + Mathf.Rad2Deg * maxPalmarFlexion;
	}
}
