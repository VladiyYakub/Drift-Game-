using UnityEngine;
using System;

public class FormatSecondsScript : MonoBehaviour
{
    public string FormatSeconds(float elapsed)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsed);
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;
        int hundredths = timeSpan.Milliseconds / 10;
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, hundredths);
    }
}
