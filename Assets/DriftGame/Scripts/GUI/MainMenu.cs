using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum Panels { MainMenu = 0, SelectVehicle = 1, SelectLevel = 2, Settings = 3 }

public class MainMenu : MonoBehaviour
{
    public float CameraRotateSpeed = 5;
    public Animator FadeBackGround;

    public AudioSource MenuMusic;
    public Transform VehicleRoot;
    public Material[] AllRestMaterials;

    public MenuPanels menuPanels;
    public MenuGUI menuGUI;
    public VehicleSetting[] vehicleSetting;
    public LevelSetting[] levelSetting;

    [System.Serializable]
    public class MenuGUI
    {
        public Text GameScore;

        public Toggle Audio;
        public Toggle Music;

        public Image WheelColor;
        public Image SmokeColor;
        public Image LoadingBar;

        public GameObject loading;
        public GameObject CustomizeVehicle;
    }

    [System.Serializable]
    public class MenuPanels
    {
        public GameObject MainMenu;
        public GameObject SelectVehicle;
        public GameObject SelectLevel;
        public GameObject Settings;
    }

    [System.Serializable]
    public class VehicleSetting
    {

        public GameObject Vehicle;
        public GameObject WheelSmokes;

        public Material RingMat;
        public Material SmokeMat;
        public Transform RearWheels;        

        [System.Serializable]
        public class VehiclePower
        {
            public float speed = 80;
            public float braking = 1000;
            public float nitro = 10;
        }
    }

    [System.Serializable]
    public class LevelSetting
    {
        public bool locked = true;
        public Button Panel;
        public Text BestTime;
        public Image LockImage;
        public StarClass Stars;

        [System.Serializable]
        public class StarClass
        {
            public Image Star1, Star2, Star3;
        }
    }

    private Panels activePanel = Panels.MainMenu;

    private float x, y = 0;

    private VehicleSetting currentVehicle;

    private int currentVehicleNumber = 0;   

    private Color mainColor;
    private bool randomColorActive = false;    
    private AsyncOperation sceneLoadingOperation = null;   

    void Awake()
    {
        InitializeScene();
        LoadAudioSettings();
        LoadVehicleSettings();
        InitializeVehicles();
    }

    private void InitializeScene()
    {
        AudioListener.pause = false;
        Time.timeScale = 1.0f;       
        CurrentPanel(0);
    }

    private void LoadAudioSettings()
    {
        bool isAudioActive = PlayerPrefs.GetInt("AudioActive") == 0;
        menuGUI.Audio.isOn = isAudioActive;
        AudioListener.volume = isAudioActive ? 1.0f : 0.0f;

        bool isMusicActive = PlayerPrefs.GetInt("MusicActive") == 0;
        menuGUI.Music.isOn = isMusicActive;
        MenuMusic.mute = !isMusicActive;
    }

    private void LoadVehicleSettings()
    {
        currentVehicleNumber = PlayerPrefs.GetInt("CurrentVehicle");
        currentVehicle = vehicleSetting[currentVehicleNumber];
    }

    private void InitializeVehicles()
    {
        for (int i = 0; i < vehicleSetting.Length; i++)
        {
            VehicleSetting VSetting = vehicleSetting[i];    

            VSetting.Vehicle.SetActive(i == currentVehicleNumber);
            if (i == currentVehicleNumber) currentVehicle = VSetting;
        }
    }
  
    void Update()
    {
        UpdateLoadingBar();
        HandleWheelRotationAndSmoke();
        HandleCameraRotation();
    }

    private void UpdateLoadingBar()
    {
        if (sceneLoadingOperation != null)
        {
            menuGUI.LoadingBar.fillAmount = Mathf.MoveTowards(menuGUI.LoadingBar.fillAmount, sceneLoadingOperation.progress + 0.2f, Time.deltaTime * 0.5f);

            if (menuGUI.LoadingBar.fillAmount > sceneLoadingOperation.progress)
                sceneLoadingOperation.allowSceneActivation = true;
        }
    }

    private void HandleWheelRotationAndSmoke()
    {
        if (menuGUI.SmokeColor.gameObject.activeSelf || randomColorActive)
        {
            vehicleSetting[currentVehicleNumber].RearWheels.Rotate(1000 * Time.deltaTime, 0, 0);
            vehicleSetting[currentVehicleNumber].WheelSmokes.SetActive(true);
        }
        else
        {
            vehicleSetting[currentVehicleNumber].WheelSmokes.SetActive(false);
        }
    }

    private void HandleCameraRotation()
    {
        if ((Input.GetMouseButton(0) || Input.touchCount == 1) && activePanel != Panels.SelectLevel)
        {
            float inputAxisX = Input.GetMouseButton(0) ? Input.GetAxis("Mouse X") : Input.GetTouch(0).deltaPosition.x;
            AdjustCameraRotation(inputAxisX, 1.0f); 
        }
        else
        {
            AdjustCameraRotation(CameraRotateSpeed * 0.02f, 0.25f); 
        }
    }    

