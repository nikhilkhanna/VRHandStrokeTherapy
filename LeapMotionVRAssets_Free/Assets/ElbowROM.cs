using UnityEngine;
using System.Collections;
using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;

public class ElbowROM : MonoBehaviour {
	private float[][] elbowExtensionData = new float[][] { new float[] {0, 0, 10f/45},new float[] {45, 10, 15f/35},new float[] {80, 25, 75f/60} };
	private float[][] elbowFlexionData = new float[][] {new float[] {0, 100, -75f/80}, new float[]{80, 25, -15f/30},new float[] {110, 10, -10f/30} };
	private float[][] elbowSupinationData = new float[][] {new float[] {-60, 0, 15f/80},new float[] {20, 15, 15f/20}, new float[]{40, 30, 50f/20} };
	private float[][] elbowPronationData = new float[][] { new float[]{-60, 100, -30f/40}, new float[]{-20, 70, -55f/40},new float[] {20, 15, -15f/40} };
	private float[][] wristDorsiflexionData = new float[][] { new float[]{-60, 0, 30f/70}, new float[]{10, 30, 70f/50} };
	private float[][] wristPalmarFlexionData = new float[][] { new float[]{-60, 100, -40f/20}, new float[]{-40, 60, -30f/30},new float[] {-10, 30, -30f/70} };
	private float[][] wristRadialDeviationData = new float[][] { new float[]{-20, 0, 30f/30}, new float[]{10, 30, 70f/20} };
	private float[][] wristUlnarDeviationData = new float[][] { new float[]{-20, 100, -70f/20}, new float[]{0, 30, -30f/30} };


	// Myo game object to connect with.
	// This object must have a ThalmicMyo script attached.
	public GameObject myo = null;
	
	// A rotation that compensates for the Myo armband's orientation parallel to the ground, i.e. yaw.
	// Once set, the direction the Myo armband is facing becomes "forward" within the program.
	// Set by making the fingers spread pose or pressing "r".
	private Quaternion _antiYaw = Quaternion.identity;
	
	// A reference angle representing how the armband is rotated about the wearer's arm, i.e. roll.
	// Set by making the fingers spread pose or pressing "r".
	private float _referenceRoll = 0.0f;
	
	// The pose from the last update. This is used to determine if the pose has changed
	// so that actions are only performed upon making them rather than every frame during
	// which they are active.
	private Pose _lastPose = Pose.Unknown;
	
	//elbow stuff
	private double maxFlexion = 0;
	private double maxExtension = Mathf.PI/2; //actually min
	private double maxPronation = 0;
	private double maxSupination = 0; //actually min
	private ElbowExerciseInstructions display;
	private bool calibrated = false;
	private bool detectingPS = true;


	//wrist stuff
	private double maxDorsiflexion = 0;
	private double maxPalmarFlexion = 0; //actually min
	private double maxRadialDeviation = 0;
	private double maxUlnarDeviation = 0; //actually min
	private WristExerciseInstructions displayWrist;
	private bool detectingDP = false;

	//both
	private int updatesSinceMaxChange = 0;
	private const int UPDATES_THRESHOLD = 150;
	private bool detectingMotion1=true;
	private bool hasMoved=false;
	private bool testingElbow=true;

	//scoring
	private float elbowImpairment;
	private float wristImpairment;
	
	
	// Use this for initialization
	void Start () {
		displayWrist =(WristExerciseInstructions) GameObject.FindObjectOfType (typeof(WristExerciseInstructions));

		display =(ElbowExerciseInstructions) GameObject.FindObjectOfType (typeof(ElbowExerciseInstructions));
	}
	
