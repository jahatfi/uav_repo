using UnityEngine;
using System;
using System.Collections;
using XInputDotNetPure; // Required in C#
//Testing Visual Studio Script
//using UnityTest;
using TactorTest;
using Tdk;

public class PlayerController : MonoBehaviour {
	//Xbox 360 controller code:
	bool playerIndexSet;
	private float vib_time;
	PlayerIndex playerIndex;
	GamePadState state;
	GamePadState prevState;

	//Visual Feedback: time, orbs collected, etc
	private int count;
	public GUIText WinText;
	public GUIText timer;
	public GUIText countText;
	
	//Basic Class to test import of .dll
	//MyUtilities utils;

	//Class containing tactors methods
	TactorClass tactor_class;
	public string portID;
	//bool connected;
	int connectingBoardID;
	public int connectingType;
	public int gain;
	public int frequency;
	public int delay;
	public int duration;
	public int tactor_id;
	
	//Player controls
	public float speed;
	public float jumpSpeed;
	public bool IsGrounded;



	// Use this for initialization
	void Start () {
		//Xbox 360 code
		playerIndexSet = false;
		vib_time = 0;

		//Visual feedback
		count = 0;
		WinText.text = "";
		SetTimer ();
		SetCountText();


		//basic test class
		//utils = new MyUtilities();
		//utils.AddValues(2, 3);
		//print("2 + 3 = " + utils.c);

		//Tactor class
		tactor_class = new TestClass();

		//portID = "COM3";
		connectingBoardID = -1;
		//The default will be USB, which is 1. Change this to 2 for Bluetooth
		connectingType = 1;
		gain = 255;
		frequency = 2000;
		duration = 500;
		delay = 0;
		tactor_id = 1;
		//connected = false;

		if (Tdk.TdkInterface.InitializeTI() == 0) print (string.Format("Successfully initialized!"));
		else{
			print (string.Format("Could not be initialized!"));
			print (string.Format(Tdk.TdkDefines.GetLastEAIErrorString()));
        }

		connectingBoardID = Tdk.TdkInterface.Connect(portID,
		                                             connectingType,
		                                             //This line didn't work:(int)Tdk.TdkDefines.DeviceTypes.WinUsb,
		                                             IntPtr.Zero);
		if (connectingBoardID > -1) {
			print (string.Format ("Connected!"));
			tactor_class.PulseTactor (ref connectingBoardID, ref tactor_id, ref duration, ref delay, ref frequency, ref gain);
		}
		else{
			print (string.Format("Could not connect!"));
			print (string.Format(Tdk.TdkDefines.GetLastEAIErrorString()));
		}
		
		/*
		CheckTDKErrors(tactor_class.Connect(ref portID, ref connectingBoardID));
		print (string.Format ("connected is now {0}", connectingBoardID));
		if (connected) {
			tactor_class.PulseTactor (ref connectingBoardID, ref tactor_id, ref duration, ref delay, ref frequency, ref gain);
			Debug.Log (string.Format ("Connected, connectingBoardID is now: {0}", connectingBoardID));                             
		}
		*/
	}//start
	
	// Update is called once per frame
	void Update () {
		//Ensure controller vibrates for no more than one second.

		if (Time.realtimeSinceStartup - vib_time > 1.0) {
			GamePad.SetVibration (playerIndex, 0.0f, 0.0f);
			vib_time = 0.0f;
		}

		// Find a PlayerIndex, for a single player game
		// Will find the first controller that is connected and use it

		if (!playerIndexSet || !prevState.IsConnected)
		{
			for (int i = 0; i < 4; ++i)
			{
				PlayerIndex testPlayerIndex = (PlayerIndex)i;
				GamePadState testState = GamePad.GetState(testPlayerIndex);
				if (testState.IsConnected)
				{
					Debug.Log(string.Format("GamePad found {0}", testPlayerIndex));
					playerIndex = testPlayerIndex;
					playerIndexSet = true;
				}
			}
		}
		
		prevState = state;
		state = GamePad.GetState(playerIndex);
		SetTimer ();
	}

	void FixedUpdate(){
		SetTimer ();
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		float moveUp = 8.0f;

		Vector3 movement = new Vector3(3*moveHorizontal, 0.0f, 3*moveVertical);
	
		if (IsGrounded) {
			if (Input.GetButton ("Jump"))
				movement.y = moveUp * jumpSpeed;
			rigidbody.AddForce (movement * speed * Time.deltaTime);
		}
	}
	void OnCollisionStay (Collision collisionInfo)
	{
		IsGrounded = true;
	}
	void OnCollisionExit (Collision collisionInfo)
	{
		IsGrounded = false;
	}

	void OnTriggerEnter(Collider other){
		//Destroy (other.gameObject);
		if (other.gameObject.tag == "PickUp") {
			other.gameObject.SetActive (false);
			count++;
			SetCountText();

			// Set vibration
			GamePad.SetVibration(playerIndex, 255.0f, 255.0f);
			vib_time = Time.realtimeSinceStartup;

			tactor_class.PulseTactor (ref connectingBoardID, ref tactor_id, ref duration, ref delay, ref frequency, ref gain);
		}

	}

	void SetCountText(){
		countText.text = "Count " + count.ToString ();
		if(count > 12) WinText.text = "Congradulations, you win!";
	}

	void SetTimer(){
		timer.text = "Time: " + Time.realtimeSinceStartup;
	}

	private void CheckTDKErrors(int ret)
	{
		//Debug.Log (string.Format ("Checking for TDK Errors"));
		//if a tdk method returns less then zero then we should display the last error
		//in the tdk interface
		print (ret);
		if (ret < 0)
		{
			//the GetLastEAIErrorString returns a string that represents the last error code
			//recorded in the tdk interface.
			Debug.Log(string.Format(Tdk.TdkDefines.GetLastEAIErrorString()));
		}
	}

	private void CheckTDKErrors(bool flag)
	{
		//Debug.Log (string.Format ("Checking for TDK Errors"));
		//if a tdk method returns less then zero then we should display the last error
		//in the tdk interface
		if (!flag)
		{
			//the GetLastEAIErrorString returns a string that represents the last error code
			//recorded in the tdk interface.
			Debug.Log(string.Format(Tdk.TdkDefines.GetLastEAIErrorString()));
		}
	}

	~PlayerController(){
		print (string.Format("In destructor!"));
		Tdk.TdkInterface.ShutdownTI(); 
	}
	
}