    private void AdjustCameraRotation(float input, float lerpSpeed)
    {
        x = Mathf.Lerp(x, Mathf.Clamp(input * 0.25f, -2, 2) * CameraRotateSpeed, Time.deltaTime * lerpSpeed); 
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 50, 60);
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, (lerpSpeed == 5.0f) ? 50 : 60, Time.deltaTime);
        transform.RotateAround(VehicleRoot.position, Vector3.up, x);
    }

    public void ActiveCurrentColor(Image activeImage)
    {
        mainColor = activeImage.color;

        if (menuGUI.WheelColor.gameObject.activeSelf)
        {
            vehicleSetting[currentVehicleNumber].RingMat.SetColor("_Color", mainColor);            
        }
        else if (menuGUI.SmokeColor.gameObject.activeSelf)
        {
            vehicleSetting[currentVehicleNumber].SmokeMat.SetColor("_TintColor", new Color(mainColor.r, mainColor.g, mainColor.b, 0.2f));            
        }
    }

    public void ActiveWheelColor(Image activeImage)
    {
        randomColorActive = false;

        activeImage.gameObject.SetActive(true);
        menuGUI.WheelColor = activeImage;
        menuGUI.SmokeColor.gameObject.SetActive(false);
    }

    public void ActiveSmokeColor(Image activeImage)
    {
        randomColorActive = false;

        activeImage.gameObject.SetActive(true);
        menuGUI.SmokeColor = activeImage;
        menuGUI.WheelColor.gameObject.SetActive(false);
    }

    public void OutCustomizeVehicle()
    {
        randomColorActive = false;
        menuGUI.WheelColor.gameObject.SetActive(false);
        menuGUI.SmokeColor.gameObject.SetActive(false);
    }

    public void SettingActive(bool activePanel)
    {
        menuPanels.Settings.gameObject.SetActive(activePanel);
    }

    public void ClickExitButton()
    {
        Application.Quit();
    }

    private void SetActivePanel(Panels panel)
    {
        menuPanels.MainMenu.SetActive(panel == Panels.MainMenu);
        menuPanels.SelectVehicle.SetActive(panel == Panels.SelectVehicle);
        menuPanels.SelectLevel.SetActive(panel == Panels.SelectLevel);
        menuPanels.Settings.SetActive(panel == Panels.Settings);
    }

    public void CurrentPanel(int current)
    {
        SetActivePanel(activePanel);
        activePanel = (Panels)current;

        if (currentVehicleNumber != PlayerPrefs.GetInt("CurrentVehicle"))
        {
            currentVehicleNumber = PlayerPrefs.GetInt("CurrentVehicle");

            foreach (VehicleSetting VSetting in vehicleSetting)
            {

                if (VSetting == vehicleSetting[currentVehicleNumber])
                {
                    VSetting.Vehicle.SetActive(true);
                    currentVehicle = VSetting;
                }
                else
                {
                    VSetting.Vehicle.SetActive(false);
                }
            }
        }

        switch (activePanel)
        {

            case Panels.MainMenu:
                menuPanels.MainMenu.SetActive(true);
                menuPanels.SelectVehicle.SetActive(false);
                menuPanels.SelectLevel.SetActive(false);
                if (menuGUI.WheelColor) menuGUI.WheelColor.gameObject.SetActive(true);

                break;
            case Panels.SelectVehicle:
                menuPanels.MainMenu.gameObject.SetActive(false);
                menuPanels.SelectVehicle.SetActive(true);
                menuPanels.SelectLevel.SetActive(false);
                break;
            case Panels.SelectLevel:
                menuPanels.MainMenu.SetActive(false);
                menuPanels.SelectVehicle.SetActive(false);
                menuPanels.SelectLevel.SetActive(true);
                break;
            case Panels.Settings:
                menuPanels.MainMenu.SetActive(false);
                menuPanels.SelectVehicle.SetActive(false);
                menuPanels.SelectLevel.SetActive(false);
                break;
        }
    }

    public void DisableAudioButton(Toggle toggle)
    {
        if (toggle.isOn)
        {
            AudioListener.volume = 1;
            PlayerPrefs.SetInt("AudioActive", 0);
        }
        else
        {
            AudioListener.volume = 0;
            PlayerPrefs.SetInt("AudioActive", 1);
        }
    }

    public void DisableMusicButton(Toggle toggle)
    {
        if (toggle.isOn)
        {
            MenuMusic.GetComponent<AudioSource>().mute = false;
            PlayerPrefs.SetInt("MusicActive", 0);
        }
        else
        {
            MenuMusic.GetComponent<AudioSource>().mute = true;
            PlayerPrefs.SetInt("MusicActive", 1);
        }
    }
}