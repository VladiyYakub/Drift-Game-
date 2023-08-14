using UnityEngine;
using System.Collections.Generic;

public class VehicleCamera : MonoBehaviour
{
    public static VehicleCamera Instance { get; private set; }

    public float Smooth = 0.3f;
    public float Distance = 5.0f;
    public float Height = 1.0f;
    public float Angle = 20;

    public LayerMask LineOfSightMask = 0;

    [HideInInspector]
    public Transform Target;
    [HideInInspector]
    public List<Transform> CameraSwitchView;

    private bool farCameraView = false;
    private Vector3 farCameraPosition;
    private Vector3 velocity = Vector3.zero;

    private float Xsmooth;
    private float farDistance = 0.0f;
    private float zAngleAmount = 0.0f;
    private float currentDistance;
    private int Switch = -1;

    void Awake()
    {
        Instance = this;
        farCameraPosition = transform.position;
    }

    void Start()
    {
        SetupFarCameraPosition();
    }

    void LateUpdate()
    {
        if (GameUI.Manage.GameFinished)
            Switch = 4;

        if (Switch == -1)
        {
            HandleMainCameraView();
        }
        else if (Switch < AIControl.CurrentVehicle.cameraView.cameraSwitchView.Count)
        {
            HandleSwitchCameraView();
        }
        else
        {
            HandleFarCameraView();
        }
    }

    public void CameraSwitch()
    {
        Switch++;
        if (Switch > AIControl.CurrentVehicle.cameraView.cameraSwitchView.Count) { Switch = -1; }
    }

    private float AccelerationAngle()
    {
        zAngleAmount = Mathf.Clamp(zAngleAmount, -45.0f, 45.0f);
        zAngleAmount = Mathf.Lerp(zAngleAmount, Input.acceleration.x * -70.0f, Time.deltaTime * 2.0f);
        return zAngleAmount;
    }

    private float AdjustLineOfSight(Vector3 target, Vector3 direction)
    {
        RaycastHit hit;

        if (Physics.Raycast(target, direction, out hit, currentDistance, LineOfSightMask.value))
            return hit.distance;
        else
            return currentDistance;
    }

    private void SetupFarCameraPosition()
    {        
        Transform nextNode = AIControl.Instance.FirstAINode.GetComponent<AINode>().NextAINode;
        for (int i = 0; i < 6; i++)
        {
            nextNode = nextNode.GetComponent<AINode>().NextAINode;
        }
        farCameraPosition = nextNode.position + new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(5.0f, 10.0f), Random.Range(-5.0f, 5.0f));
    }

    private void HandleMainCameraView()
    {
        RenderSettings.flareStrength = 0.3f;
        GetComponent<Camera>().fieldOfView = Mathf.Clamp(AIControl.CurrentVehicle.speed / 10.0f + 60.0f, 60, 90.0f);
        currentDistance = Distance;

        float yAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, Target.eulerAngles.y, ref velocity.y, Smooth);
        float xAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x, Target.eulerAngles.x + (Angle), ref velocity.x, Smooth);

        transform.eulerAngles = new Vector3(xAngle, yAngle, AccelerationAngle());
        Xsmooth = Mathf.Lerp(Xsmooth, velocity.y, Time.deltaTime * 10.0f);

        var direction = transform.rotation * -new Vector3(-Xsmooth / 300.0f, 0, 1);
        var targetDistance = AdjustLineOfSight(Target.position + new Vector3(0, Height, 0), direction);
        transform.position = Target.position + new Vector3(0, Height, 0) + direction * targetDistance;
    }

    private void HandleSwitchCameraView()
    {
        RenderSettings.flareStrength = 0.3f;
        GetComponent<Camera>().fieldOfView = 60;
        transform.position = CameraSwitchView[Switch].position;
        transform.rotation = Quaternion.Lerp(transform.rotation, CameraSwitchView[Switch].rotation, Time.deltaTime * 5.0f);
    }

    private void HandleFarCameraView()
    {
        if (farDistance > 120.0f && !farCameraView)
        {           
            farCameraView = true;
        }

        RenderSettings.flareStrength = 0.0f;
        GetComponent<Camera>().fieldOfView = Mathf.Clamp(50.0f - (farDistance / 2.0f), 10.0f, 120.0f);
        var newRotation = Quaternion.LookRotation(Target.position - transform.position);

        transform.position = farCameraPosition;
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 15);
    }

}


