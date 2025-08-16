using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Основной контроллер краш игры
/// Управляет игровой логикой, UI и состоянием игры
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI betText;
    public TextMeshProUGUI gameStatusText;
    public TextMeshProUGUI historyText;
    public Button placeBetButton;
    public Button cashoutButton;
    public TMP_InputField betInput;
    
    [Header("Game Settings")]
    public float playerBalance = 1000f;
    public float currentBet = 10f;
    public float minBet = 0.01f;
    public float maxBet = 1000f;
    
    [Header("Game State")]
    public bool isGameRunning = false;
    public bool hasPlacedBet = false;
    public float gameTime = 0f;
    public float currentMultiplier = 1.0f;
    public float crashPoint = 1.01f;
    
    [Header("History")]
    public int maxHistoryItems = 10;
    private List<float> crashHistory = new List<float>();
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public Button testRTPButton;
    
    private void Start()
    {
        SetupUI();
        UpdateBalance();
        UpdateUI();
        
        // Начинаем первый раунд
        StartNewRound();
    }
    
    private void Update()
    {
        if (isGameRunning)
        {
            UpdateGame();
        }
    }
    
    /// <summary>
    /// Обновляет игровую логику каждый кадр
    /// </summary>
    private void UpdateGame()
    {
        gameTime += Time.deltaTime;
        currentMultiplier = MultiplierCalculator.CalculateMultiplier(gameTime);
        
        // Обновляем отображение мультипликатора
        if (multiplierText != null)
        {
            multiplierText.text = $"x{currentMultiplier:F2}";
            
            // Изменяем цвет в зависимости от значения
            if (currentMultiplier < 2.0f)
            {
                multiplierText.color = Color.green;
            }
            else if (currentMultiplier < 10.0f)
            {
                multiplierText.color = Color.yellow;
            }
            else
            {
                multiplierText.color = Color.red;
            }
        }
        
        // Обновляем статус игры
        if (gameStatusText != null)
        {
            if (hasPlacedBet)
            {
                gameStatusText.text = $"ИГРА АКТИВНА | Время: {gameTime:F1}с";
                gameStatusText.color = Color.green;
            }
            else
            {
                gameStatusText.text = "ОЖИДАНИЕ СТАВКИ";
                gameStatusText.color = Color.white;
            }
        }
        
        // Проверяем краш
        if (currentMultiplier >= crashPoint)
        {
            Crash();
        }
    }
    
    /// <summary>
    /// Размещает ставку
    /// </summary>
    public void PlaceBet()
    {
        if (isGameRunning || hasPlacedBet) return;
        
        if (currentBet < minBet || currentBet > maxBet)
        {
            Debug.Log($"Ставка должна быть от {minBet} до {maxBet}!");
            return;
        }
        
        if (currentBet > playerBalance)
        {
            Debug.Log("Недостаточно средств!");
            return;
        }
        
        playerBalance -= currentBet;
        hasPlacedBet = true;
        
        UpdateBalance();
        UpdateUI();
        
        Debug.Log($"Ставка размещена: {currentBet:F2}");
    }
    
    /// <summary>
    /// Забирает выигрыш
    /// </summary>
    public void Cashout()
    {
        if (!hasPlacedBet || !isGameRunning) return;
        
        float winAmount = currentBet * currentMultiplier;
        playerBalance += winAmount;
        
        Debug.Log($"Кешаут на x{currentMultiplier:F2}! Выигрыш: {winAmount:F2}");
        
        EndRound();
    }
    
    /// <summary>
    /// Обрабатывает краш
    /// </summary>
    private void Crash()
    {
        Debug.Log($"Краш на x{currentMultiplier:F2}!");
        
        // Добавляем в историю
        crashHistory.Add(crashPoint);
        if (crashHistory.Count > maxHistoryItems)
        {
            crashHistory.RemoveAt(0);
        }
        
        UpdateHistory();
        
        EndRound();
    }
    
    /// <summary>
    /// Завершает раунд
    /// </summary>
    private void EndRound()
    {
        isGameRunning = false;
        hasPlacedBet = false;
        gameTime = 0f;
        currentMultiplier = 1.0f;
        
        UpdateUI();
        
        // Начинаем новый раунд через 3 секунды
        Invoke(nameof(StartNewRound), 3f);
    }
    
    /// <summary>
    /// Начинает новый раунд
    /// </summary>
    private void StartNewRound()
    {
        crashPoint = CrashPointGenerator.GenerateCrashPoint();
        isGameRunning = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"Новый раунд! Краш будет на x{crashPoint:F2}");
        }
        
        UpdateUI();
    }
    
    /// <summary>
    /// Настраивает UI элементы
    /// </summary>
    private void SetupUI()
    {
        if (placeBetButton != null)
            placeBetButton.onClick.AddListener(PlaceBet);
        
        if (cashoutButton != null)
            cashoutButton.onClick.AddListener(Cashout);
        
        if (betInput != null)
        {
            betInput.text = currentBet.ToString();
            betInput.onValueChanged.AddListener(OnBetChanged);
        }
        
        if (testRTPButton != null)
            testRTPButton.onClick.AddListener(TestRTP);
    }
    
    /// <summary>
    /// Обрабатывает изменение ставки
    /// </summary>
    private void OnBetChanged(string value)
    {
        if (float.TryParse(value, out float bet))
        {
            currentBet = Mathf.Clamp(bet, minBet, maxBet);
            
            if (betText != null)
            {
                betText.text = $"Ставка: {currentBet:F2}";
            }
        }
    }
    
    /// <summary>
    /// Обновляет баланс
    /// </summary>
    private void UpdateBalance()
    {
        if (balanceText != null)
        {
            balanceText.text = $"Баланс: {playerBalance:F2}";
        }
    }
    
    /// <summary>
    /// Обновляет историю крашей
    /// </summary>
    private void UpdateHistory()
    {
        if (historyText != null)
        {
            string history = "История крашей:\n";
            for (int i = crashHistory.Count - 1; i >= 0; i--)
            {
                float crash = crashHistory[i];
                Color color = crash < 2.0f ? Color.red : Color.green;
                history += $"x{crash:F2} ";
            }
            
            historyText.text = history;
        }
    }
    
    /// <summary>
    /// Обновляет состояние UI
    /// </summary>
    private void UpdateUI()
    {
        if (placeBetButton != null)
            placeBetButton.interactable = !hasPlacedBet && !isGameRunning;
        
        if (cashoutButton != null)
            cashoutButton.interactable = hasPlacedBet && isGameRunning;
        
        if (betInput != null)
            betInput.interactable = !hasPlacedBet && !isGameRunning;
        
        if (multiplierText != null)
        {
            multiplierText.text = $"x{currentMultiplier:F2}";
            multiplierText.color = Color.white;
        }
        
        if (gameStatusText != null)
        {
            if (isGameRunning)
            {
                gameStatusText.text = hasPlacedBet ? "ИГРА АКТИВНА" : "ОЖИДАНИЕ СТАВКИ";
                gameStatusText.color = hasPlacedBet ? Color.green : Color.white;
            }
            else
            {
                gameStatusText.text = "НОВЫЙ РАУНД ЧЕРЕЗ 3С";
                gameStatusText.color = Color.yellow;
            }
        }
    }
    
    /// <summary>
    /// Тестирует RTP
    /// </summary>
    public void TestRTP()
    {
        Debug.Log("Начинаем тест RTP...");
        RTPValidator.QuickRTPTest();
    }
    
    /// <summary>
    /// Полный тест RTP
    /// </summary>
    public void FullRTPTest()
    {
        Debug.Log("Начинаем полный тест RTP...");
        RTPValidator.FullRTPTest();
    }
    
    /// <summary>
    /// Тестирует формулу мультипликатора
    /// </summary>
    public void TestMultiplierFormula()
    {
        MultiplierCalculator.TestMultiplierFormula();
    }
    
    /// <summary>
    /// Тестирует распределение крашей
    /// </summary>
    public void TestCrashDistribution()
    {
        CrashPointGenerator.TestCrashDistribution();
    }
    
    // Методы для кнопок быстрого изменения ставки
    public void IncreaseBet()
    {
        currentBet = Mathf.Min(currentBet * 2f, maxBet);
        if (betInput != null)
            betInput.text = currentBet.ToString();
    }
    
    public void DecreaseBet()
    {
        currentBet = Mathf.Max(currentBet / 2f, minBet);
        if (betInput != null)
            betInput.text = currentBet.ToString();
    }
    
    public void SetBetHalf()
    {
        currentBet = playerBalance / 2f;
        currentBet = Mathf.Clamp(currentBet, minBet, maxBet);
        if (betInput != null)
            betInput.text = currentBet.ToString();
    }
    
    public void SetBetMax()
    {
        currentBet = playerBalance;
        currentBet = Mathf.Clamp(currentBet, minBet, maxBet);
        if (betInput != null)
            betInput.text = currentBet.ToString();
    }
} 