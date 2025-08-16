using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Security.Cryptography; // Для HMAC
using System.Text; // Для Encoding
using System; // Для BitConverter

public class CrashGamePrototype : MonoBehaviour
{
    [Header("Game Settings")]
    public float playerBalance = 1000f;
    public float currentBet = 10f;
    public float minBet = 10f;
    public float maxBet = 100f;
    public float betStep = 5f;
    
    [Header("Game State")]
    public bool isGameRunning = false;
    public bool hasPlacedBet = false;
    public bool hasCashedOut = false;
    public bool isBettingPhase = false;
    public float currentMultiplier = 1f;
    public float gameTime = 0f;
    public float crashPoint = 1f;
    public float finalWinAmount = 0f;
    
    [Header("Round System")]
    public float bettingPhaseDuration = 5f;
    public float bettingPhaseTime = 0f;
    public int currentRound = 1;
    
    [Header("UI Elements")]
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI betText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI historyText;
    public TextMeshProUGUI finalWinText;
    public TextMeshProUGUI roundText;
    public Slider bettingProgressBar;
    
    [Header("Buttons")]
    public Button placeBetButton;
    public Button cashoutButton;
    public Button increaseBetButton;
    public Button decreaseBetButton;
    
    [Header("UI Colors")]
    public Color normalColor = Color.white;
    public Color winColor = Color.green;
    public Color loseColor = Color.red;
    public Color missedColor = Color.yellow;
    
    [Header("History")]
    public string[] gameHistory = new string[10];
    public int historyIndex = 0;
    
    [Header("Multiplier & RTP Settings")] // Новое: Параметры для роста и RTP
    public float k = 0.04140147f; // Подогнанный для контрольных точек
    public float n = 1.20439294f; // Подогнанный для контрольных точек
    public float epsilon = 0.04f; // House edge для RTP 96%
    public float minCrash = 1.01f;
    public float maxCrash = 1000f;
    
    private Coroutine gameCoroutine;
    private string serverSeed = "fake_server_seed"; // Fake для прототипа
    private string clientSeed = "fake_client_seed";
    private long nonce = 0;
    
    private void Start()
    {
        playerBalance = 1000f;
        currentBet = Mathf.Clamp(10f, minBet, maxBet);
        
        // Симуляция RTP для валидации математики
        SimulateRTP(10000); // 10k раундов, лог в консоль
        
        StartNewRound();
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
        
        // Улучшенная формула роста множителя с динамическим ускорением
        currentMultiplier = GetMultiplier(gameTime);
        
        // Проверяем краш
        if (currentMultiplier >= crashPoint)
        {
            Crash();
        }
        
        UpdateUI();
    }
    
    // Улучшенная: Формула роста множителя (fit под контрольные точки)
    private float GetMultiplier(float timeInSeconds)
    {
        if (timeInSeconds <= 0) return 1.0f;
        float expValue = k * Mathf.Pow(timeInSeconds, n);
        return Mathf.Clamp(Mathf.Exp(expValue), 1.0f, maxCrash);
    }
    
    // Новая: Текущая скорость роста (dm/dt) для управления динамикой
    public float GetGrowthRate(float timeInSeconds)
    {
        if (timeInSeconds <= 0) return 0f;
        float m = GetMultiplier(timeInSeconds);
        return m * k * n * Mathf.Pow(timeInSeconds, n - 1f);
    }
    
    // Новая: Ускорение (d²m/dt²) для продвинутой анимации/эффектов
    public float GetAcceleration(float timeInSeconds)
    {
        if (timeInSeconds <= 0) return 0f;
        float m = GetMultiplier(timeInSeconds);
        float term1 = k * n * Mathf.Pow(timeInSeconds, n - 1f);
        float term2 = k * n * (n - 1f) * Mathf.Pow(timeInSeconds, n - 2f);
        return m * (term1 * term1 + term2);
    }
    
    // Улучшенная: Генерация краш-пойнта с RTP 96% (Provably Fair симуляция)
    private float GenerateCrashPoint()
    {
        // Симулируем Provably Fair
        string combined = clientSeed + nonce.ToString() + currentRound.ToString();
        byte[] hmacBytes = ComputeHMAC(serverSeed, combined);
        ulong u52 = BitConverter.ToUInt64(hmacBytes, 0) >> 12; // Первые 52 бита
        double u = (double)u52 / Math.Pow(2, 52);
        
        // Формула для M с RTP 96%
        float m = (1f - epsilon) / (1f - (float)u);
        nonce++; // Инкремент для следующего раунда
        
        return Mathf.Clamp(m, minCrash, maxCrash);
    }
    
