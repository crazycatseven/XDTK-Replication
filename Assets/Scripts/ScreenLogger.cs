using System.Collections.Generic;
using UnityEngine;

public class ScreenLogger : MonoBehaviour
{
    private List<string> logs = new List<string>(); // 存储所有日志信息
    private string logText = ""; // 显示在屏幕上的日志内容
    private GUIStyle guiStyle = new GUIStyle(); // GUI 样式
    private Vector2 scrollPosition;

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

        // 可选:自动滚动到最新日志
        scrollPosition = new Vector2(0, float.MaxValue);
    }

    private void OnGUI()
    {
        // 设置自动换行
        guiStyle.wordWrap = true;
        
        // 创建一个固定大小的显示区域
        float areaWidth = Screen.width * 0.95f;
        float areaHeight = Screen.height * 0.2f;
        
        // 开始一个滚动视图区域
        GUILayout.BeginArea(new Rect(10, 10, areaWidth, areaHeight));
        var scrollViewStyle = new GUIStyle(GUI.skin.scrollView);
        scrollViewStyle.padding = new RectOffset(10, 10, 10, 10); // 设置滚动条的内边距
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(areaWidth), GUILayout.Height(areaHeight));
        
        // 显示日志内容
        GUILayout.Label(logText, guiStyle);
        
        // 结束滚动视图
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
