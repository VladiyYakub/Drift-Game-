using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VehicleMode { Player = 0, AICar = 1 }
public enum ControlMode { Simple = 1, Mobile = 2 }
public class AIControl : MonoBehaviour
{
    public static AIControl Instance;
    public static VehicleControl CurrentVehicle;

    public ControlMode ControlMode = ControlMode.Simple;
    public Transform FirstAINode;
    public Transform StartPoint;
    public GameObject[] CarsPrefabs;

    void Awake()
    {
        Instance = this; 
    }

    void Start()
    {
        GameObject InstantiatedCar = InstantiateCar();
        SetupAIVehicle(InstantiatedCar);
        SetupCamera(InstantiatedCar);
    }

    private GameObject InstantiateCar()
    {
        return Instantiate(CarsPrefabs[PlayerPrefs.GetInt("CurrentVehicle")], StartPoint.position, StartPoint.rotation) as GameObject;
    }

    private void SetupAIVehicle(GameObject car)
    {
        car.GetComponent<AIVehicle>().NextNode = FirstAINode;
        CurrentVehicle = car.GetComponent<VehicleControl>();
    }

    private void SetupCamera(GameObject car)
    {
        VehicleCamera.Instance.Target = car.transform;
        VehicleCamera.Instance.CameraSwitchView = CurrentVehicle.cameraView.cameraSwitchView;
        VehicleCamera.Instance.Distance = CurrentVehicle.cameraView.distance;
        VehicleCamera.Instance.Height = CurrentVehicle.cameraView.height;
        VehicleCamera.Instance.Angle = CurrentVehicle.cameraView.angle;
    }
}