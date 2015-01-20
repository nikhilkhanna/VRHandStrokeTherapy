using UnityEngine;
using System.Collections;
using Parse;

public class InputEmail : MonoBehaviour {
	public static bool complete = false;
	private TextMesh mytext;
	public static string email;
	// Use this for initialization
	void Start () {
		mytext = gameObject.GetComponent<TextMesh> ();

		ParseObject testObject = new ParseObject ("TestObject");
		testObject ["food"] = "bar";
		testObject.SaveAsync ();
	}
	
	// Update is called once per frame
	void Update() {
		email = mytext.text;
		if (!complete)
		{
			foreach (char c in Input.inputString) {
				if ((int) c == 8)
				{
					if (mytext.text.Length != 0)
						mytext.text = mytext.text.Substring(0, mytext.text.Length - 1);
				}
				
				else if ((int) c == 10 || (int) c == 13) {
						complete = true;
				}
				else
				{
					mytext.text += c;
				}
			}
		}
	}
}
