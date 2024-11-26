using System.Threading.Tasks;
using UnityEngine;
using Vuplex.WebView;
using System;

public class WebViewManager : MonoBehaviour
{
    // 设置要加载的初始 URL
    [SerializeField]
    private int _width = 1080;
    [SerializeField]
    private int _height = 2340;

    [SerializeField]
    private string _initialUrl = System.IO.Path.Combine(Application.streamingAssetsPath, "index.html");

    [SerializeField] private WebViewMessageHandler messageHandler;

    // CanvasWebViewPrefab 的引用
    private CanvasWebViewPrefab _webViewPrefab;



    // 初始化方法
    private async void Start()
    {

        if (messageHandler == null)
        {
            Debug.LogError("WebViewMessageHandler 未找到");
        }


        // 实例化 CanvasWebViewPrefab
        _webViewPrefab = CanvasWebViewPrefab.Instantiate();

        // 将其作为子对象添加到当前的 Canvas（即 WebCanvas）中
        _webViewPrefab.transform.SetParent(transform, false);

        _webViewPrefab.Native2DModeEnabled = true;
       
        // 设置大小和位置
        var rectTransform = _webViewPrefab.transform as RectTransform;
        rectTransform.sizeDelta = new Vector2(_width, _height); // 设置宽度和高度
        rectTransform.anchoredPosition = Vector2.zero; // 居中显示
        rectTransform.anchorMin = new Vector2(0.5f, 0.35f); // 设置锚点为中心
        rectTransform.anchorMax = new Vector2(0.5f, 0.35f); // 设置锚点为中心

        // 等待 WebView 初始化完成
        await _webViewPrefab.WaitUntilInitialized();

        // 初始化完成后再设置消息处理
        _webViewPrefab.WebView.MessageEmitted += (sender, args) =>
        {
            messageHandler.HandleMessage(args.Value);
        };

        // 加载初始 URL
        _webViewPrefab.WebView.LoadUrl(_initialUrl);
    }


    private void SendMessageToJavaScript()
    {
        // 向 JavaScript 发送消息
        string message = "{\"type\": \"greeting\", \"message\": \"Hello from C#!\"}";
        _webViewPrefab.WebView.PostMessage(message);
        Debug.Log("C# 已发送消息: " + message);
    }

    // 销毁时清理资源
    private void OnDestroy()
    {
        _webViewPrefab?.Destroy();
    }
}
