using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Algorithm (ctw0727)

- 'Point A' is The start position of Robotic Arm
- 'Point B' is The position of two Bones meat at (same with the joint)
- 'Point C' is The targeted position of Robotic Arm

- Point B's value is the intersection of two circles which get Point A and B as Center Point and get each bones length as radius

- Position each bone at Line AB's and Line BC's midpoint and set the Angle of bones
*/

public class EN_RoboticArmPrecoding : MonoBehaviour
{
	private Vector3 MouseStaticPos;				// Vector3 for get mouse position
	
	private Transform Root, Main, Last;			// Transforms that actually in the Scene with GameObjects
	
    private Vector3 PosStart, PosDef;			// Vector3 for 'Point A' and 'Point C'
	private Vector3 PosAnkle;					// Vector3 for 'Point B'
	private Vector3 PosAnkle_last;				// Vector3 for save last value of enable 'Point B'
	
	private bool Lefted;						// boolean that save where the direction will joint bow
	
	public float StoA, AtoD;					// float for save bones's length
	
	public Camera MainCamera;					// Camera for get Mouse position in Unity
	
	/// <summary>
	/// return Angle(Quaternion) of the line from Pos1 to Pos2
	/// </summary>
	private Quaternion Get_Pos_rotation(Vector3 Pos1, Vector3 Pos2){
        
        float angle = Mathf.Atan2(Pos2.y-Pos1.y, Pos2.x-Pos1.x) * Mathf.Rad2Deg;
        
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        return rotation;
    }
	
	/// <summary>
	/// return Length(float) of the line from Pos1 to Pos2
	/// </summary>
    private float Get_Vector3_Range(Vector3 Pos1, Vector3 Pos2){
		
        return Mathf.Sqrt(Mathf.Pow((Pos1.x - Pos2.x),2)+Mathf.Pow((Pos1.y - Pos2.y),2));
    }
	
	/// <summary>
	/// return Midpoint(Vector3) of the line from P1 to P2
	/// </summary>
	private Vector3 Get_Vector3_Middle(Vector3 P1, Vector3 P2){
		return new Vector3(P2.x + (P1.x - P2.x)/2, P2.y + (P1.y - P2.y)/2, 0);
	}
	
	/// <summary>
	/// return Position(Vector3) of PosAnkle
	/// </summary>
	private Vector3 Get_Pos_Ankle(Vector3 P1, Vector3 P2){
		
		float Length = Get_Vector3_Range(P1, P2);
		float buf1, buf2, buf3, buf4;
		buf1 = Mathf.Acos(( Mathf.Pow(StoA, 2) - Mathf.Pow(AtoD, 2) + Mathf.Pow(Length, 2) ) / ( 2 * StoA * Length));
		
		if (Lefted == false){
			buf2 = Mathf.Atan( (P2.y - P1.y) / (P1.x - P2.x) );
			buf3 = P1.x - StoA * Mathf.Cos(buf2 - buf1);
			buf4 = P1.y + StoA * Mathf.Sin(buf2 - buf1);
		}
		else{
			buf2 = Mathf.Atan( (P2.y - P1.y) / (P2.x - P1.x) );
			buf3 = P1.x + StoA * Mathf.Cos(buf2 - buf1);
			buf4 = P1.y + StoA * Mathf.Sin(buf2 - buf1);
		}
		
		if ( Length <= Mathf.Abs(AtoD-StoA) || Length >= AtoD+StoA ){
			return PosAnkle_last;
		}
		
		PosAnkle_last = new Vector3(buf3, buf4, 0f);
		return PosAnkle_last;
	}
	
	/// <summary>
	/// return Position(Vector3) of the bone close from 'Point A'
	/// </summary>
	private Vector3 get_Pos_AnkleA(){
		PosAnkle = Get_Pos_Ankle(PosStart, PosDef);
		return Get_Vector3_Middle(PosAnkle, PosStart);
	}
	
	/// <summary>
	/// return Position(Vector3) of the bone close from 'Point C'
	/// </summary>
	private Vector3 get_Pos_AnkleB(){
		PosAnkle = Get_Pos_Ankle(PosStart, PosDef);
		return Get_Vector3_Middle(PosDef, PosAnkle);
	}
	
	/// <summary>
	/// return Angle(Quaternion) of the bone close from 'Point A'
	/// </summary>
	private Quaternion get_Quaternion_AnkleA(){
		PosAnkle = Get_Pos_Ankle(PosStart, PosDef);
		return Get_Pos_rotation(PosStart, PosAnkle);
	}
	
	/// <summary>
	/// return Angle(Quaternion) of the bone close from 'Point C'
	/// </summary>
	private Quaternion get_Quaternion_AnkleB(){
		PosAnkle = Get_Pos_Ankle(PosStart, PosDef);
		return Get_Pos_rotation(PosAnkle, PosDef);
	}
	
	/// <summary>
	/// Get Mouse cursor's Position
	/// </summary>
	private void getMouseStaticPosition(){
		MouseStaticPos = new Vector3(MainCamera.ScreenToWorldPoint(Input.mousePosition).x,MainCamera.ScreenToWorldPoint(Input.mousePosition).y,0);
	}
	
	/// <summary>
	/// return void, Update the 'Point A' and 'Point C' 's position
	/// </summary>
	private void getPrivatePosition(){
		PosStart = Root.position;
		PosDef = MouseStaticPos;
	}
	
	/// <summary>
	/// return void, Update the direction of joint to bow
	/// </summary>
	private void getLefted(){
		if (PosStart.x >= PosDef.x) Lefted = false;
		else Lefted = true;
	}
	
	/// <summary>
	/// return void, Calculate and Apply the bones
	/// </summary>
	private void updateAnklePosition(){
		Main.position = get_Pos_AnkleA();
		Last.position = get_Pos_AnkleB();
		Main.rotation = get_Quaternion_AnkleA();
		Last.rotation = get_Quaternion_AnkleB();
	}
	
	// Working Stage
	
	void Start(){						// Getting GameObject's Transform
		Root = GameObject.Find("RootAnkle").GetComponent<Transform>();
		Main = GameObject.Find("MainAnkle").GetComponent<Transform>();
		Last = GameObject.Find("LastAnkle").GetComponent<Transform>();
	}
	
	void Update(){
		getMouseStaticPosition();		// Get cursor's position lively
		getPrivatePosition();			// Update the positions that need at code
		getLefted();					// Update which direction the joint will be bowed
		updateAnklePosition();			// Calculate the position and angle of bones and apply
	}
}