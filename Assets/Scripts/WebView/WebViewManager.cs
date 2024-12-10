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

        // Instantiate CanvasWebViewPrefab
        _webViewPrefab = CanvasWebViewPrefab.Instantiate();
        _webViewPrefab.transform.SetParent(transform, false);
        _webViewPrefab.Native2DModeEnabled = true;
       
        // Set size and position
        var rectTransform = _webViewPrefab.transform as RectTransform;
        rectTransform.sizeDelta = new Vector2(_width, _height);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        // Wait for WebView to initialize
        await _webViewPrefab.WaitUntilInitialized();

        // Inject universal gesture detection script
        if (LoadPinchDetectorScript(out string script))
        {
            _webViewPrefab.WebView.PageLoadScripts.Add(script);
        }

        // Set message handling
        _webViewPrefab.WebView.MessageEmitted += (sender, args) =>
        {
            messageHandler.HandleMessage(args.Value);
        };

        // Load initial URL
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

    public void LoadSuccessPage()
    {
        Debug.Log("Try to load success page");
        // _webViewPrefab.WebView.LoadUrl(System.IO.Path.Combine(Application.streamingAssetsPath, "payment_success.html"));
    }

    private void SendMessageToJavaScript()
    {
        string message = "{\"type\": \"greeting\", \"message\": \"Hello from C#!\"}";
        _webViewPrefab.WebView.PostMessage(message);
        Debug.Log("C# sent message: " + message);
    }

    private void OnDestroy()
    {
        _webViewPrefab?.Destroy();
    }

    public async void GoBack()
    {
        if (_webViewPrefab?.WebView != null && await _webViewPrefab.WebView.CanGoBack())
        {
            _webViewPrefab.WebView.GoBack();
        }
    }
}
