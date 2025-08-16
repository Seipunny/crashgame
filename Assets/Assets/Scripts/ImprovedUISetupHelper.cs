using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Улучшенный UI Setup Helper с адаптивным дизайном и раундовой системой
/// </summary>
public class ImprovedUISetupHelper : MonoBehaviour
{
    [Header("Автоматическая настройка")]
    public bool autoSetupOnStart = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupImprovedUI();
        }
    }
    
    [ContextMenu("Настроить улучшенный UI")]
    public void SetupImprovedUI()
    {
        Debug.Log("🎨 НАСТРОЙКА УЛУЧШЕННОГО UI");
        Debug.Log("=".PadRight(50, '='));
        
        CreateUIElements();
        SetupImprovedScript();
        
        Debug.Log("✅ Улучшенный UI настроен!");
        Debug.Log("Теперь можно запускать игру с раундовой системой!");
    }
    
    private void CreateUIElements()
    {
        Debug.Log("📱 Создаем улучшенные UI элементы...");
        
        // Создаем Canvas если его нет
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("✅ Canvas создан");
        }
        
        // Создаем EventSystem если его нет
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✅ EventSystem создан");
        }
        
        // Создаем панели
        CreatePanels(canvas);
        
        // Создаем UI элементы
        CreateMultiplierText(canvas);
        CreateButtons(canvas);
        CreateInfoTexts(canvas);
        CreateCountdownText(canvas);
        
        Debug.Log("✅ Все улучшенные UI элементы созданы");
    }
    
    private void CreatePanels(Canvas canvas)
    {
        // Создаем панели для разных состояний игры
        CreatePanel(canvas, "GamePanel", "Игровая панель", new Vector2(0, 0), new Vector2(1, 1));
        CreatePanel(canvas, "HistoryPanel", "Панель истории", new Vector2(0.7f, 0.1f), new Vector2(0.9f, 0.9f));
        CreatePanel(canvas, "BettingPanel", "Панель ставок", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f));
        CreatePanel(canvas, "CountdownPanel", "Панель обратного отсчета", new Vector2(0.3f, 0.7f), new Vector2(0.7f, 0.9f));
        
        Debug.Log("✅ Панели созданы");
    }
    
    private void CreatePanel(Canvas canvas, string name, string displayName, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panelObj = new GameObject(name);
        panelObj.transform.SetParent(canvas.transform, false);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform rect = panelObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Добавляем текст с названием панели
        GameObject textObj = new GameObject("PanelText");
        textObj.transform.SetParent(panelObj.transform, false);
        
        TextMeshProUGUI panelText = textObj.AddComponent<TextMeshProUGUI>();
        panelText.text = displayName;
        panelText.fontSize = 16;
        panelText.color = Color.white;
        panelText.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.9f);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private void CreateMultiplierText(Canvas canvas)
    {
        GameObject multiplierObj = new GameObject("MultiplierText");
        multiplierObj.transform.SetParent(canvas.transform, false);
        
        TextMeshProUGUI multiplierText = multiplierObj.AddComponent<TextMeshProUGUI>();
        multiplierText.text = "x1.00";
        multiplierText.fontSize = 72;
        multiplierText.color = Color.white;
        multiplierText.alignment = TextAlignmentOptions.Center;
        
        RectTransform rect = multiplierObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(400, 100);
        
        Debug.Log("✅ MultiplierText создан");
    }
    
    private void CreateButtons(Canvas canvas)
    {
        // Place Bet Button
        CreateButton(canvas, "PlaceBetButton", "СТАВКА", new Vector2(-200, -200));
        
        // Cashout Button
        CreateButton(canvas, "CashoutButton", "КЕШАУТ", new Vector2(200, -200));
        
        // Increase Bet Button
        CreateButton(canvas, "IncreaseBetButton", "+", new Vector2(-100, -300));
        
        // Decrease Bet Button
        CreateButton(canvas, "DecreaseBetButton", "-", new Vector2(100, -300));
        
        // Cancel Bet Button
        CreateButton(canvas, "CancelBetButton", "ОТМЕНА", new Vector2(0, -300));
        
        Debug.Log("✅ Кнопки созданы");
    }
    
    private void CreateButton(Canvas canvas, string name, string text, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(canvas.transform, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Создаем текст кнопки
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Настраиваем позицию кнопки
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(120, 50);
    }
    
    private void CreateInfoTexts(Canvas canvas)
    {
        // Balance Text
        CreateInfoText(canvas, "BalanceText", "Баланс: 1000", new Vector2(-300, 200));
        
        // Bet Text
        CreateInfoText(canvas, "BetText", "Ставка: 10", new Vector2(300, 200));
        
        // Status Text
        CreateInfoText(canvas, "StatusText", "Разместите ставку", new Vector2(0, 150));
        
        // Round Info Text
        CreateInfoText(canvas, "RoundInfoText", "Раунд #1", new Vector2(0, 250));
        
        // History Text
        CreateInfoText(canvas, "HistoryText", "История:", new Vector2(0, -100));
        
        Debug.Log("✅ Информационные тексты созданы");
    }
    
    private void CreateInfoText(Canvas canvas, string name, string text, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(canvas.transform, false);
        
        TextMeshProUGUI infoText = textObj.AddComponent<TextMeshProUGUI>();
        infoText.text = text;
        infoText.fontSize = 18;
        infoText.color = Color.white;
        infoText.alignment = TextAlignmentOptions.Center;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(300, 50);
    }
    
    private void CreateCountdownText(Canvas canvas)
    {
        GameObject countdownObj = new GameObject("CountdownText");
        countdownObj.transform.SetParent(canvas.transform, false);
        
        TextMeshProUGUI countdownText = countdownObj.AddComponent<TextMeshProUGUI>();
        countdownText.text = "Время на ставку: 5.0s";
        countdownText.fontSize = 24;
        countdownText.color = Color.yellow;
        countdownText.alignment = TextAlignmentOptions.Center;
        
        RectTransform rect = countdownObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 100);
        rect.sizeDelta = new Vector2(400, 50);
        
        Debug.Log("✅ CountdownText создан");
    }
    
    private void SetupImprovedScript()
    {
        Debug.Log("🔧 Настраиваем улучшенный скрипт...");
        
        CrashGameImproved improved = FindObjectOfType<CrashGameImproved>();
        if (improved == null)
        {
            Debug.LogWarning("⚠️ CrashGameImproved не найден в сцене");
            return;
        }
        
        // Находим UI элементы и присваиваем их скрипту
        TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>();
        Button[] buttons = FindObjectsOfType<Button>();
        GameObject[] panels = FindObjectsOfType<GameObject>();
        
        foreach (TextMeshProUGUI text in texts)
        {
            switch (text.name)
            {
                case "MultiplierText":
                    improved.multiplierText = text;
                    break;
                case "BalanceText":
                    improved.balanceText = text;
                    break;
                case "BetText":
                    improved.betText = text;
                    break;
                case "StatusText":
                    improved.statusText = text;
                    break;
                case "HistoryText":
                    improved.historyText = text;
                    break;
                case "CountdownText":
                    improved.countdownText = text;
                    break;
                case "RoundInfoText":
                    improved.roundInfoText = text;
                    break;
            }
        }
        
        foreach (Button button in buttons)
        {
            switch (button.name)
            {
                case "PlaceBetButton":
                    improved.placeBetButton = button;
                    break;
                case "CashoutButton":
                    improved.cashoutButton = button;
                    break;
                case "IncreaseBetButton":
                    improved.increaseBetButton = button;
                    break;
                case "DecreaseBetButton":
                    improved.decreaseBetButton = button;
                    break;
                case "CancelBetButton":
                    improved.cancelBetButton = button;
                    break;
            }
        }
        
        foreach (GameObject panel in panels)
        {
            switch (panel.name)
            {
                case "GamePanel":
                    improved.gamePanel = panel;
                    break;
                case "HistoryPanel":
                    improved.historyPanel = panel;
                    break;
                case "BettingPanel":
                    improved.bettingPanel = panel;
                    break;
                case "CountdownPanel":
                    improved.countdownPanel = panel;
                    break;
            }
        }
        
        // Находим CanvasScaler
        CanvasScaler scaler = FindObjectOfType<CanvasScaler>();
        if (scaler != null)
        {
            improved.canvasScaler = scaler;
        }
        
        Debug.Log("✅ Улучшенный скрипт настроен");
    }
} 