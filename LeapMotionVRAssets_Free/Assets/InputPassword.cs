using UnityEngine;
using System.Collections;
using Parse;

public class InputPassword : MonoBehaviour {
	public bool complete = false;
	private TextMesh mytext;
	public static string pswd;
	bool cancelFirst = true;
	// Use this for initialization
	void Start () {
		mytext = gameObject.GetComponent<TextMesh> ();
	}
	
	// Update is called once per frame
	void Update() {
				pswd = mytext.text;
				if (InputEmail.complete) {
						if (cancelFirst) {
								cancelFirst = false;
						} else if (!complete) {
								foreach (char c in Input.inputString) {
										if ((int)c == 8) {
												if (mytext.text.Length != 0)
														mytext.text = mytext.text.Substring (0, mytext.text.Length - 1);
										} else if ((int)c == 10 || (int)c == 13) {
						pswd = mytext.text;
												ParseUser.LogInAsync (InputEmail.email, pswd).ContinueWith (t =>
												{
														if (t.IsFaulted || t.IsCanceled) {
																// Fck u
																//Application.LoadLevel (2);
														} else {
																//Application.LoadLevel (1); //#YAY
														}

												});
										}
					else{
						mytext.text += c;
					}

								}

						}

				}
		}
}