	// Update is called once per frame.
	void Update ()
	{
		// Access the ThalmicMyo component attached to the Myo object.
		ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo> ();
		
		// Update references when the pose becomes fingers spread or the q key is pressed.
		bool updateReference = false;
		/*
		if (thalmicMyo.pose != _lastPose) {
			_lastPose = thalmicMyo.pose;
			
			if (thalmicMyo.pose == Pose.Fist) {
				updateReference = true;
				
				ExtendUnlockAndNotifyUserAction(thalmicMyo);
			}
		}
		*/
		if (Input.GetKeyDown ("r")) {
			updateReference = true;
		}
		if (Input.GetKeyDown ("w")) {

			testingElbow = false;
			displayWrist.updateInstruction("Now keep your forearm straight while rotating your hand to\nthe left until you reach your comfortable limit.");
		}
		// Update references. This anchors the joint on-screen such that it faces forward away
		// from the viewer when the Myo armband is oriented the way it is when these references are taken.
		if (updateReference) {
			// _antiYaw represents a rotation of the Myo armband about the Y axis (up) which aligns the forward
			// vector of the rotation with Z = 1 when the wearer's arm is pointing in the reference direction.
			_antiYaw = Quaternion.FromToRotation (
				new Vector3 (myo.transform.forward.x, 0, myo.transform.forward.z),
				new Vector3 (0, 0, 1)
				);
			
			// _referenceRoll represents how many degrees the Myo armband is rotated clockwise
			// about its forward axis (when looking down the wearer's arm towards their hand) from the reference zero
			// roll direction. This direction is calculated and explained below. When this reference is
			// taken, the joint will be rotated about its forward axis such that it faces upwards when
			// the roll value matches the reference.
			Vector3 referenceZeroRoll = computeZeroRollVector (myo.transform.forward);
			_referenceRoll = rollFromZero (referenceZeroRoll, myo.transform.forward, myo.transform.up);
			calibrated = true;
			display.updateInstruction("Rotate your hand counter-clockwise to your comfortable limit.");
		}
		if (calibrated) {
			// Current zero roll vector and roll value.
			Vector3 zeroRoll = computeZeroRollVector (myo.transform.forward);
			float roll = rollFromZero (zeroRoll, myo.transform.forward, myo.transform.up);

			// The relative roll is simply how much the current roll has changed relative to the reference roll.
			// adjustAngle simply keeps the resultant value within -180 to 180 degrees.
			float relativeRoll = normalizeAngle (roll - _referenceRoll);

			// antiRoll represents a rotation about the myo Armband's forward axis adjusting for reference roll.
			Quaternion antiRoll = Quaternion.AngleAxis (relativeRoll, myo.transform.forward);
			Quaternion rotation;
			// Here the anti-roll and yaw rotations are applied to the myo Armband's forward direction to yield
			// the orientation of the joint.
			rotation = _antiYaw * antiRoll * Quaternion.LookRotation (myo.transform.forward);

			// The above calculations were done assuming the Myo armbands's +x direction, in its own coordinate system,
			// was facing toward the wearer's elbow. If the Myo armband is worn with its +x direction facing the other way,
			// the rotation needs to be updated to compensate.

			if (thalmicMyo.xDirection == Thalmic.Myo.XDirection.TowardWrist) {
					// Mirror the rotation around the XZ plane in Unity's coordinate system (XY plane in Myo's coordinate
					// system). This makes the rotation reflect the arm's orientation, rather than that of the Myo armband.
					rotation = new Quaternion (rotation.x,
	                                    -rotation.y,
	                                    rotation.z,
	                                    -rotation.w);
			}


			bool madeChange=false;
			if(testingElbow) {
				if (detectingPS)
				{
					double angle = -rotation.z;
					if (thalmicMyo.arm.Equals("left"))
					{
						angle *= -1;
					}
					if (angle > maxPronation && detectingMotion1) {
						maxPronation = angle;
						display.updatePronation (maxPronation);
						madeChange=true;
					}
					if (angle < maxSupination && !detectingMotion1) {
						maxSupination = angle;
						display.updateSupination (maxSupination);
						madeChange=true;
					}
				}
				else 
				{
					double angle = rotation.x + Mathf.PI / 2;
					
					if (angle > maxFlexion && detectingMotion1) {
						maxFlexion = angle;
						display.updateFlexion (maxFlexion);
						madeChange=true;
					}
					if (angle < maxExtension && !detectingMotion1) {
						maxExtension = angle;
						display.updateExtension (maxExtension);
						madeChange=true;
					}

				}

				if (!madeChange) {
					updatesSinceMaxChange++;
					if (updatesSinceMaxChange>UPDATES_THRESHOLD&&hasMoved) {
						hasMoved=false;
						updatesSinceMaxChange = 0;
						if(detectingPS) {
							if(detectingMotion1) {
								display.updateInstruction("Now rotate your hand clockwise to your comfortable limit.");
								detectingMotion1=false;
							}
							else {
								display.updateInstruction("Well done! Now return your hand to vertical and bring forearm\ntoward upper arm to your comfortable limit.");
								detectingMotion1=true;
								detectingPS=false;
							}
						}
						else {
							if(detectingMotion1) {
								display.updateInstruction("Now extend forearm toward the ground to your comfortable limit.");
								detectingMotion1=false;
							}
							else {
								display.updateInstruction("Good job completing elbow mobility!\nReturn to the start position with your palf facing down then\npress W to continue to your wrist evaluation.");
								detectingMotion1=true;
							}
						}
					}
				}
				else {
					updatesSinceMaxChange=0;
					hasMoved=true;
				}
			}
			//wrist
			else {

				if (!detectingDP)
				{
					double angle = rotation.y;
					if (thalmicMyo.arm.Equals("left"))
					{
						angle *= -1;
					}
					if (angle > maxUlnarDeviation && !detectingMotion1) {
						maxUlnarDeviation = angle;
						displayWrist.updateUlnarDeviation (maxUlnarDeviation);
						madeChange=true;
					}
					if (angle < maxRadialDeviation && detectingMotion1) {
						maxRadialDeviation = angle;
						displayWrist.updateRadialDeviation (maxRadialDeviation);
						madeChange=true;
					}
				}
				else 
				{
					double angle = rotation.x;
					
					if (angle > maxDorsiflexion && detectingMotion1) {
						maxDorsiflexion = angle;
						displayWrist.updateDorsiflexion (maxDorsiflexion);
						madeChange=true;
					}
					if (angle < maxPalmarFlexion && !detectingMotion1) {
						maxPalmarFlexion = angle;
						displayWrist.updatePalmarFlexion (maxPalmarFlexion);
						madeChange=true;
					}
				}
				if (!madeChange) {
					updatesSinceMaxChange++;
					if (updatesSinceMaxChange>UPDATES_THRESHOLD&&hasMoved) {
						hasMoved=false;
						updatesSinceMaxChange = 0;
						if(!detectingDP) {
							if(detectingMotion1) {
								displayWrist.updateInstruction("Now rotate your hand to the right until you reach your comfortable limit.");
								detectingMotion1=false;
							}
							else {
								displayWrist.updateInstruction("Well done! Now return your hand to neutral and tilt it\nupwards until your comfortable limit.");
								detectingMotion1=true;
								detectingDP=true;
							}
						}
						else {
							if(detectingMotion1) {
								displayWrist.updateInstruction("Now tilt your hand downwards until your comfortable limit.");
								detectingMotion1=false;
							}
							else {
								/*
								elbowImpairment=findImpairment(elbowPronationData, elbowSupinationData, elbowFlexionData, elbowExtensionData,
								                               maxPronation, maxSupination, maxFlexion, maxExtension, .4f, .6f);
								wristImpairment=findImpairment(wristRadialDeviationData, wristUlnarDeviationData, wristDorsiflexionData, wristPalmarFlexionData,
								                               maxRadialDeviation, maxUlnarDeviation, maxDorsiflexion, maxPalmarFlexion, .3f, .7f);

*/
								string results = "Good job completing your mobility diagnostic test!\nHere are your results:";
								float ps = findImpairment(elbowPronationData, elbowSupinationData, maxPronation, maxSupination);
								float fe = findImpairment(elbowFlexionData, elbowExtensionData, maxFlexion, maxExtension);
								//ru.3, dp .7
								results += "\nElbow: " + (ps *.4f+ fe*.6f) + "\nPronation/Supination: " + ps;
								results += "\nFlexion/Extension: " + fe;

								float ru = findImpairment(wristRadialDeviationData, wristUlnarDeviationData, maxRadialDeviation, maxUlnarDeviation);
								float dp = findImpairment(wristDorsiflexionData, wristPalmarFlexionData, maxDorsiflexion, maxPalmarFlexion);
								results += "\nWrist: " + (ru * .3f + dp * .7f) + "\nRadial/Ulnar Deviation: " + ru + "\nDorsal/Palmers Flexion: " + dp;
								results += "\nTotal Impairment Score: " + (ps *.4f + fe *.6f + ru *.3f + dp *.7f);
								displayWrist.instruction.text = results;
							}
						}
					}
				}
				else {
					updatesSinceMaxChange=0;
					hasMoved=true;
				}
			}
		}
	}

