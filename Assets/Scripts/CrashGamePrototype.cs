using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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
    
    private Coroutine gameCoroutine;
    
    private void Start()
    {
        // Инициализируем игру
        playerBalance = 1000f;
        currentBet = Mathf.Clamp(10f, minBet, maxBet);
        
        // Запускаем систему раундов (UI создастся автоматически)
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
        
        // Рассчитываем текущий множитель
        currentMultiplier = Mathf.Pow(1.1f, gameTime);
        
        // Проверяем краш
        if (currentMultiplier >= crashPoint)
        {
            Crash();
        }
        
        UpdateUI();
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
        
        Debug.Log("✅ UI обработчики настроены");
    }
    
    public void PlaceBet()
    {
        // Если игра идет и уже был кешаут - блокируем
        if (isGameRunning && hasCashedOut)
        {
            return;
        }
        
        // Если в фазе ставок и ставка еще не размещена
        if (isBettingPhase && !hasPlacedBet)
        {
            hasPlacedBet = true;
            playerBalance -= currentBet;
            UpdateUI();
            return;
        }
        
        // Если в фазе ставок и ставка уже размещена - отменяем
        if (isBettingPhase && hasPlacedBet)
        {
            hasPlacedBet = false;
            playerBalance += currentBet;
            UpdateUI();
            return;
        }
        
        // Если игра идет и ставка не размещена - размещаем
        if (isGameRunning && !hasPlacedBet && !hasCashedOut)
        {
            hasPlacedBet = true;
            playerBalance -= currentBet;
            UpdateUI();
            return;
        }
    }
    
    public void Cashout()
    {
        Debug.Log($"🔘 Cashout вызван! isGameRunning={isGameRunning}, hasPlacedBet={hasPlacedBet}, hasCashedOut={hasCashedOut}");
        
        if (!isGameRunning || !hasPlacedBet || hasCashedOut) 
        {
            Debug.Log("❌ Cashout: условие не выполнено");
            return;
        }
        
        hasCashedOut = true;
        finalWinAmount = currentBet * currentMultiplier;
        playerBalance += finalWinAmount;
        
        Debug.Log($"✅ Кешаут! Выигрыш: {finalWinAmount:F2}");
        
        AddToHistory($"WIN x{currentMultiplier:F2}");
        
        UpdateUI();
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
        // Дополнительная проверка для гарантии
        if (currentBet < minBet) currentBet = minBet;
        
        UpdateUI();
    }
    
    private void StartNewRound()
    {
        // Генерируем новый краш-пойнт
        crashPoint = GenerateCrashPoint();
        
        // Сбрасываем состояние игры
        isGameRunning = false;
        hasPlacedBet = false;
        hasCashedOut = false;
        currentMultiplier = 1f;
        finalWinAmount = 0f;
        
        // Начинаем фазу ставок
        isBettingPhase = true;
        bettingPhaseTime = 0f;
        
        // Настраиваем состояние кнопок
        SetBettingPhaseButtonStates();
        
        // Запускаем фазу ставок
        StartCoroutine(BettingPhase());
    }
    
    private IEnumerator BettingPhase()
    {
        while (bettingPhaseTime < bettingPhaseDuration && isBettingPhase)
        {
            bettingPhaseTime += Time.deltaTime;
            UpdateUI();
            yield return null;
        }
        
        // Фаза ставок закончилась
        isBettingPhase = false;
        
        // Начинаем игровую фазу независимо от того, поставил ли игрок ставку
        StartGame();
    }
    
    private void StartGame()
    {
        // Запускаем игру
        isGameRunning = true;
        
        // Игра продолжается до краша или кешаута
        gameCoroutine = StartCoroutine(GameLoop());
    }
    
    private IEnumerator GameLoop()
    {
        gameTime = 0f;
        
        while (isGameRunning)
        {
            gameTime += Time.deltaTime;
            
            // Рассчитываем текущий множитель
            currentMultiplier = Mathf.Pow(1.1f, gameTime);
            
            // Проверяем краш
            if (currentMultiplier >= crashPoint)
            {
                Crash();
                break;
            }
            
            UpdateUI();
            yield return null;
        }
    }
    
    private void Crash()
    {
        if (!isGameRunning) return;
        
        // Добавляем в историю
        AddToHistory($"CRASH x{currentMultiplier:F2}");
        
        // Показываем результат
        if (hasPlacedBet && !hasCashedOut)
        {
            // Игрок проиграл
            if (statusText != null)
            {
                statusText.text = "Краш! Проигрыш";
                statusText.color = loseColor;
            }
        }
        else if (hasCashedOut)
        {
            // Игрок уже забрал выигрыш
            if (statusText != null)
            {
                statusText.text = "УПУЩЕНО!";
                statusText.color = missedColor;
            }
        }
        else
        {
            // Игрок не участвовал
            if (statusText != null)
            {
                statusText.text = $"Краш! x{currentMultiplier:F2} (игрок не участвовал)";
                statusText.color = normalColor;
            }
        }
        
        // Завершаем раунд
        EndRound();
    }
    
    private void EndRound()
    {
        isGameRunning = false;
        
        if (gameCoroutine != null)
        {
            StopCoroutine(gameCoroutine);
        }
        
        // Запускаем новый раунд через 2 секунды
        StartCoroutine(DelayedNewRound());
    }
    
    private IEnumerator DelayedNewRound()
    {
        yield return new WaitForSeconds(2f);
        currentRound++;
        StartNewRound();
    }
    
    private void SetBettingPhaseButtonStates()
    {
        if (placeBetButton != null)
        {
            placeBetButton.interactable = true;
        }
        
        if (increaseBetButton != null)
        {
            increaseBetButton.interactable = !hasPlacedBet;
        }
        
        if (decreaseBetButton != null)
        {
            decreaseBetButton.interactable = !hasPlacedBet;
        }
        
        if (cashoutButton != null)
        {
            cashoutButton.interactable = false;
        }
    }
    
    private void SetButtonStates(bool gameRunning)
    {
        if (placeBetButton != null)
        {
            // Блокируем кнопку ставки если уже был кешаут
            placeBetButton.interactable = !hasCashedOut;
        }
        
        if (cashoutButton != null)
        {
            bool shouldBeInteractable = gameRunning && hasPlacedBet && !hasCashedOut;
            cashoutButton.interactable = shouldBeInteractable;
            Debug.Log($"🔘 Кешаут кнопка: gameRunning={gameRunning}, hasPlacedBet={hasPlacedBet}, hasCashedOut={hasCashedOut}, interactable={shouldBeInteractable}");
        }
        
        if (increaseBetButton != null)
        {
            increaseBetButton.interactable = !gameRunning && !hasPlacedBet;
        }
        
        if (decreaseBetButton != null)
        {
            decreaseBetButton.interactable = !gameRunning && !hasPlacedBet;
        }
    }
    
    public void UpdateUI()
    {
        // Обновляем тексты
        if (multiplierText != null)
            multiplierText.text = $"x{currentMultiplier:F2}";
        
        if (balanceText != null)
            balanceText.text = $"💰 Баланс: {playerBalance:F0}";
        
        // Обновляем номер раунда
        if (roundText != null)
        {
            roundText.text = $"🎯 Раунд {currentRound}";
        }
        
        if (betText != null)
        {
            if (hasPlacedBet && isGameRunning)
            {
                float potentialWin = currentBet * currentMultiplier;
                betText.text = $"Ставка: {currentBet:F0} | Выигрыш: {potentialWin:F0}";
                betText.color = normalColor;
            }
            else if (hasCashedOut && isGameRunning)
            {
                betText.text = $"Ставка: {currentBet:F0} | Забрано: +{finalWinAmount:F0}";
                betText.color = winColor;
            }
            else
            {
                betText.text = $"Ставка: {currentBet:F0}";
                betText.color = normalColor;
            }
        }
        
        if (statusText != null)
        {
            if (isBettingPhase)
            {
                float timeLeft = bettingPhaseDuration - bettingPhaseTime;
                if (hasPlacedBet)
                {
                    statusText.text = $"✅ Ставка размещена! Осталось: {timeLeft:F1}с";
                    statusText.color = winColor;
                }
                else
                {
                    statusText.text = $"⏰ Фаза ставок! Осталось: {timeLeft:F1}с";
                    statusText.color = normalColor;
                }
            }
            else if (isGameRunning)
            {
                if (hasPlacedBet)
                {
                    statusText.text = "🎮 Игра идет...";
                    statusText.color = normalColor;
                }
                else if (hasCashedOut)
                {
                    statusText.text = $"Кешаут! +{finalWinAmount:F0}";
                    statusText.color = winColor;
                }
                else
                {
                    statusText.text = "👀 Игра продолжается...";
                    statusText.color = normalColor;
                }
            }
            else if (hasPlacedBet)
            {
                statusText.text = "Готов к игре";
                statusText.color = normalColor;
            }
            else
            {
                statusText.text = "Разместите ставку";
                statusText.color = normalColor;
            }
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
    
    private float GenerateCrashPoint()
    {
        // Простая реализация генерации краш-пойнта с RTP ~96%
        float random = Random.Range(0f, 1f);
        float crashPoint = 1f / (1f - random * 0.96f);
        return Mathf.Clamp(crashPoint, 1.01f, 1000f);
    }
} 