
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-1000)]
public class CryptEnhancedUISetupHelper : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSpawn()
    {
        if (FindObjectOfType<CryptEnhancedUISetupHelper>() == null)
        {
            var go = new GameObject("UI_AutoSetup");
            go.AddComponent<CryptEnhancedUISetupHelper>();
        }
    }

    private void Start()
    {
        SetupEnhancedUI();
    }

    [ContextMenu("Setup Enhanced UI")]
    public void SetupEnhancedUI()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var co = new GameObject("Canvas");
            canvas = co.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = co.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            co.AddComponent<GraphicRaycaster>();
        }
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        var logic = Object.FindObjectOfType<CryptCrashGamePrototype>();
        if (logic == null)
        {
            var go = new GameObject("CryptCrashGamePrototype");
            logic = go.AddComponent<CryptCrashGamePrototype>();
        }

        BuildUI(canvas, logic);
        logic.SetupUI();
        logic.UpdateUI();
    }

    private void BuildUI(Canvas canvas, CryptCrashGamePrototype p)
    {
        // Big multiplier (replace FontStyles.Black -> Bold)
        p.multiplierText = TMP(canvas.transform, "MultiplierText", "x1.00", 82, TextAlignmentOptions.Center, new Vector2(0, 160), new Vector2(900, 120), FontStyles.Bold);

        // Top row info
        p.balanceText = TMP(canvas.transform, "BalanceText", "💰 Баланс: 1000", 26, TextAlignmentOptions.Left, new Vector2(-700, 420), new Vector2(600,60), FontStyles.Bold);
        p.roundText   = TMP(canvas.transform, "RoundText", "🎯 Раунд 1", 26, TextAlignmentOptions.Center, new Vector2(0, 420), new Vector2(260,60), FontStyles.Bold);
        p.betText     = TMP(canvas.transform, "BetText", "Ставка: 10 | Цель: x2.00", 26, TextAlignmentOptions.Right, new Vector2(700, 420), new Vector2(600,60), FontStyles.Bold);

        // Status + progress
        p.statusText  = TMP(canvas.transform, "StatusText", "Разместите ставку", 30, TextAlignmentOptions.Center, new Vector2(0, 80), new Vector2(900,70), FontStyles.Bold);
        p.bettingProgressBar = Progress(canvas.transform, "BettingProgressBar", new Vector2(0, 120), new Vector2(700, 10));

        // Input row (bet, target, latency)
        var panel = Panel(canvas.transform, "InputPanel", new Vector2(0, 30), new Vector2(820, 70));
        Label(panel.transform, "BetLabel", "Ставка:", 22, new Vector2(-330, 0), new Vector2(120,50));
        p.betInput = Input(panel.transform, "BetInput", "10", new Vector2(-220, 0), new Vector2(150, 50));
        Label(panel.transform, "TargetLabel", "Цель (x):", 22, new Vector2(-20, 0), new Vector2(140,50));
        p.targetInput = Input(panel.transform, "TargetInput", "2.00", new Vector2(110, 0), new Vector2(150, 50));
        Label(panel.transform, "LatLabel", "RTT (мс):", 22, new Vector2(270, 0), new Vector2(140,50));
        p.latencyInput = Input(panel.transform, "LatencyInput", "80", new Vector2(380, 0), new Vector2(120, 50));

        // Buttons row
        p.placeBetButton   = Button(canvas.transform, "PlaceBetButton", "СТАВКА", new Vector2(-280, -220), new Vector2(220, 70), new Color(0.15f, 0.7f, 0.25f));
        p.cancelBetButton  = Button(canvas.transform, "CancelBetButton", "ОТМЕНИТЬ", new Vector2(-20, -220), new Vector2(220, 70), new Color(0.25f, 0.25f, 0.28f));
        p.manualCashoutButton = Button(canvas.transform, "ManualCashoutButton", "КЕШАУТ (запрос)", new Vector2(240, -220), new Vector2(260, 70), new Color(0.8f, 0.6f, 0.2f));

        // History
        p.historyText = History(canvas.transform, new Vector2(0, -40));

        // Commit/Reveal panel
        p.commitHashText = TMP(canvas.transform, "CommitHash", "Commit (hash): —", 18, TextAlignmentOptions.Left, new Vector2(-680, -360), new Vector2(1360, 40));
        p.revealText = TMP(canvas.transform, "RevealText", "Reveal: —", 16, TextAlignmentOptions.Left, new Vector2(-680, -420), new Vector2(1360, 120));
    }

    #region Primitives
    private GameObject Panel(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = new Color(0,0,0,0.35f);
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
        return go;
    }

    private TextMeshProUGUI TMP(Transform parent, string name, string text, int size, TextAlignmentOptions align, Vector2 pos, Vector2 dims, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.alignment = align; tmp.fontStyle = style; tmp.color = Color.white;
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = dims;
        return tmp;
    }

    private void Label(Transform parent, string name, string text, int size, Vector2 pos, Vector2 dims)
    {
        TMP(parent, name, text, size, TextAlignmentOptions.Center, pos, dims, FontStyles.Bold);
    }

    private Slider Progress(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var slider = go.AddComponent<Slider>(); slider.minValue = 0; slider.maxValue = 1; slider.value = 0; slider.transition = Selectable.Transition.None;
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
        var bg = new GameObject("Background"); bg.transform.SetParent(go.transform, false); var bgImg = bg.AddComponent<Image>(); bgImg.color = new Color(0.2f,0.2f,0.2f,0.85f);
        var bgRt = bg.GetComponent<RectTransform>(); bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        var fillArea = new GameObject("Fill Area"); fillArea.transform.SetParent(go.transform, false); var faRt = fillArea.AddComponent<RectTransform>(); faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one; faRt.offsetMin = Vector2.zero; faRt.offsetMax = Vector2.zero;
        var fill = new GameObject("Fill"); fill.transform.SetParent(fillArea.transform, false); var fillImg = fill.AddComponent<Image>(); fillImg.color = new Color(0.2f,0.8f,0.2f);
        var fillRt = fill.GetComponent<RectTransform>(); fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one; fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
        slider.fillRect = fillRt; slider.targetGraphic = bgImg;
        return slider;
    }

    private TMP_InputField Input(Transform parent, string name, string placeholder, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = new Color(0.15f,0.16f,0.2f,0.85f);
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;

        var txtGO = new GameObject("Text"); txtGO.transform.SetParent(go.transform, false);
        var txt = txtGO.AddComponent<TextMeshProUGUI>(); txt.fontSize = 22; txt.alignment = TextAlignmentOptions.Center; txt.color = Color.white;
        var txtRT = txtGO.GetComponent<RectTransform>(); txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one; txtRT.offsetMin = new Vector2(10,8); txtRT.offsetMax = new Vector2(-10,-8);

        var phGO = new GameObject("Placeholder"); phGO.transform.SetParent(go.transform, false);
        var ph = phGO.AddComponent<TextMeshProUGUI>(); ph.text = placeholder; ph.fontSize = 22; ph.alignment = TextAlignmentOptions.Center; ph.color = new Color(1,1,1,0.4f);
        var phRT = phGO.GetComponent<RectTransform>(); phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one; phRT.offsetMin = new Vector2(10,8); phRT.offsetMax = new Vector2(-10,-8);

        var input = go.AddComponent<TMP_InputField>(); input.textComponent = txt; input.placeholder = ph; input.contentType = TMP_InputField.ContentType.DecimalNumber; input.characterValidation = TMP_InputField.CharacterValidation.Decimal; input.text = placeholder;
        return input;
    }

    private Button Button(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = color;
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
        var btn = go.AddComponent<Button>();
        var tgo = new GameObject("Text"); tgo.transform.SetParent(go.transform, false);
        var tmp = tgo.AddComponent<TextMeshProUGUI>(); tmp.text = label; tmp.fontSize = 24; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        var trt = tgo.GetComponent<RectTransform>(); trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        go.AddComponent<Shadow>().effectColor = new Color(0,0,0,0.5f);
        return btn;
    }

    private TextMeshProUGUI History(Transform parent, Vector2 pos)
    {
        var bg = new GameObject("HistoryBG"); bg.transform.SetParent(parent, false);
        var bgImg = bg.AddComponent<Image>(); bgImg.color = new Color(0,0,0,0.6f);
        var bgRt = bg.GetComponent<RectTransform>(); bgRt.anchorMin = bgRt.anchorMax = new Vector2(0.5f,0.5f); bgRt.anchoredPosition = pos; bgRt.sizeDelta = new Vector2(900, 240);

        var go = new GameObject("HistoryText"); go.transform.SetParent(bg.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>(); tmp.text = "История:"; tmp.fontSize = 20; tmp.alignment = TextAlignmentOptions.TopLeft; tmp.color = Color.white;
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = new Vector2(12,12); rt.offsetMax = new Vector2(-12,-12);
        return tmp;
    }
    #endregion
}
