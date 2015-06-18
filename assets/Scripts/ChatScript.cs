using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;


//Make this a singletion to allow for global, singular, access
public class ChatScript : MonoBehaviour {

	private List<string> messages = new List<string>();
	public List<string> messagesFromHQ = new List<string>();
	public List<float> messageTimes = new List<float>();
	private Vector2 scrollPosition = Vector2.zero;
	private int messageCounter = 0;
	public int textLines;
	private string message;
	//private Vector2 position = g
	public Button sendButton;
	public InputField input;
	private int viewHeight = 200;

	//Singleton stuff
	private static ChatScript _instance;
	public static ChatScript instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = GameObject.FindObjectOfType<ChatScript>();
				
				//Tell unity not to destroy this object when loading a new scene!
				DontDestroyOnLoad(_instance.gameObject);
			}
			
			return _instance;
		}
	}
	void Awake() 
	{
		if(_instance == null)
		{
			//If I am the first instance, make me the Singleton

			_instance = this;
			DontDestroyOnLoad(this);
			Debug.Log(String.Format("Waking up chat script."));
		}
		else
		{
			//If a Singleton already exists and you find
			//another reference in scene, destroy it!
			if(this != _instance)
				Destroy(this.gameObject);
		}

	}

	private void OnGUI() {

		//GUI.TextField (new Rect (0, (float)(.95*Screen.height), (float)(.125*Screen.width*.8), (float)(.05*Screen.height)), message);
		//message = EditorGUI.TextField (new Rect (0, (float)(.95*Screen.height), (float)(.125*Screen.width*.8), (float)(.05*Screen.height)), message);
		//GUI.Button(new Rect((float)(.125*Screen.width*.8), (float)(.95*Screen.height), (float)(.125*Screen.width/5), (float)(.05*Screen.height)), "Send");
		//scrollPosition.y = 16 * messages.Count;
		scrollPosition = GUI.BeginScrollView(new Rect(0, (float)(.75*Screen.height), (float)(.25*Screen.width), (float)(.2*Screen.height)), scrollPosition, new Rect(-5, 0, 80, viewHeight), false, true);
		//scrollPosition.y = 16 * messages.Count;
		//GUI.Button(new Rect(0, 0, 100, 20), "Top-left");
		//GUI.Label(new Rect(0, 0, 100, 22),"message");
		//GUI.Label(new Rect(0, 16, 100, 22),"message2");
		for(int i = 0; i < messages.Count; ++i)
			GUI.Label(new Rect(0, 16*i, 400, 22),messages[i]);

			//GUILayout.Label(mn,
		GUI.EndScrollView();

		/*
		GUILayout.BeginHorizontal(GUILayout.Width(250));
		input = GUILayout.TextField (input);
		if(GUILayout.Button("Send")) messages.Add(input);

		foreach (string m in messages)
			GUILayout.Label (m);
		*/
		input.Select ();
		//if(Input.GetButtonDown("Send Message")) sendMessage();
	}

	private void getViewHeight(){
		int temp = 16 * messages.Count;
		if (temp > 200)
				viewHeight = temp;
	}


	public void sendMessage(){
		message = input.text.Trim ();
		if (message != string.Empty) {

			messages.Add ("You: " + message);
			input.text = "";
			updateScrollPosition ();
			//Debug.Log(string.Format("You sent:" + input.text));
			//GUI.SetNextControlName ("input");
			//EditorGUI.FocusTextInControl ("input");
			getViewHeight();
			input.Select ();
			if(input.isFocused) Debug.Log("Input field is currently in focus.");
			else  Debug.Log("Input field is NOT focused.");
		}
		//EditorGUI.FocusTextInControl ("InputField");
	}

	public void Update()
	{
		if (messagesFromHQ.Count == messageTimes.Count  && messageCounter < messageTimes.Count) {
			if (Time.time > messageTimes [messageCounter]) {

				messages.Add ("HQ: " + messagesFromHQ [messageCounter]);
				++messageCounter;
				updateScrollPosition();
			}
		}

		if(Input.GetButtonDown("Send Message"))
		{
			sendMessage();
			input.Select ();
			Debug.Log(String.Format("You just sent a message by pressing the 'return' key."));
			if(input.isFocused) Debug.Log("Input field is currently in focus.");
		}
		//input.Select ();

	}

	public List<string> getMessages(){ return messages;}

	private void updateScrollPosition(){
		scrollPosition.y = 16 * (messages.Count - textLines);
		if (scrollPosition.y < 0)
			scrollPosition.y = 0;
	}







	
}
