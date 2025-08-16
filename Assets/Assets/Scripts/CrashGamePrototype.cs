using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Прототип краш-игры с базовым UI
/// </summary>
public class CrashGamePrototype : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI betText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI historyText;
    
    [Header("Buttons")]
    public Button placeBetButton;
    public Button cashoutButton;
    public Button increaseBetButton;
    public Button decreaseBetButton;
    
    [Header("Game Settings")]
    public float playerBalance = 1000f;
    public float currentBet = 10f;
    public float minBet = 1f;
    public float maxBet = 100f;
    public float betStep = 5f;
    
    [Header("Game State")]
    public bool isGameRunning = false;
    public bool hasPlacedBet = false;
    public float currentMultiplier = 1.0f;
    public float crashPoint = 1.0f;
    public float gameTime = 0f;
    public float maxGameTime = 60f;
    
    [Header("History")]
    public string[] gameHistory = new string[10];
    public int historyIndex = 0;
    
    private Coroutine gameCoroutine;
    
    private void Start()
    {
        SetupUI();
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
        
        // Начальное состояние кнопок
        SetButtonStates(false);
    }
    
    private void Update()
    {
        if (isGameRunning)
        {
            UpdateGame();
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
    
    private void UpdateUI()
    {
        // Обновляем тексты
        if (multiplierText != null)
            multiplierText.text = $"x{currentMultiplier:F2}";
        
        if (balanceText != null)
            balanceText.text = $"Баланс: {playerBalance:F0}";
        
        if (betText != null)
            betText.text = $"Ставка: {currentBet:F0}";
        
        if (statusText != null)
        {
            if (isGameRunning)
                statusText.text = "Игра идет...";
            else if (hasPlacedBet)
                statusText.text = "Готов к игре";
            else
                statusText.text = "Разместите ставку";
        }
        
        // Обновляем историю
        UpdateHistoryUI();
    }
    
    private void UpdateHistoryUI()
    {
        if (historyText != null)
        {
            string history = "История:\n";
            for (int i = 0; i < gameHistory.Length; i++)
            {
                int index = (historyIndex - i + gameHistory.Length) % gameHistory.Length;
                if (!string.IsNullOrEmpty(gameHistory[index]))
                {
                    history += gameHistory[index] + "\n";
                }
            }
            historyText.text = history;
        }
    }
    
    public void PlaceBet()
    {
        if (isGameRunning || hasPlacedBet) return;
        
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
        
        // Запускаем игру после размещения ставки
        gameCoroutine = StartCoroutine(GameLoop());
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
        // Генерируем новую точку краша
        crashPoint = CrashPointGenerator.GenerateCrashPoint();
        
        // Сбрасываем состояние игры
        isGameRunning = false;
        hasPlacedBet = false;
        currentMultiplier = 1.0f;
        gameTime = 0f;
        
        SetButtonStates(false);
        UpdateUI();
        
        Debug.Log($"Новый раунд. Краш: x{crashPoint:F2}");
    }
    

    
    private IEnumerator GameLoop()
    {
        // Небольшая задержка перед началом
        yield return new WaitForSeconds(1f);
        
        // Запускаем игру
        isGameRunning = true;
        Debug.Log("Игра началась!");
        
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
        StartCoroutine(DelayedNewRound());
    }
    
    private IEnumerator DelayedNewRound()
    {
        yield return new WaitForSeconds(2f);
        StartNewRound();
    }
    
    private void SetButtonStates(bool gameActive)
    {
        if (placeBetButton != null)
            placeBetButton.interactable = !gameActive && !hasPlacedBet;
        
        if (cashoutButton != null)
            cashoutButton.interactable = gameActive && hasPlacedBet;
        
        if (increaseBetButton != null)
            increaseBetButton.interactable = !gameActive && !hasPlacedBet;
        
        if (decreaseBetButton != null)
            decreaseBetButton.interactable = !gameActive && !hasPlacedBet;
    }
    
    private void AddToHistory(string result)
    {
        gameHistory[historyIndex] = result;
        historyIndex = (historyIndex + 1) % gameHistory.Length;
    }
    
    // Методы для автоматического запуска игры (для тестирования)
    [ContextMenu("Авто-ставка и запуск")]
    public void AutoBetAndStart()
    {
        if (!hasPlacedBet)
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