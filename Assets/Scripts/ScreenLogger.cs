using System.Collections.Generic;
using UnityEngine;

public class ScreenLogger : MonoBehaviour
{
    private List<string> logs = new List<string>(); // 存储所有日志信息
    private string logText = ""; // 显示在屏幕上的日志内容
    private GUIStyle guiStyle = new GUIStyle(); // GUI 样式

    private void OnEnable()
    {
        // 订阅日志事件
        Application.logMessageReceived += HandleLog;
        guiStyle.fontSize = 20; // 设置字体大小
        guiStyle.normal.textColor = Color.white; // 设置字体颜色
    }

    private void OnDisable()
    {
        // 取消订阅日志事件
        Application.logMessageReceived -= HandleLog;
    }

    // 处理日志的回调函数
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string newLog = $"{type}: {logString}";

        if (type == LogType.Error || type == LogType.Exception || type == LogType.Warning)
        {
            newLog += $"\n{stackTrace}"; // 错误和异常需要添加堆栈信息
        }

        logs.Add(newLog); // 将日志添加到列表
        logText = string.Join("\n", logs.ToArray()); // 更新屏幕显示的日志内容

        // 限制日志数量，避免屏幕显示过多信息
        if (logs.Count > 10)
        {
            logs.RemoveAt(0); // 移除最早的日志
        }
    }

    private void OnGUI()
    {
        // 在屏幕左上角显示日志
        GUI.Label(new Rect(10, 10, Screen.width, Screen.height), logText, guiStyle);
    }
}