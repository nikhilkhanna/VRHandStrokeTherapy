using UnityEngine;
using System.Collections;

public class SceneSwitch : MonoBehaviour
{

		// Use this for initialization
		void Start ()
		{
	
		}
	
		// Update is called once per frame
		void Update ()
		{
			foreach (char c in Input.inputString) {
				if (c=='1')//replace '' with whatever char you want to be the hotkey
				{
					Application.LoadLevel(0); //replace levelname with the name of the scene you want to open, may need to do some stuff with rearranging
				}
			if (c=='2')//replace '' with whatever char you want to be the hotkey
			{
				Application.LoadLevel (1); //replace levelname with the name of the scene you want to open, may need to do some stuff with rearranging
			}
			if (c=='3')//replace '' with whatever char you want to be the hotkey
			{
				Application.LoadLevel (2); //replace levelname with the name of the scene you want to open, may need to do some stuff with rearranging
			}//repeat this if conditional as many times as necessary depending on how many hotkeys you need
			if (c=='4')//replace '' with whatever char you want to be the hotkey
			{
				Application.LoadLevel (3); //replace levelname with the name of the scene you want to open, may need to do some stuff with rearranging
			}//
		}
		}
}

