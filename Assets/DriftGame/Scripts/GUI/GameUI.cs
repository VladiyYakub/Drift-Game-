using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class GameUI : MonoBehaviour
{
    public static GameUI Manage;

    public Panels PanelsMenu;
    public CarUI CurrentCarUI;
    public Sounds SoundsGame;

    [HideInInspector]
    public int TotalDriftCoins;

    [HideInInspector]
    public int EarnCoins;
    public int TotalEarnCoins;

    [HideInInspector]
    public bool CanDrift;
    [HideInInspector]
    public int DriftAmount = 0;

    [HideInInspector]
    public float PenaltyTime = 0.0f;

    [HideInInspector]
    public bool GameStarted;
    public bool GamePaused;
    public bool GameRest;
    public bool GameFinished = false;
    
    [HideInInspector]
    public int RacePrize;
    public int TotalPrize = 0;  

    private float menuLoadTime = 0.0f;
    private AsyncOperation sceneLoadingOperation = null;

    private AIVehicle AIVehicleScript;
    private float timerDrift = 1.0f;

    [HideInInspector]
    public bool CarPenalty = false;
    [HideInInspector]
    public bool CarWrongWay = false;
    [HideInInspector]
    public bool CarBrakeWarning = false;

    private int gearst = 0;
    private float thisAngle = -150;

    private float startTimer = 1.0f;
    private int startCont = 3;

    [System.Serializable]
    public class Panels
    {

        public GameStart gameStart;
        public GamePlay gamePlay;
        public GamePuased gamePuased;
        public GameFinish gameFinish;

        [System.Serializable]
        public class GameStart
        {
            public GameObject Root;
            public Text StartTimeUI;

            public GameObject Loading;
            public Image LoadingBar;

            public Animator FadeBackGround;
        }

        [System.Serializable]
        public class GamePlay
        {
            public GameObject Root;
            public GameObject ButtonsUI;
            public GameObject AccelUI;
            public Image WrongWay;
            public Image DriftWheel;
            public Image BrakeWarning;
            public Text CurrentTime;
            public Text BestTime;
            public Text DriftCoins;
            public Text DriftText;
            public Text DriftXText;
            public Text PenaltyText;

        }
        [System.Serializable]
        public class GamePuased
        {
            public GameObject Root;
            public Toggle AudioToggle;
            public Toggle MusicToggle;
        }
        [System.Serializable]
        public class GameFinish
        {
            public GameObject Root;
            public Text YourTime;
            public Text PenaltyTime;
            public Text BestTime;
            public Text RacePrize;
            public Text DriftPrize;
            public Text TotalPrize;
            public StarClass Stars;
        }

        [System.Serializable]
        public class StarClass
        {
            public float Star1Time;
            public float Star2Time;
            public float Star3Time;
            public Image Star1;
            public Image Star2;
            public Image Star3;
        }
    }

    [System.Serializable]
    public class CarUI
    {
        public Image TachometerNeedle;
        public Image barShiftGUI;

        public Text SpeedText;
        public Text GearText;
    }


    [System.Serializable]
    public class Sounds
    {
        public AudioSource Music;
        public AudioClip CountDown, CountStart;
    }
    void Awake()
    {


        Manage = this;
        AudioListener.pause = false;
        Time.timeScale = 1.0f;

        if (PlayerPrefs.GetFloat("BestTime" + Application.loadedLevel.ToString()) != 0.0f)
            PanelsMenu.gamePlay.BestTime.text = "Best: " + FormatSeconds(PlayerPrefs.GetFloat("BestTime" + Application.loadedLevel.ToString()));

        PanelsMenu.gamePuased.AudioToggle.isOn = (PlayerPrefs.GetInt("AudioActive") == 0) ? true : false;
        PanelsMenu.gamePuased.MusicToggle.isOn = (PlayerPrefs.GetInt("MusicActive") == 0) ? true : false;

        AudioListener.volume = (PlayerPrefs.GetInt("AudioActive") == 0) ? 1.0f : 0.0f;
        SoundsGame.Music.mute = (PlayerPrefs.GetInt("MusicActive") == 0) ? false : true;
    } 

    void Update()
    {

        StartingGameTimer();

        ShowCarUI();

        if (sceneLoadingOperation != null)
        {
            PanelsMenu.gameStart.LoadingBar.fillAmount = Mathf.MoveTowards(PanelsMenu.gameStart.LoadingBar.fillAmount, sceneLoadingOperation.progress + 0.2f, Time.deltaTime * 0.5f);

            if (PanelsMenu.gameStart.LoadingBar.fillAmount > sceneLoadingOperation.progress)
                sceneLoadingOperation.allowSceneActivation = true;
        }

        if (CarPenalty) { PenaltyTime += Time.deltaTime; PanelsMenu.gamePlay.PenaltyText.color = Color.red; } else { PanelsMenu.gamePlay.PenaltyText.color = Color.white; }

        if (CarWrongWay) { PanelsMenu.gamePlay.WrongWay.gameObject.SetActive(true); } else { PanelsMenu.gamePlay.WrongWay.gameObject.SetActive(false); }

        if (CarBrakeWarning) { PanelsMenu.gamePlay.BrakeWarning.color = new Color(1, 1, 1, 1); } else { PanelsMenu.gamePlay.BrakeWarning.color = new Color(1, 1, 1, 0.2f); }


        PanelsMenu.gamePlay.CurrentTime.text = "Time: " + FormatSeconds(AIControl.CurrentVehicle.AIVehicle.PlayerCurrentTime);
        PanelsMenu.gamePlay.BestTime.text = "Best: " + FormatSeconds(PlayerPrefs.GetFloat("BestTime" + Application.loadedLevel));
        PanelsMenu.gamePlay.PenaltyText.text = "Penalty: " + FormatSeconds(PenaltyTime).ToString();

        PanelsMenu.gamePlay.DriftCoins.text = "Drift Coins: " + TotalDriftCoins.ToString();


        if (!GameFinished && !CarWrongWay)
        {
            if (timerDrift == 0)
                CanDrift = true;
            else
                timerDrift = Mathf.MoveTowards(timerDrift, 0.0f, Time.deltaTime);

            if ((DriftAmount / 100.0f) > 1.0f)
            {
                timerDrift = 1.0f;
                EarnCoins = (DriftAmount - 100);

                PanelsMenu.gamePlay.DriftWheel.fillAmount = 1.0f;
                PanelsMenu.gamePlay.DriftWheel.rectTransform.Rotate(0, 0, -500.0f * Time.deltaTime);
                PanelsMenu.gamePlay.DriftText.text = (DriftAmount - 100).ToString();

                PanelsMenu.gamePlay.DriftXText.gameObject.SetActive(true);


                if (EarnCoins > 300)
                {
                    if (PanelsMenu.gamePlay.DriftXText.text == "2X")
                    {
                        PanelsMenu.gamePlay.DriftXText.GetComponent<Animator>().Play(0);
                        PanelsMenu.gamePlay.DriftXText.text = "3X";
                    }
                    TotalEarnCoins = EarnCoins * 3;
                }
                else if (EarnCoins > 150)
                {
                    if (PanelsMenu.gamePlay.DriftXText.text == "1X")
                    {
                        PanelsMenu.gamePlay.DriftXText.GetComponent<Animator>().Play(0);
                        PanelsMenu.gamePlay.DriftXText.text = "2X";
                    }
                    TotalEarnCoins = EarnCoins * 2;

                }
                else if (EarnCoins > 0)
                {
                    PanelsMenu.gamePlay.DriftXText.text = "1X";
                    TotalEarnCoins = EarnCoins;
                }
            }
            else if (CanDrift)
            {
                if (PanelsMenu.gamePlay.DriftWheel.fillAmount == 1)
                {
                    TotalDriftCoins += TotalEarnCoins;
                    TotalEarnCoins = 0;
                    EarnCoins = 0;
                }

                PanelsMenu.gamePlay.DriftWheel.fillAmount = DriftAmount / 100.0f;
                PanelsMenu.gamePlay.DriftWheel.rectTransform.rotation = Quaternion.identity;
                PanelsMenu.gamePlay.DriftText.text = "";
                PanelsMenu.gamePlay.DriftXText.gameObject.SetActive(false);
            }
        }
        else
        {
            DriftAmount = 0;
            PanelsMenu.gamePlay.DriftWheel.fillAmount = 0.0f;
            PanelsMenu.gamePlay.DriftWheel.rectTransform.rotation = Quaternion.identity;
            PanelsMenu.gamePlay.DriftText.text = "";
            PanelsMenu.gamePlay.DriftXText.gameObject.SetActive(false);

            if (GameFinished)
            {
                if (PanelsMenu.gameFinish.Root.activeSelf == false)
                {

                    AIVehicleScript = AIControl.CurrentVehicle.AIVehicle;

                    PanelsMenu.gameFinish.YourTime.text = "Current: " + FormatSeconds(AIVehicleScript.PlayerBestTime);
                    PanelsMenu.gameFinish.PenaltyTime.text = "Penalty: " + FormatSeconds(PenaltyTime);

                    if (AIVehicleScript.PlayerBestTime < PlayerPrefs.GetFloat("BestTime" + Application.loadedLevel) || PlayerPrefs.GetFloat("BestTime" + Application.loadedLevel) == 0)
                    {
                        PlayerPrefs.SetFloat("BestTime" + Application.loadedLevel, AIVehicleScript.PlayerBestTime);
                        PanelsMenu.gameFinish.BestTime.text = "Best: " + FormatSeconds(AIVehicleScript.PlayerBestTime);
                    }
                    else
                    {
                        PanelsMenu.gameFinish.BestTime.text = "Best: " + FormatSeconds(PlayerPrefs.GetFloat("BestTime" + Application.loadedLevel));
                    }

                    if (AIVehicleScript.PlayerBestTime < (PanelsMenu.gameFinish.Stars.Star3Time - PenaltyTime))
                    {

                        PanelsMenu.gameFinish.Stars.Star1.color = Color.white;
                        PanelsMenu.gameFinish.Stars.Star2.color = Color.white;
                        PanelsMenu.gameFinish.Stars.Star3.color = Color.white;

                        RacePrize = (int)((Application.loadedLevel / 5.0f) * 3000.0f);
                        PlayerPrefs.SetInt("LevelStar" + Application.loadedLevel, 3);

                    }
                    else if (AIVehicleScript.PlayerBestTime < (PanelsMenu.gameFinish.Stars.Star2Time - PenaltyTime))
                    {

                        PanelsMenu.gameFinish.Stars.Star1.color = Color.white;
                        PanelsMenu.gameFinish.Stars.Star2.color = Color.white;

                        RacePrize = (int)((Application.loadedLevel / 5.0f) * 2000.0f);
                        PlayerPrefs.SetInt("LevelStar" + Application.loadedLevel, 2);

                    }
                    else if (AIVehicleScript.PlayerBestTime < (PanelsMenu.gameFinish.Stars.Star1Time - PenaltyTime))
                    {

                        PanelsMenu.gameFinish.Stars.Star1.color = Color.white;

                        RacePrize = (int)((Application.loadedLevel / 5.0f) * 1500.0f);
                        PlayerPrefs.SetInt("LevelStar" + Application.loadedLevel, 1);
                    }
                    else
                    {
                        PlayerPrefs.SetInt("LevelStar" + Application.loadedLevel, 0);
                        RacePrize = (int)((Application.loadedLevel / 5.0f) * 1000.0f);
                    }

                    TotalPrize = (TotalDriftCoins + RacePrize);

                    PanelsMenu.gameFinish.RacePrize.text = "Track: " + RacePrize.ToString();
                    PanelsMenu.gameFinish.DriftPrize.text = "Drift Coins: " + TotalDriftCoins.ToString();
                    PanelsMenu.gameFinish.TotalPrize.text = "Total: " + TotalPrize.ToString();

                    PanelsMenu.gamePlay.Root.gameObject.SetActive(false);
                    PanelsMenu.gamePuased.Root.gameObject.SetActive(false);
                    PanelsMenu.gameFinish.Root.gameObject.SetActive(true);

                    PlayerPrefs.SetInt("GameScore", PlayerPrefs.GetInt("GameScore") + TotalPrize);

                    if (PlayerPrefs.GetInt("CurrentLevelUnlocked") <= Application.loadedLevel)
                        PlayerPrefs.SetInt("CurrentLevelUnlocked", Application.loadedLevel);

                }
            }
        }
    }
    public void DisableAudio(Toggle toggle)
    {
        if (toggle.isOn)
        {
            PlayerPrefs.SetInt("AudioActive", 0);
        }
        else
        {
            AudioListener.volume = 0;
            PlayerPrefs.SetInt("AudioActive", 1);
        }
    }


    public void DisableMusic(Toggle toggle)
    {
        if (toggle.isOn)
        {
            SoundsGame.Music.mute = false;
            PlayerPrefs.SetInt("MusicActive", 0);
        }
        else
        {
            SoundsGame.Music.mute = true;
            PlayerPrefs.SetInt("MusicActive", 1);
        }
    }
    public void PauseGame()
    {

        if (GameRest) return;

        GamePaused = true;
        AudioListener.pause = true;
        PanelsMenu.gamePuased.Root.gameObject.SetActive(true);
        PanelsMenu.gameStart.Root.SetActive(false);
        PanelsMenu.gamePlay.Root.SetActive(false);
        Time.timeScale = 0.0f;
    }


    public void ResumeGame()
    {

        if (GameRest) return;

        GamePaused = false;
        AudioListener.pause = false;
        AudioListener.volume = (PlayerPrefs.GetInt("AudioActive") == 0) ? 1.0f : 0.0f;
        PanelsMenu.gamePuased.Root.gameObject.SetActive(false);
        PanelsMenu.gameStart.Root.SetActive(true);
        PanelsMenu.gamePlay.Root.SetActive(true);
        Time.timeScale = 1.0f;
    }

    public void RestartGame()
    {
        if (GameRest) return;
        Time.timeScale = 1.0f;
        PanelsMenu.gameStart.FadeBackGround.SetBool("FadeOut", true);        
        GameRest = true;
    }

    public void MainMenu()
    {
        if (GameRest) return;

        Time.timeScale = 1.0f;
        PanelsMenu.gameStart.FadeBackGround.SetBool("FadeOut", true);      
        GameRest = true;
    }
    public void ShowCarUI()
    {
        gearst = AIControl.CurrentVehicle.currentGear;
        CurrentCarUI.SpeedText.text = ((int)AIControl.CurrentVehicle.speed).ToString();

        if (gearst > 0 && AIControl.CurrentVehicle.speed > 1)
        {
            CurrentCarUI.GearText.color = Color.green;
            CurrentCarUI.GearText.text = gearst.ToString();
        }
        else if (AIControl.CurrentVehicle.speed > 1)
        {
            CurrentCarUI.GearText.color = Color.red;
            CurrentCarUI.GearText.text = "R";
        }
        else
        {
            CurrentCarUI.GearText.color = Color.white;
            CurrentCarUI.GearText.text = "N";
        }

        thisAngle = (AIControl.CurrentVehicle.motorRPM / 20) - 175;
        thisAngle = Mathf.Clamp(thisAngle, -180, 90);

        CurrentCarUI.TachometerNeedle.rectTransform.rotation = Quaternion.Euler(0, 0, -thisAngle);
        CurrentCarUI.barShiftGUI.rectTransform.localScale = new Vector3(AIControl.CurrentVehicle.powerShift / 100.0f, 1, 1);

    }
    void StartingGameTimer()
    {

        if (GameStarted && startTimer == 0) { PanelsMenu.gameStart.StartTimeUI.gameObject.SetActive(false); return; }

        startTimer = Mathf.MoveTowards(startTimer, 0.0f, Time.deltaTime);

        if (startTimer == 0 && !GameStarted)
        {

            startTimer = 1.0f;
            PanelsMenu.gameStart.StartTimeUI.fontSize = 200;

            if (startCont < 0)
            {
                PanelsMenu.gameStart.StartTimeUI.text = "";
            }
            else if (startCont == 0)
            {
                GameStarted = true;

                PanelsMenu.gameStart.StartTimeUI.GetComponent<AudioSource>().clip = SoundsGame.CountStart;
                PanelsMenu.gameStart.StartTimeUI.GetComponent<AudioSource>().Play();
                PanelsMenu.gameStart.StartTimeUI.text = startCont.ToString("START");
            }
            else if (startCont > 0)
            {
                PanelsMenu.gameStart.StartTimeUI.GetComponent<AudioSource>().clip = SoundsGame.CountDown;
                PanelsMenu.gameStart.StartTimeUI.GetComponent<AudioSource>().Play();
                PanelsMenu.gameStart.StartTimeUI.text = startCont.ToString("F0");

            }
            startCont--;
        }
        else
        {
            PanelsMenu.gameStart.StartTimeUI.fontSize = (int)Mathf.Lerp(PanelsMenu.gameStart.StartTimeUI.fontSize, 1.0f, Time.deltaTime * 2.0f);
        }
    }

    string FormatSeconds(float elapsed)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsed);
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;
        int hundredths = timeSpan.Milliseconds / 10;
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, hundredths);
    }    
}