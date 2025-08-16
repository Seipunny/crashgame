using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Вспомогательный скрипт для автоматического создания UI прототипа
/// </summary>
public class UISetupHelper : MonoBehaviour
{
    [Header("Автоматическая настройка")]
    public bool autoSetupOnStart = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupPrototypeUI();
        }
    }
    
    [ContextMenu("Настроить UI прототипа")]
    public void SetupPrototypeUI()
    {
        Debug.Log("🎨 НАСТРОЙКА UI ПРОТОТИПА");
        Debug.Log("=".PadRight(50, '='));
        
        CreateUIElements();
        SetupPrototypeScript();
        
        Debug.Log("✅ UI прототипа настроен!");
        Debug.Log("Теперь можно запускать игру.");
    }
    
    private void CreateUIElements()
    {
        Debug.Log("📱 Создаем UI элементы...");
        
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
        
        // Создаем UI элементы
        CreateMultiplierText(canvas);
        CreateButtons(canvas);
        CreateInfoTexts(canvas);
        
        Debug.Log("✅ Все UI элементы созданы");
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
    
    private void SetupPrototypeScript()
    {
        Debug.Log("🔧 Настраиваем скрипт прототипа...");
        
        CrashGamePrototype prototype = FindObjectOfType<CrashGamePrototype>();
        if (prototype == null)
        {
            Debug.LogWarning("⚠️ CrashGamePrototype не найден в сцене");
            return;
        }
        
        // Находим UI элементы и присваиваем их скрипту
        TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>();
        Button[] buttons = FindObjectsOfType<Button>();
        
        foreach (TextMeshProUGUI text in texts)
        {
            switch (text.name)
            {
                case "MultiplierText":
                    prototype.multiplierText = text;
                    break;
                case "BalanceText":
                    prototype.balanceText = text;
                    break;
                case "BetText":
                    prototype.betText = text;
                    break;
                case "StatusText":
                    prototype.statusText = text;
                    break;
                case "HistoryText":
                    prototype.historyText = text;
                    break;
            }
        }
        
        foreach (Button button in buttons)
        {
            switch (button.name)
            {
                case "PlaceBetButton":
                    prototype.placeBetButton = button;
                    break;
                case "CashoutButton":
                    prototype.cashoutButton = button;
                    break;
                case "IncreaseBetButton":
                    prototype.increaseBetButton = button;
                    break;
                case "DecreaseBetButton":
                    prototype.decreaseBetButton = button;
                    break;
            }
        }
        
        Debug.Log("✅ Скрипт прототипа настроен");
    }
} 