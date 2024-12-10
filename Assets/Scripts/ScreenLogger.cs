using System.Collections.Generic;
using UnityEngine;

public class ScreenLogger : MonoBehaviour
{
    private List<string> logs = new List<string>(); // Store all log messages
    private string logText = ""; // Log content displayed on the screen
    private GUIStyle guiStyle = new GUIStyle(); // GUI style
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        // Subscribe to log events
        Application.logMessageReceived += HandleLog;
        guiStyle.fontSize = 20; // Set font size
        guiStyle.normal.textColor = Color.white; // Set font color
    }

    private void OnDisable()
    {
        // Unsubscribe from log events
        Application.logMessageReceived -= HandleLog;
    }

    // Callback function to handle logs
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string newLog = $"{type}: {logString}";

        if (type == LogType.Error || type == LogType.Exception || type == LogType.Warning)
        {
            newLog += $"\n{stackTrace}"; // Add stack trace for errors and exceptions
        }

        logs.Add(newLog); // Add log to the list
        logText = string.Join("\n", logs.ToArray()); // Update the log content displayed on the screen

        // Optional: Automatically scroll to the latest log
        scrollPosition = new Vector2(0, float.MaxValue);
    }

    private void OnGUI()
    {
        // Enable word wrap
        guiStyle.wordWrap = true;
        
        // Create a fixed-size display area
        float areaWidth = Screen.width * 0.95f;
        float areaHeight = Screen.height * 0.2f;
        
        // Start a scroll view area
        GUILayout.BeginArea(new Rect(10, 10, areaWidth, areaHeight));
        var scrollViewStyle = new GUIStyle(GUI.skin.scrollView);
        scrollViewStyle.padding = new RectOffset(10, 10, 10, 10); // Set padding for the scroll view
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(areaWidth), GUILayout.Height(areaHeight));
        
        // Display log content
        GUILayout.Label(logText, guiStyle);
        
        // End the scroll view
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