    // Новая: Вычисление HMAC для Provably Fair
    private byte[] ComputeHMAC(string key, string message)
    {
        using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        }
    }
    
    // Новая: Симуляция RTP с поведением игроков (валидация математики)
    private void SimulateRTP(int rounds)
    {
        float totalBets = 0f;
        float totalPayouts = 0f;
        int wins = 0;
        float betAmount = 100f;
        
        // Для распределения крашей (buckets как в docs)
        int[] buckets = new int[7]; // 1.01-2, 2.01-5, 5.01-10, 10.01-20, 20.01-50, 50.01-100, 100+
        float[] bucketEdges = {1.01f, 2f, 5f, 10f, 20f, 50f, 100f, float.MaxValue};
        
        for (int i = 0; i < rounds; i++)
        {
            float crash = GenerateCrashPoint();
            totalBets += betAmount;
            float target = SampleTarget(); // Random target
            if (crash >= target)
            {
                float payout = betAmount * target;
                totalPayouts += payout;
                wins++;
            }
            
            // Bucket count
            for (int b = 0; b < bucketEdges.Length - 1; b++)
            {
                if (crash > bucketEdges[b] && crash <= bucketEdges[b+1])
                {
                    buckets[b]++;
                    break;
                }
            }
        }
        
        float rtp = (totalPayouts / totalBets) * 100f;
        float winrate = (wins / (float)rounds) * 100f;
        
        Debug.Log($"RTP Simulation ({rounds} rounds): RTP = {rtp:F4}%, Winrate = {winrate:F2}%");
        Debug.Log("Crash Distribution:");
        string[] bucketNames = {"1.01-2.00", "2.01-5.00", "5.01-10.00", "10.01-20.00", "20.01-50.00", "50.01-100.00", "100.01+"};
        for (int b = 0; b < buckets.Length; b++)
        {
            float pct = (buckets[b] / (float)rounds) * 100f;
            Debug.Log($"{bucketNames[b]}: {pct:F2}%");
        }
    }
    
    // Сэмплинг target (поведение игроков, как в симуляциях)
    private float SampleTarget()
    {
        float r = UnityEngine.Random.Range(0f, 1f);
        if (r < 0.55f) return 1.10f + UnityEngine.Random.Range(0f, 1.40f);
        if (r < 0.85f) return 2.50f + UnityEngine.Random.Range(0f, 2.50f);
        if (r < 0.97f) return 5.00f + UnityEngine.Random.Range(0f, 5.00f);
        return 10.00f + UnityEngine.Random.Range(0f, 40.00f);
    }
    
    public void SetupUI()
    {
        Debug.Log("🔧 Настройка UI обработчиков...");
        
        // Настраиваем обработчики событий для кнопок
        if (placeBetButton != null)
        {
            placeBetButton.onClick.AddListener(PlaceBet);
            Debug.Log("✅ PlaceBetButton обработчик добавлен");
        }
        else
        {
            Debug.LogWarning("⚠️ PlaceBetButton не найден");
        }
        
        if (cashoutButton != null)
        {
            cashoutButton.onClick.AddListener(Cashout);
            Debug.Log("✅ CashoutButton обработчик добавлен");
        }
        else
        {
            Debug.LogWarning("⚠️ CashoutButton не найден");
        }
        
        if (increaseBetButton != null)
        {
            increaseBetButton.onClick.AddListener(IncreaseBet);
            Debug.Log("✅ IncreaseBetButton обработчик добавлен");
        }
        else
        {
            Debug.LogWarning("⚠️ IncreaseBetButton не найден");
        }
        
        if (decreaseBetButton != null)
        {
            decreaseBetButton.onClick.AddListener(DecreaseBet);
            Debug.Log("✅ DecreaseBetButton обработчик добавлен");
        }
        else
        {
            Debug.LogWarning("⚠️ DecreaseBetButton не найден");
        }
        
        UpdateUI();
    }
    
    public void PlaceBet()
    {
        if (isBettingPhase)
        {
            if (hasPlacedBet)
            {
                // Отменяем ставку
                hasPlacedBet = false;
                playerBalance += currentBet;
                statusText.text = "Ставка отменена";
                statusText.color = missedColor;
            }
            else
            {
                // Размещаем ставку
                if (playerBalance >= currentBet)
                {
                    playerBalance -= currentBet;
                    hasPlacedBet = true;
                    statusText.text = "Ставка размещена!";
                    statusText.color = winColor;
                }
                else
                {
                    statusText.text = "Недостаточно средств!";
                    statusText.color = loseColor;
                }
            }
            UpdateUI();
        }
        else if (!isGameRunning && !hasPlacedBet)
        {
            // Размещаем ставку вне фазы ставок (для тестов)
            if (playerBalance >= currentBet)
            {
                playerBalance -= currentBet;
                hasPlacedBet = true;
                statusText.text = "Ставка размещена!";
                statusText.color = winColor;
                StartGame();
            }
            else
            {
                statusText.text = "Недостаточно средств!";
                statusText.color = loseColor;
            }
            UpdateUI();
        }
    }
    
    public void Cashout()
    {
        if (isGameRunning && hasPlacedBet && !hasCashedOut)
        {
            hasCashedOut = true;
            finalWinAmount = currentBet * currentMultiplier;
            playerBalance += finalWinAmount;
            statusText.text = $"Кешаут на x{currentMultiplier:F2}! Выигрыш: {finalWinAmount:F0}";
            statusText.color = winColor;
            AddToHistory($"WIN x{currentMultiplier:F2}");
            UpdateUI();
        }
    }
    
    private void Crash()
    {
        isGameRunning = false;
        if (!hasCashedOut && hasPlacedBet)
        {
            statusText.text = $"КРАШ на x{crashPoint:F2}! Проигрыш: -{currentBet:F0}";
            statusText.color = loseColor;
            AddToHistory($"CRASH x{crashPoint:F2}");
            finalWinAmount = 0f;
        }
        else if (hasPlacedBet)
        {
            statusText.text += $"\nКРАШ на x{crashPoint:F2}";
        }
        else
        {
            statusText.text = $"КРАШ на x{crashPoint:F2}! Вы не ставили.";
            statusText.color = missedColor;
            AddToHistory($"MISSED x{crashPoint:F2}");
        }
        UpdateUI();
        StartCoroutine(WaitForNextRound());
    }
    
    private IEnumerator WaitForNextRound()
    {
        yield return new WaitForSeconds(3f);
        ResetRound();
        StartNewRound();
    }
    
    private void ResetRound()
    {
        hasPlacedBet = false;
        hasCashedOut = false;
        currentMultiplier = 1f;
        gameTime = 0f;
        finalWinAmount = 0f;
        isGameRunning = false;
        isBettingPhase = false;
    }
    
    public void StartNewRound()
    {
        currentRound++;
        crashPoint = GenerateCrashPoint();
        ResetRound();
        isBettingPhase = true;
        bettingPhaseTime = 0f;
        StartCoroutine(BettingPhase());
        UpdateUI();
    }
    
    private IEnumerator BettingPhase()
    {
        while (bettingPhaseTime < bettingPhaseDuration)
        {
            bettingPhaseTime += Time.deltaTime;
            UpdateUI();
            yield return null;
        }
        isBettingPhase = false;
        if (hasPlacedBet)
        {
            StartGame();
        }
        else
        {
            statusText.text = "Ставка не размещена! Раунд пропущен.";
            statusText.color = missedColor;
            StartCoroutine(WaitForNextRound());
        }
    }
    
    private void StartGame()
    {
        isGameRunning = true;
        statusText.text = "Игра началась!";
        statusText.color = normalColor;
        UpdateUI();
    }
    
    public void IncreaseBet()
    {
        if (isBettingPhase && !hasPlacedBet)
        {
            currentBet = Mathf.Clamp(currentBet + betStep, minBet, maxBet);
            UpdateUI();
        }
    }
    
    public void DecreaseBet()
    {
        if (isBettingPhase && !hasPlacedBet)
        {
            currentBet = Mathf.Clamp(currentBet - betStep, minBet, maxBet);
            UpdateUI();
        }
    }
    
    private void SetButtonStates(bool gameRunning)
    {
        if (placeBetButton != null)
            placeBetButton.interactable = !gameRunning && !hasPlacedBet;
        
        if (cashoutButton != null)
            cashoutButton.interactable = gameRunning && hasPlacedBet && !hasCashedOut;
        
        if (increaseBetButton != null)
            increaseBetButton.interactable = !gameRunning && !hasPlacedBet;
        
        if (decreaseBetButton != null)
            decreaseBetButton.interactable = !gameRunning && !hasPlacedBet;
    }
    
    private void SetBettingPhaseButtonStates()
    {
        if (placeBetButton != null)
            placeBetButton.interactable = true;
        
        if (cashoutButton != null)
            cashoutButton.interactable = false;
        
        if (increaseBetButton != null)
            increaseBetButton.interactable = !hasPlacedBet;
        
        if (decreaseBetButton != null)
            decreaseBetButton.interactable = !hasPlacedBet;
    }
    
    public void UpdateUI()
    {
        if (multiplierText != null)
        {
            multiplierText.text = $"x{currentMultiplier:F2}";
            if (isGameRunning)
                multiplierText.color = normalColor;
            else if (hasCashedOut)
                multiplierText.color = winColor;
            else
                multiplierText.color = loseColor;
        }
        
        if (balanceText != null)
            balanceText.text = $"Баланс: {playerBalance:F0}";
        
        if (betText != null)
            betText.text = $"Ставка: {currentBet:F0}";
        
        if (roundText != null)
            roundText.text = $"Раунд: {currentRound}";
        
        if (statusText != null && string.IsNullOrEmpty(statusText.text))
        {
            statusText.text = "Разместите ставку";
            statusText.color = normalColor;
        }
        
        // Обновляем историю
        UpdateHistoryUI();
        
        // Обновляем итоговый выигрыш
        UpdateFinalWinText();
        
        // Обновляем прогресс-бар фазы ставок
        UpdateBettingProgressBar();
        
        // Обновляем состояние кнопок в фазе ставок
        if (isBettingPhase)
        {
            SetBettingPhaseButtonStates();
            UpdateBetButtonText();
        }
        else
        {
            SetButtonStates(isGameRunning);
            UpdateBetButtonText();
        }
    }
    
    private void UpdateHistoryUI()
    {
        if (historyText == null) return;
        
        string historyDisplay = "История:\n";
        for (int i = 0; i < gameHistory.Length; i++)
        {
            if (!string.IsNullOrEmpty(gameHistory[i]))
            {
                historyDisplay += gameHistory[i] + "\n";
            }
        }
        
        historyText.text = historyDisplay;
    }
    
    private void UpdateFinalWinText()
    {
        if (finalWinText == null) return;
        
        if (hasCashedOut && finalWinAmount > 0)
        {
            finalWinText.text = $"🎉 Итоговый выигрыш: +{finalWinAmount:F0}";
            finalWinText.color = winColor;
        }
        else
        {
            finalWinText.text = "";
        }
    }
    
    private void UpdateBettingProgressBar()
    {
        if (bettingProgressBar == null) return;
        
        if (isBettingPhase)
        {
            bettingProgressBar.gameObject.SetActive(true);
            bettingProgressBar.value = bettingPhaseTime / bettingPhaseDuration;
        }
        else
        {
            bettingProgressBar.gameObject.SetActive(false);
        }
    }
    
    private void UpdateBetButtonText()
    {
        if (placeBetButton == null) return;
        
        if (hasCashedOut)
        {
            placeBetButton.GetComponentInChildren<TextMeshProUGUI>().text = "ЗАБЛОКИРОВАНО";
        }
        else if (isBettingPhase)
        {
            placeBetButton.GetComponentInChildren<TextMeshProUGUI>().text = hasPlacedBet ? "ОТМЕНИТЬ" : "СТАВКА";
        }
        else if (isGameRunning && !hasPlacedBet)
        {
            placeBetButton.GetComponentInChildren<TextMeshProUGUI>().text = "СТАВКА";
        }
        else
        {
            placeBetButton.GetComponentInChildren<TextMeshProUGUI>().text = "СТАВКА";
        }
    }
    
    private void AddToHistory(string result)
    {
        if (historyIndex >= gameHistory.Length)
        {
            historyIndex = 0;
        }
        
        gameHistory[historyIndex] = result;
        historyIndex++;
        
        UpdateHistoryUI();
    }
}