	float findImpairment(float[][] curve1, float[][] curve2,
	                     double angle1, double angle2)
	{
		float score =(findImpairmentSubscore (curve1, angle1) + findImpairmentSubscore (curve2, angle2));
		return score;
	}

	float findImpairmentSubscore (float[][] curve, double angleAchieved)
	{
		float score = -1;
		for(int segment=curve.Length-1; segment>=0; segment--)
		{
			if (angleAchieved>curve[segment][0])
			{
				score=(float) (curve[segment][1]+(angleAchieved-curve[segment][0])*curve[segment][2]);
				break;
			}
		}
		if(score>100) {score=100;}
		if(score<0) {score=0;}
		return score;
	}
	// Compute the angle of rotation clockwise about the forward axis relative to the provided zero roll direction.
	// As the armband is rotated about the forward axis this value will change, regardless of which way the
	// forward vector of the Myo is pointing. The returned value will be between -180 and 180 degrees.
	float rollFromZero (Vector3 zeroRoll, Vector3 forward, Vector3 up)
	{
		// The cosine of the angle between the up vector and the zero roll vector. Since both are
		// orthogonal to the forward vector, this tells us how far the Myo has been turned around the
		// forward axis relative to the zero roll vector, but we need to determine separately whether the
		// Myo has been rolled clockwise or counterclockwise.
		float cosine = Vector3.Dot (up, zeroRoll);
		
		// To determine the sign of the roll, we take the cross product of the up vector and the zero
		// roll vector. This cross product will either be the same or opposite direction as the forward
		// vector depending on whether up is clockwise or counter-clockwise from zero roll.
		// Thus the sign of the dot product of forward and it yields the sign of our roll value.
		Vector3 cp = Vector3.Cross (up, zeroRoll);
		float directionCosine = Vector3.Dot (forward, cp);
		float sign = directionCosine < 0.0f ? 1.0f : -1.0f;
		
		// Return the angle of roll (in degrees) from the cosine and the sign.
		return sign * Mathf.Rad2Deg * Mathf.Acos (cosine);
	}
	
	// Compute a vector that points perpendicular to the forward direction,
	// minimizing angular distance from world up (positive Y axis).
	// This represents the direction of no rotation about its forward axis.
	Vector3 computeZeroRollVector (Vector3 forward)
	{
		Vector3 antigravity = Vector3.up;
		Vector3 m = Vector3.Cross (myo.transform.forward, antigravity);
		Vector3 roll = Vector3.Cross (m, myo.transform.forward);
		
		return roll.normalized;
	}
	
	// Adjust the provided angle to be within a -180 to 180.
	float normalizeAngle (float angle)
	{
		if (angle > 180.0f) {
			return angle - 360.0f;
		}
		if (angle < -180.0f) {
			return angle + 360.0f;
		}
		return angle;
	}
	
	// Extend the unlock if ThalmcHub's locking policy is standard, and notifies the given myo that a user action was
	// recognized.
	void ExtendUnlockAndNotifyUserAction (ThalmicMyo myo)
	{
		ThalmicHub hub = ThalmicHub.instance;
		
		if (hub.lockingPolicy == LockingPolicy.Standard) {
			myo.Unlock (UnlockType.Timed);
		}
		
		myo.NotifyUserAction ();
	}
}
