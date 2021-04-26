using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
알고리즘 (ctw0727)

- 관절이 시작되는 위치를 점A, 두 관절이 연결되는 위치를 점B, 목표위치를 점C로 둔다
- A와 C를 중심으로 하고 관절의 두 길이를 반지름으로 하는 두 원의 교점을 구하는 방식으로 점B의 위치를 구함
- 선AB와 선BC의 중점에 각각 해당하는 관절을 배치하고 각도를 조정
*/

public class KR_RoboticArmPrecoding : MonoBehaviour
{
	private Vector3 MouseStaticPos;				// 마우스의 위치를 받아오기 위한 Vector3
	
	private Transform Root, Main, Last;			// Scene 안의 실제 시작지점, 관절위치, 목표지점 (Control용도)
	
    private Vector3 PosStart, PosDef;			// 실제 위치에서 받아온 가상의 Vector3 시작지점, 목표지점
	private Vector3 PosAnkle;					// 계산된 가상의 관절 위치를 저장하는 Vector3
	private Vector3 PosAnkle_last;				// 마지막으로 계산된 가상의 관절 위치를 저장하는 Vector3
	
	private bool Lefted;						// 관절이 어느쪽으로 휠지 정하는 boolean
	
	public float StoA, AtoD;					// 관절의 길이를 정하기 위한 float
	
	public Camera MainCamera;					// 마우스 위치를 받아오기 위한 카메라(Camera)
	
	/// <summary>
	/// Pos1에서 Pos2로 향하는 각도(Quaternion)를 반환
	/// </summary>
	private Quaternion Get_Pos_rotation(Vector3 Pos1, Vector3 Pos2){
        
        float angle = Mathf.Atan2(Pos2.y-Pos1.y, Pos2.x-Pos1.x) * Mathf.Rad2Deg;
        
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        return rotation;
    }
	
	/// <summary>
	/// 입력받은 두 좌표간의 거리를 반환
	/// </summary>
    private float Get_Vector3_Range(Vector3 Pos1, Vector3 Pos2){
		
        return Mathf.Sqrt(Mathf.Pow((Pos1.x - Pos2.x),2)+Mathf.Pow((Pos1.y - Pos2.y),2));
    }
	
	/// <summary>
	/// 두 좌표의 중간지점의 좌표를 반환
	/// </summary>
	private Vector3 Get_Vector3_Middle(Vector3 P1, Vector3 P2){
		return new Vector3(P2.x + (P1.x - P2.x)/2, P2.y + (P1.y - P2.y)/2, 0);
	}
	
	/// <summary>
	/// PosAnkle의 좌표를 구하는 함수
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
	/// 시작지점과 가까운 뼈대의 좌표를 반환하는 함수
	/// </summary>
	private Vector3 get_Pos_AnkleA(){
		PosAnkle = Get_Pos_Ankle(PosStart, PosDef);
		return Get_Vector3_Middle(PosAnkle, PosStart);
	}
	
	/// <summary>
	/// 목표지점과 가까운 뼈대의 좌표를 반환하는 함수
	/// </summary>
	private Vector3 get_Pos_AnkleB(){
		PosAnkle = Get_Pos_Ankle(PosStart, PosDef);
		return Get_Vector3_Middle(PosDef, PosAnkle);
	}
	
	/// <summary>
	/// 시작지점과 가까운 뼈대의 각도를 반환하는 함수
	/// </summary>
	private Quaternion get_Quaternion_AnkleA(){
		PosAnkle = Get_Pos_Ankle(PosStart, PosDef);
		return Get_Pos_rotation(PosStart, PosAnkle);
	}
	
	/// <summary>
	/// 목표지점괴 가까운 뼈대의 각도를 반환하는 함수
	/// </summary>
	private Quaternion get_Quaternion_AnkleB(){
		PosAnkle = Get_Pos_Ankle(PosStart, PosDef);
		return Get_Pos_rotation(PosAnkle, PosDef);
	}
	
	/// <summary>
	/// 마우스의 위치를 받아오는 함수
	/// </summary>
	private void getMouseStaticPosition(){
		MouseStaticPos = new Vector3(MainCamera.ScreenToWorldPoint(Input.mousePosition).x,MainCamera.ScreenToWorldPoint(Input.mousePosition).y,0);
	}
	
	/// <summary>
	/// PosStart와 PosDef의 관절 위치를 받아오는 함수, 함수 실행순서를 조정하기 위해 따로 선언함
	/// </summary>
	private void getPrivatePosition(){
		PosStart = Root.position;
		PosDef = MouseStaticPos;
	}
	
	/// <summary>
	/// 관절이 어느 방향으로 휘어야 하는지를 갱신하는 함수
	/// </summary>
	private void getLefted(){
		if (PosStart.x >= PosDef.x) Lefted = false;
		else Lefted = true;
	}
	
	/// <summary>
	/// 뼈대들의 위치와 각도를 조정하는 함수
	/// </summary>
	private void updateAnklePosition(){
		Main.position = get_Pos_AnkleA();
		Last.position = get_Pos_AnkleB();
		Main.rotation = get_Quaternion_AnkleA();
		Last.rotation = get_Quaternion_AnkleB();
	}
	
	// 구동부
	
	void Start(){						// GameObject의 Transform을 받아옴
		Root = GameObject.Find("RootAnkle").GetComponent<Transform>();
		Main = GameObject.Find("MainAnkle").GetComponent<Transform>();
		Last = GameObject.Find("LastAnkle").GetComponent<Transform>();
	}
	
	void Update(){
		getMouseStaticPosition();		// 마우스의 위치를 실시간으로 받아옴
		getPrivatePosition();			// 관절 구현에 필요한 관절 시작위치와 목표위치를 갱신
		getLefted();					// 관절이 어느 방향으로 휘어야 하는지 갱신
		updateAnklePosition();			// 관절의 위치를 계산하고 적용
	}
}