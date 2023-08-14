using UnityEngine;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private Transform _mapPlane;
    [SerializeField] private Transform _miniMapCamera;
    [SerializeField] private float _cameraHeightAboveMap = 25f;
    [SerializeField] private float _cameraRotationX = 90f;

    private Transform currentVehicleTransform;

    private void Start()
    {
        if (AIControl.CurrentVehicle != null)
        {
            currentVehicleTransform = AIControl.CurrentVehicle.transform;
        }        
    }

    private void Update()
    {
        if (currentVehicleTransform != null)
        {
            UpdateMiniMapCamera();
        }
    }

    private void UpdateMiniMapCamera()
    {
        Vector3 newPosition = new Vector3(
            currentVehicleTransform.position.x,
            _mapPlane.position.y + _cameraHeightAboveMap,
            currentVehicleTransform.position.z);

        _miniMapCamera.position = newPosition;

        Vector3 newRotation = new Vector3(
            _cameraRotationX,
            currentVehicleTransform.eulerAngles.y,
            0);

        _miniMapCamera.eulerAngles = newRotation;
    }
}

