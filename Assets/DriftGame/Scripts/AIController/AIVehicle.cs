using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIVehicle : MonoBehaviour
{

    public float СarRestTime = 5.0f;

    public float ForwardSpeed = 1.0f;
    public float SteerSpeed = 1.0f;
    public float NextNodeDistance = 5;

    
    public Transform FrontPoint;

    [HideInInspector]
    public Transform CurrentNode;
    [HideInInspector]
    public Transform NextNode;
    [HideInInspector]
    public Transform WrongNode;

    [HideInInspector]
    public int CurrentLap = 0;

    [HideInInspector]
    public float PlayerCurrentTime;
    [HideInInspector]
    public float PlayerBestTime;
    [HideInInspector]
    public float PlayerLastTime = 0.0f;

    [HideInInspector]
    public float AIAccel;
    [HideInInspector]
    public float AISteer = 0.0f;

    [HideInInspector]
    public bool AIBrake = false;

    private VehicleControl vehicleControl;

    private bool goNextNode = true;
    private bool getLap = false;

    private int carPreviousNodes = 0;
    private float targetAngle;
    private float restTimeer = 0.0f;

    void Start()
    {
        restTimeer = СarRestTime;
        CurrentLap = 0;
        vehicleControl = transform.GetComponent<VehicleControl>();
        CurrentNode = NextNode;
    }

    void Update()
    {
        if (GameUI.Manage.GameStarted)
            PlayerCurrentTime += Time.deltaTime;

        AICarControl();
    }
    void AICarControl()
    {


        if (AIControl.Instance.FirstAINode == WrongNode && !getLap)
        {
            CurrentLap++;
            PlayerLastTime = PlayerCurrentTime;

            if (CurrentLap == 1) GameUI.Manage.GameFinished = true;

            if (PlayerBestTime == 0.0f || PlayerBestTime > PlayerCurrentTime) PlayerBestTime = PlayerCurrentTime;

            PlayerCurrentTime = 0.0f;
            getLap = true;
        }
        else if (AIControl.Instance.StartPoint != CurrentNode)
        {
            getLap = false;
        }

        Vector3 CurrentNodeForward = CurrentNode.TransformDirection(Vector3.forward);
        Vector3 CurrentNodetoOther = CurrentNode.position - FrontPoint.position;

        Vector3 NextNodeForward = NextNode.TransformDirection(Vector3.forward);
        Vector3 NextNodetoOther = NextNode.position - FrontPoint.position;

        carPreviousNodes = Mathf.Clamp(carPreviousNodes, 0, 6);


        if (Mathf.Abs(Quaternion.Dot(NextNode.rotation, FrontPoint.rotation)) < 0.5f && !GameUI.Manage.GameFinished)
            GameUI.Manage.CarWrongWay = true;
        else
            GameUI.Manage.CarWrongWay = false;


        if (NextNode)
        {

            if (vehicleControl.VehicleMode == VehicleMode.AICar)
            {
                if (Vector3.Distance(FrontPoint.position, NextNode.position) < NextNodeDistance && NextNode != CurrentNode)
                {
                    CurrentNode = NextNode;
                    WrongNode = NextNode;
                    goNextNode = true;
                }

            }
            else if (vehicleControl.VehicleMode == VehicleMode.Player)
            {

                if (Vector3.Dot(NextNodeForward, NextNodetoOther) < 0.0f)
                {

                    carPreviousNodes--;
                    CurrentNode = NextNode;

                    if (WrongNode != null && WrongNode.GetComponent<AINode>().NextAINode == NextNode) WrongNode = NextNode;

                    goNextNode = true;
                }
                else if (Vector3.Dot(CurrentNodeForward, CurrentNodetoOther) > 0.0f && CurrentNode.GetComponent<AINode>().PreviousNode != CurrentNode)
                {
                    carPreviousNodes++;
                    CurrentNode = CurrentNode.GetComponent<AINode>().PreviousNode;

                    if (carPreviousNodes == 5)
                    {
                        carPreviousNodes = 0;
                        transform.GetComponent<Rigidbody>().Sleep();
                        transform.rotation = WrongNode.rotation;
                        transform.position = WrongNode.position + Vector3.up;

                        CurrentNode = WrongNode;
                    }

                    goNextNode = true;
                }
            }

            if (Vector3.Distance(FrontPoint.position, NextNode.position) > NextNodeDistance * 5)
            {
                carPreviousNodes = 0;
                transform.GetComponent<Rigidbody>().Sleep();
                transform.rotation = WrongNode.rotation;
                transform.position = WrongNode.position + Vector3.up;

                CurrentNode = WrongNode;
            }
        }

        GameUI.Manage.CarBrakeWarning = false;

        if (CurrentNode.GetComponent<AINode>())
        {
            AINode Nodescript = CurrentNode.GetComponent<AINode>();

            if (Mathf.Abs(AISteer) > 0.0f && vehicleControl.speed > 70.0f && Nodescript.NodeAISetting.braking)
            {
                AIAccel = -Mathf.Abs(AISteer);
                GameUI.Manage.CarBrakeWarning = true;
            }
            else
            {
                AIAccel = 0.2f;
            }

            if (goNextNode)
            {
                NextNode = Nodescript.NextAINode;
                goNextNode = false;
            }
        }

        var relativeTarget = transform.InverseTransformPoint(NextNode.position);

        targetAngle = Mathf.Atan2(relativeTarget.x, relativeTarget.z);
        targetAngle *= Mathf.Rad2Deg;
        targetAngle = Mathf.Clamp(targetAngle, -65, 65);

        AISteer = Mathf.SmoothStep(AISteer, targetAngle / 60, SteerSpeed / 3.0f);
    }
}
