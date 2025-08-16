using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Улучшенный помощник для настройки UI краш-игры
/// </summary>
public class EnhancedUISetupHelper : MonoBehaviour
{
    private void Start()
    {
        SetupEnhancedUI();
    }
    
    [ContextMenu("Настроить улучшенный UI")]
    public void SetupEnhancedUI()
    {
        
        // Создаем Canvas если его нет
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Создаем EventSystem если его нет
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
        
        // Создаем UI элементы
        CreateEnhancedUIElements();
        
        // Настраиваем скрипт прототипа
        SetupPrototypeScript();
        
    }
    
    private void CreateEnhancedUIElements()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        
        // Создаем основные UI элементы
        CreateEnhancedMultiplierText(canvas);
        CreateEnhancedButtons(canvas);
        CreateEnhancedInfoTexts(canvas);
        
    }
    
    private void CreateEnhancedMultiplierText(Canvas canvas)
    {
        GameObject multiplierObj = new GameObject("MultiplierText");
        multiplierObj.transform.SetParent(canvas.transform, false);
        
        TextMeshProUGUI multiplierText = multiplierObj.AddComponent<TextMeshProUGUI>();
        multiplierText.text = "x1.00";
        multiplierText.fontSize = 72;
        multiplierText.color = Color.white;
        multiplierText.alignment = TextAlignmentOptions.Center;
        multiplierText.fontStyle = FontStyles.Bold;
        
        // Добавляем тень
        Shadow shadow = multiplierObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(3, -3);
        
        RectTransform rect = multiplierObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(400, 100);
        
    }
    
    private void CreateEnhancedButtons(Canvas canvas)
    {
        // Place Bet Button - больше и красивее
        CreateEnhancedButton(canvas, "PlaceBetButton", "СТАВКА", new Vector2(-250, -200), new Color(0.2f, 0.8f, 0.2f));
        
        // Cashout Button - больше и красивее
        CreateEnhancedButton(canvas, "CashoutButton", "КЕШАУТ", new Vector2(250, -200), new Color(0.8f, 0.6f, 0.2f));
        
        // Increase Bet Button
        CreateEnhancedButton(canvas, "IncreaseBetButton", "+", new Vector2(-150, -300), new Color(0.3f, 0.3f, 0.3f));
        
        // Decrease Bet Button
        CreateEnhancedButton(canvas, "DecreaseBetButton", "-", new Vector2(150, -300), new Color(0.3f, 0.3f, 0.3f));
        

        
    }
    
    private void CreateEnhancedButton(Canvas canvas, string name, string text, Vector2 position, Color color)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(canvas.transform, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;
        
        // Добавляем тень
        Shadow shadow = buttonObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(2, -2);
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Создаем текст кнопки
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 28;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        
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
        buttonRect.sizeDelta = new Vector2(150, 60);
    }
    
    private void CreateEnhancedInfoTexts(Canvas canvas)
    {
        // Balance Text - с иконкой
        CreateEnhancedInfoText(canvas, "BalanceText", "💰 Баланс: 1000", new Vector2(-350, 250));
        
        // Round Text - номер раунда
        CreateEnhancedInfoText(canvas, "RoundText", "🎯 Раунд 1", new Vector2(0, 300));
        
        // Bet Text - с иконкой
        CreateEnhancedInfoText(canvas, "BetText", "🎯 Ставка: 10", new Vector2(350, 250));
        
        // Status Text - больше и заметнее
        CreateEnhancedInfoText(canvas, "StatusText", "Разместите ставку", new Vector2(0, 200));
        
        // Final Win Text - для отображения итогового выигрыша
        CreateFinalWinText(canvas, "FinalWinText", "", new Vector2(0, 50));
        
        // Betting Progress Bar - тонкий горизонтальный
        CreateBettingProgressBar(canvas, "BettingProgressBar", new Vector2(0, 150));
        
        // History Text - с рамкой
        CreateHistoryText(canvas, "HistoryText", "История:", new Vector2(0, -150));
        
    }
    
    private void CreateEnhancedInfoText(Canvas canvas, string name, string text, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(canvas.transform, false);
        
        TextMeshProUGUI infoText = textObj.AddComponent<TextMeshProUGUI>();
        infoText.text = text;
        infoText.fontSize = 22;
        infoText.color = Color.white;
        infoText.alignment = TextAlignmentOptions.Center;
        infoText.fontStyle = FontStyles.Bold;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(350, 60);
    }
    
    private void CreateFinalWinText(Canvas canvas, string name, string text, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(canvas.transform, false);
        
        TextMeshProUGUI finalWinText = textObj.AddComponent<TextMeshProUGUI>();
        finalWinText.text = text;
        finalWinText.fontSize = 22;
        finalWinText.color = Color.white;
        finalWinText.alignment = TextAlignmentOptions.Center;
        finalWinText.fontStyle = FontStyles.Bold;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(350, 60);
    }
    
    private void CreateBettingProgressBar(Canvas canvas, string name, Vector2 position)
    {
        // Создаем основной объект прогресс-бара
        GameObject progressObj = new GameObject(name);
        progressObj.transform.SetParent(canvas.transform, false);
        
        // Добавляем Slider компонент
        Slider progressBar = progressObj.AddComponent<Slider>();
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f;
        progressBar.transition = Selectable.Transition.None;
        
        // Настраиваем RectTransform основного объекта
        RectTransform rect = progressObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400, 8); // Тонкий горизонтальный
        
        // Создаем фон (Background)
        GameObject background = new GameObject("Background");
        background.transform.SetParent(progressObj.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Создаем область заполнения (Fill Area)
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(progressObj.transform, false);
        
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Создаем заполнение (Fill)
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.green;
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        // Привязываем компоненты к слайдеру
        progressBar.fillRect = fillRect;
        progressBar.targetGraphic = bgImage;
        
    }
    
    private void CreateHistoryText(Canvas canvas, string name, string text, Vector2 position)
    {
        // Создаем фон для истории
        GameObject bgObj = new GameObject(name + "Background");
        bgObj.transform.SetParent(canvas.transform, false);
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = position;
        bgRect.sizeDelta = new Vector2(400, 200);
        
        // Создаем текст истории
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(bgObj.transform, false);
        
        TextMeshProUGUI historyText = textObj.AddComponent<TextMeshProUGUI>();
        historyText.text = text;
        historyText.fontSize = 18;
        historyText.color = Color.white;
        historyText.alignment = TextAlignmentOptions.TopLeft;
        historyText.fontStyle = FontStyles.Normal;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
    }
    
    private void SetupPrototypeScript()
    {
        
        CrashGamePrototype prototype = FindObjectOfType<CrashGamePrototype>();
        if (prototype == null)
        {
            Debug.LogWarning("⚠️ CrashGamePrototype не найден в сцене");
            return;
        }
        
        // Находим и присваиваем UI элементы
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas не найден!");
            return;
        }
        
        // Ищем все TextMeshProUGUI в сцене
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        
        foreach (TextMeshProUGUI text in allTexts)
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
                case "FinalWinText":
                    prototype.finalWinText = text;
                    break;
                case "RoundText":
                    prototype.roundText = text;
                    break;
            }
        }
        
        // Ищем все Button в сцене
        Button[] allButtons = FindObjectsOfType<Button>();
        
        foreach (Button button in allButtons)
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
        
        // Ищем Slider
        Slider[] allSliders = FindObjectsOfType<Slider>();
        foreach (Slider slider in allSliders)
        {
            if (slider.name == "BettingProgressBar")
            {
                prototype.bettingProgressBar = slider;
                break;
            }
        }
        
        // Вызываем SetupUI для настройки обработчиков событий
        prototype.SetupUI();
        prototype.UpdateUI();
        
        // Принудительно обновляем историю
        if (prototype.historyText != null)
        {
            prototype.UpdateUI();
        }
        else
        {
            Debug.LogError("❌ historyText все еще не найден!");
        }
        
    }
} 