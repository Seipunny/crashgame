using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Улучшенная версия краш-игры с адаптивным UI и раундовой системой
/// </summary>
public class CrashGameImproved : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI betText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI historyText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI roundInfoText;
    
    [Header("Buttons")]
    public Button placeBetButton;
    public Button cashoutButton;
    public Button increaseBetButton;
    public Button decreaseBetButton;
    public Button cancelBetButton;
    
    [Header("UI Panels")]
    public GameObject gamePanel;
    public GameObject historyPanel;
    public GameObject bettingPanel;
    public GameObject countdownPanel;
    
    [Header("Game Settings")]
    public float playerBalance = 1000f;
    public float currentBet = 10f;
    public float minBet = 1f;
    public float maxBet = 100f;
    public float betStep = 5f;
    
    [Header("Round Settings")]
    public float bettingTime = 5f;
    public float roundDelay = 2f;
    public float maxGameTime = 60f;
    
    [Header("Game State")]
    public bool isGameRunning = false;
    public bool hasPlacedBet = false;
    public bool isBettingPhase = false;
    public float currentMultiplier = 1.0f;
    public float crashPoint = 1.0f;
    public float gameTime = 0f;
    public float bettingTimeLeft = 0f;
    public int currentRound = 0;
    
    [Header("History")]
    public List<string> gameHistory = new List<string>();
    public int maxHistoryItems = 10;
    
    [Header("Mobile UI")]
    public bool isMobile = false;
    public CanvasScaler canvasScaler;
    
    private Coroutine gameCoroutine;
    private Coroutine bettingCoroutine;
    private Coroutine countdownCoroutine;
    
    private void Start()
    {
        SetupUI();
        DetectMobile();
        UpdateUI();
        StartNewRound();
    }
    
    private void SetupUI()
    {
        // Настройка кнопок
        if (placeBetButton != null)
            placeBetButton.onClick.AddListener(PlaceBet);
        
        if (cashoutButton != null)
            cashoutButton.onClick.AddListener(Cashout);
        
        if (increaseBetButton != null)
            increaseBetButton.onClick.AddListener(IncreaseBet);
        
        if (decreaseBetButton != null)
            decreaseBetButton.onClick.AddListener(DecreaseBet);
        
        if (cancelBetButton != null)
            cancelBetButton.onClick.AddListener(CancelBet);
        
        // Начальное состояние кнопок
        SetButtonStates(false);
        SetPanelStates(false);
    }
    
    private void DetectMobile()
    {
        // Определяем мобильное устройство
        isMobile = Application.isMobilePlatform || Screen.width < 800;
        
        if (canvasScaler != null)
        {
            if (isMobile)
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(375, 812); // iPhone X
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;
            }
            else
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;
            }
        }
        
        Debug.Log($"📱 Устройство: {(isMobile ? "Мобильное" : "Десктоп")}");
    }
    
    private void Update()
    {
        if (isGameRunning)
        {
            UpdateGame();
        }
        
        if (isBettingPhase)
        {
            UpdateBettingPhase();
        }
    }
    
    private void UpdateGame()
    {
        gameTime += Time.deltaTime;
        currentMultiplier = MultiplierCalculator.CalculateMultiplier(gameTime);
        
        // Проверяем, не достигли ли точки краша
        if (currentMultiplier >= crashPoint)
        {
            Crash();
        }
        
        UpdateUI();
    }
    
    private void UpdateBettingPhase()
    {
        if (bettingTimeLeft > 0)
        {
            bettingTimeLeft -= Time.deltaTime;
            
            if (countdownText != null)
            {
                countdownText.text = $"Время на ставку: {bettingTimeLeft:F1}s";
            }
            
            if (bettingTimeLeft <= 0)
            {
                EndBettingPhase();
            }
        }
    }
    
    private void UpdateUI()
    {
        // Обновляем тексты
        if (multiplierText != null)
            multiplierText.text = $"x{currentMultiplier:F2}";
        
        if (balanceText != null)
            balanceText.text = $"Баланс: {playerBalance:F0}";
        
        if (betText != null)
            betText.text = $"Ставка: {currentBet:F0}";
        
        if (roundInfoText != null)
            roundInfoText.text = $"Раунд #{currentRound}";
        
        if (statusText != null)
        {
            if (isGameRunning)
                statusText.text = "Игра идет...";
            else if (isBettingPhase)
                statusText.text = "Разместите ставку";
            else if (hasPlacedBet)
                statusText.text = "Готов к игре";
            else
                statusText.text = "Ожидание...";
        }
        
        // Обновляем историю
        UpdateHistoryUI();
    }
    
    private void UpdateHistoryUI()
    {
        if (historyText != null)
        {
            string history = "История:\n";
            for (int i = gameHistory.Count - 1; i >= 0 && i >= gameHistory.Count - maxHistoryItems; i--)
            {
                history += gameHistory[i] + "\n";
            }
            historyText.text = history;
        }
    }
    
    public void PlaceBet()
    {
        if (!isBettingPhase || hasPlacedBet) return;
        
        if (currentBet > playerBalance)
        {
            Debug.Log("Недостаточно средств!");
            return;
        }
        
        playerBalance -= currentBet;
        hasPlacedBet = true;
        SetButtonStates(true);
        
        Debug.Log($"Ставка размещена: {currentBet}");
        UpdateUI();
    }
    
    public void CancelBet()
    {
        if (!isBettingPhase || !hasPlacedBet) return;
        
        playerBalance += currentBet;
        hasPlacedBet = false;
        SetButtonStates(false);
        
        Debug.Log("Ставка отменена");
        UpdateUI();
    }
    
    public void Cashout()
    {
        if (!isGameRunning || !hasPlacedBet) return;
        
        float winAmount = currentBet * currentMultiplier;
        playerBalance += winAmount;
        
        AddToHistory($"WIN x{currentMultiplier:F2}");
        
        Debug.Log($"Кешаут! Выигрыш: {winAmount:F2}");
        
        EndRound();
    }
    
    public void IncreaseBet()
    {
        if (isGameRunning || hasPlacedBet) return;
        
        currentBet = Mathf.Min(currentBet + betStep, maxBet);
        UpdateUI();
    }
    
    public void DecreaseBet()
    {
        if (isGameRunning || hasPlacedBet) return;
        
        currentBet = Mathf.Max(currentBet - betStep, minBet);
        UpdateUI();
    }
    
    private void StartNewRound()
    {
        currentRound++;
        
        // Генерируем новую точку краша
        crashPoint = CrashPointGenerator.GenerateCrashPoint();
        
        // Сбрасываем состояние игры
        isGameRunning = false;
        hasPlacedBet = false;
        currentMultiplier = 1.0f;
        gameTime = 0f;
        
        SetButtonStates(false);
        SetPanelStates(false);
        UpdateUI();
        
        Debug.Log($"Новый раунд #{currentRound}. Краш: x{crashPoint:F2}");
        
        // Запускаем фазу ставок
        StartBettingPhase();
    }
    
    private void StartBettingPhase()
    {
        isBettingPhase = true;
        bettingTimeLeft = bettingTime;
        SetPanelStates(true);
        
        Debug.Log($"Фаза ставок началась. Время: {bettingTime}s");
        
        // Запускаем корутину фазы ставок
        bettingCoroutine = StartCoroutine(BettingPhaseCoroutine());
    }
    
    private IEnumerator BettingPhaseCoroutine()
    {
        while (bettingTimeLeft > 0)
        {
            yield return null;
        }
        
        EndBettingPhase();
    }
    
    private void EndBettingPhase()
    {
        isBettingPhase = false;
        SetPanelStates(false);
        
        if (bettingCoroutine != null)
        {
            StopCoroutine(bettingCoroutine);
            bettingCoroutine = null;
        }
        
        Debug.Log("Фаза ставок завершена");
        
        // Если есть ставка, запускаем игру
        if (hasPlacedBet)
        {
            StartCoroutine(StartGameWithDelay());
        }
        else
        {
            // Если нет ставки, сразу краш
            AddToHistory($"SKIP x{crashPoint:F2}");
            StartCoroutine(StartNewRoundWithDelay());
        }
    }
    
    private IEnumerator StartGameWithDelay()
    {
        yield return new WaitForSeconds(roundDelay);
        
        // Запускаем игру
        isGameRunning = true;
        gameCoroutine = StartCoroutine(GameLoop());
        
        Debug.Log("Игра началась!");
    }
    
    private IEnumerator GameLoop()
    {
        // Игра продолжается до краша или кешаута
        while (isGameRunning && gameTime < maxGameTime)
        {
            yield return null;
        }
        
        // Если время истекло, но игра не закончилась
        if (isGameRunning)
        {
            Crash();
        }
    }
    
    private void Crash()
    {
        isGameRunning = false;
        
        if (hasPlacedBet)
        {
            AddToHistory($"CRASH x{crashPoint:F2}");
            Debug.Log($"Краш! x{crashPoint:F2}");
        }
        
        EndRound();
    }
    
    private void EndRound()
    {
        hasPlacedBet = false;
        SetButtonStates(false);
        
        // Останавливаем корутину игры
        if (gameCoroutine != null)
        {
            StopCoroutine(gameCoroutine);
            gameCoroutine = null;
        }
        
        // Небольшая задержка перед новым раундом
        StartCoroutine(StartNewRoundWithDelay());
    }
    
    private IEnumerator StartNewRoundWithDelay()
    {
        yield return new WaitForSeconds(roundDelay);
        StartNewRound();
    }
    
    private void SetButtonStates(bool gameActive)
    {
        if (placeBetButton != null)
            placeBetButton.interactable = isBettingPhase && !hasPlacedBet;
        
        if (cashoutButton != null)
            cashoutButton.interactable = gameActive && hasPlacedBet;
        
        if (increaseBetButton != null)
            increaseBetButton.interactable = isBettingPhase && !hasPlacedBet;
        
        if (decreaseBetButton != null)
            decreaseBetButton.interactable = isBettingPhase && !hasPlacedBet;
        
        if (cancelBetButton != null)
            cancelBetButton.interactable = isBettingPhase && hasPlacedBet;
    }
    
    private void SetPanelStates(bool bettingPhase)
    {
        if (bettingPanel != null)
            bettingPanel.SetActive(bettingPhase);
        
        if (countdownPanel != null)
            countdownPanel.SetActive(bettingPhase);
        
        if (gamePanel != null)
            gamePanel.SetActive(isGameRunning);
        
        if (historyPanel != null)
            historyPanel.SetActive(!isGameRunning && !bettingPhase);
    }
    
    private void AddToHistory(string result)
    {
        gameHistory.Add(result);
        
        // Ограничиваем количество элементов истории
        if (gameHistory.Count > maxHistoryItems)
        {
            gameHistory.RemoveAt(0);
        }
    }
    
    // Методы для автоматического тестирования
    [ContextMenu("Авто-ставка")]
    public void AutoBet()
    {
        if (isBettingPhase && !hasPlacedBet)
        {
            PlaceBet();
        }
    }
    
    [ContextMenu("Авто-кешаут на x2")]
    public void AutoCashoutAt2x()
    {
        if (isGameRunning && hasPlacedBet && currentMultiplier >= 2.0f)
        {
            Cashout();
        }
    }
} 