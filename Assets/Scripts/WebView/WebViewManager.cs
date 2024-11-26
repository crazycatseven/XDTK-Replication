using System.Threading.Tasks;
using UnityEngine;
using Vuplex.WebView;
using System;

public class WebViewManager : MonoBehaviour
{
    [SerializeField]
    private int _width = 1080;
    [SerializeField]
    private int _height = 2340;

    [SerializeField]
    private string _initialUrl = System.IO.Path.Combine(Application.streamingAssetsPath, "index.html");

    [SerializeField] private WebViewMessageHandler messageHandler;
    
    private CanvasWebViewPrefab _webViewPrefab;

    private const string PINCH_DETECTOR_PATH = "WebView/universalPinchDetector";

    private async void Start()
    {
        if (messageHandler == null)
        {
            Debug.LogError("WebViewMessageHandler 未找到");
        }

        // 实例化 CanvasWebViewPrefab
        _webViewPrefab = CanvasWebViewPrefab.Instantiate();
        _webViewPrefab.transform.SetParent(transform, false);
        _webViewPrefab.Native2DModeEnabled = true;
       
        // 设置大小和位置
        var rectTransform = _webViewPrefab.transform as RectTransform;
        rectTransform.sizeDelta = new Vector2(_width, _height);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchorMin = new Vector2(0.5f, 0.35f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.35f);

        // 等待 WebView 初始化完成
        await _webViewPrefab.WaitUntilInitialized();

        // 注入通用手势检测脚本
        if (LoadPinchDetectorScript(out string script))
        {
            _webViewPrefab.WebView.PageLoadScripts.Add(script);
        }

        // 设置消息处理
        _webViewPrefab.WebView.MessageEmitted += (sender, args) =>
        {
            messageHandler.HandleMessage(args.Value);
        };

        // 加载初始 URL
        _webViewPrefab.WebView.LoadUrl(_initialUrl);
    }

    private bool LoadPinchDetectorScript(out string script)
    {
        script = string.Empty;
        var textAsset = Resources.Load<TextAsset>(PINCH_DETECTOR_PATH);
        
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load universalPinchDetector.js from Resources/{PINCH_DETECTOR_PATH}");
            return false;
        }

        script = textAsset.text;
        return true;
    }

    private void SendMessageToJavaScript()
    {
        string message = "{\"type\": \"greeting\", \"message\": \"Hello from C#!\"}";
        _webViewPrefab.WebView.PostMessage(message);
        Debug.Log("C# 已发送消息: " + message);
    }

    private void OnDestroy()
    {
        _webViewPrefab?.Destroy();
    }
}
