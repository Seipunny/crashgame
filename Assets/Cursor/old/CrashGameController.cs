using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace CrashGame
{
    /// <summary>
    /// Основной контроллер краш-игры для Unity
    /// </summary>
    public class CrashGameController : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private float maxMultiplier = 1000f;
        [SerializeField] private float minBet = 0.01f;
        [SerializeField] private float maxBet = 1000f;
        [SerializeField] private float roundDuration = 120f; // Максимальная длительность раунда
        
        [Header("Auto Functions")]
        [SerializeField] private bool enableAutoBet = true;
        [SerializeField] private bool enableAutoCashout = true;
        [SerializeField] private bool enablePartialCashout = true;
        [SerializeField] private bool enableDoubleBet = true;
        
        [Header("Events")]
        public UnityEvent<float> OnMultiplierChanged;
        public UnityEvent<float> OnCrashPointGenerated;
        public UnityEvent OnGameStarted;
        public UnityEvent OnGameCrashed;
        public UnityEvent OnPlayerCashout;
        
        // Приватные переменные
        private float currentMultiplier = 1f;
        private float crashPoint = 1f;
        private float gameTime = 0f;
        private bool isGameRunning = false;
        private bool hasCrashed = false;
        
        // Игроки и их ставки
        private System.Collections.Generic.Dictionary<string, PlayerBet> activeBets = 
            new System.Collections.Generic.Dictionary<string, PlayerBet>();
        
        /// <summary>
        /// Начинает новый раунд игры
        /// </summary>
        public void StartNewRound()
        {
            if (isGameRunning)
            {
                Debug.LogWarning("Game is already running!");
                return;
            }
            
            // Генерируем точку краша
            crashPoint = CrashPointGenerator.GenerateCrashPoint();
            Debug.Log($"Crash point generated: {crashPoint:F2}x");
            
            // Сбрасываем состояние игры
            currentMultiplier = 1f;
            gameTime = 0f;
            isGameRunning = true;
            hasCrashed = false;
            activeBets.Clear();
            
            // Запускаем корутину обновления мультипликатора
            StartCoroutine(UpdateMultiplierCoroutine());
            
            // Вызываем событие начала игры
            OnGameStarted?.Invoke();
            OnCrashPointGenerated?.Invoke(crashPoint);
        }
        
        /// <summary>
        /// Корутина для обновления мультипликатора
        /// </summary>
        private IEnumerator UpdateMultiplierCoroutine()
        {
            while (isGameRunning && !hasCrashed && gameTime < roundDuration)
            {
                // Обновляем время
                gameTime += Time.deltaTime;
                
                // Рассчитываем новый мультипликатор
                float newMultiplier = MultiplierCalculator.CalculateMultiplier(gameTime);
                
                // Проверяем, не достигли ли мы точки краша
                if (newMultiplier >= crashPoint)
                {
                    CrashGame();
                    break;
                }
                
                // Обновляем мультипликатор
                currentMultiplier = newMultiplier;
                OnMultiplierChanged?.Invoke(currentMultiplier);
                
                // Проверяем автокешауты
                CheckAutoCashouts();
                
                yield return null;
            }
            
            // Если время истекло, крашим игру
            if (gameTime >= roundDuration && !hasCrashed)
            {
                CrashGame();
            }
        }
        
        /// <summary>
        /// Крашит игру
        /// </summary>
        private void CrashGame()
        {
            if (hasCrashed) return;
            
            hasCrashed = true;
            isGameRunning = false;
            
            Debug.Log($"Game crashed at {currentMultiplier:F2}x (target: {crashPoint:F2}x)");
            
            // Обрабатываем все активные ставки как проигрыш
            foreach (var bet in activeBets.Values)
            {
                ProcessBetLoss(bet);
            }
            
            activeBets.Clear();
            
            // Вызываем событие краша
            OnGameCrashed?.Invoke();
        }
        
        /// <summary>
        /// Размещает ставку игрока
        /// </summary>
        public bool PlaceBet(string playerId, float betAmount, AutoBetSettings autoBetSettings = null)
        {
            if (!isGameRunning || hasCrashed)
            {
                Debug.LogWarning("Cannot place bet - game is not running!");
                return false;
            }
            
            if (betAmount < minBet || betAmount > maxBet)
            {
                Debug.LogWarning($"Bet amount {betAmount} is outside allowed range [{minBet}, {maxBet}]");
                return false;
            }
            
            if (activeBets.ContainsKey(playerId))
            {
                Debug.LogWarning($"Player {playerId} already has an active bet!");
                return false;
            }
            
            // Создаем ставку игрока
            var playerBet = new PlayerBet
            {
                playerId = playerId,
                betAmount = betAmount,
                autoBetSettings = autoBetSettings,
                isActive = true
            };
            
            activeBets[playerId] = playerBet;
            
            Debug.Log($"Player {playerId} placed bet: {betAmount:F2}");
            return true;
        }
        
        /// <summary>
        /// Игрок забирает выигрыш
        /// </summary>
        public bool Cashout(string playerId, float cashoutMultiplier = 0f)
        {
            if (!isGameRunning || hasCrashed)
            {
                Debug.LogWarning("Cannot cashout - game is not running!");
                return false;
            }
            
            if (!activeBets.ContainsKey(playerId))
            {
                Debug.LogWarning($"Player {playerId} has no active bet!");
                return false;
            }
            
            var playerBet = activeBets[playerId];
            if (!playerBet.isActive)
            {
                Debug.LogWarning($"Player {playerId} bet is not active!");
                return false;
            }
            
            // Если не указан мультипликатор, используем текущий
            if (cashoutMultiplier <= 0f)
            {
                cashoutMultiplier = currentMultiplier;
            }
            
            // Проверяем, что мультипликатор не превышает точку краша
            if (cashoutMultiplier > crashPoint)
            {
                Debug.LogWarning($"Cashout multiplier {cashoutMultiplier:F2} exceeds crash point {crashPoint:F2}!");
                return false;
            }
            
            // Обрабатываем выигрыш
            ProcessBetWin(playerBet, cashoutMultiplier);
            
            // Удаляем ставку из активных
            activeBets.Remove(playerId);
            
            // Вызываем событие кешаута
            OnPlayerCashout?.Invoke();
            
            Debug.Log($"Player {playerId} cashed out at {cashoutMultiplier:F2}x");
            return true;
        }
        
        /// <summary>
        /// Частичный кешаут (50%)
        /// </summary>
        public bool PartialCashout(string playerId, float cashoutMultiplier = 0f)
        {
            if (!enablePartialCashout)
            {
                Debug.LogWarning("Partial cashout is disabled!");
                return false;
            }
            
            if (!activeBets.ContainsKey(playerId))
            {
                Debug.LogWarning($"Player {playerId} has no active bet!");
                return false;
            }
            
            var playerBet = activeBets[playerId];
            if (!playerBet.isActive)
            {
                Debug.LogWarning($"Player {playerId} bet is not active!");
                return false;
            }
            
            // Если не указан мультипликатор, используем текущий
            if (cashoutMultiplier <= 0f)
            {
                cashoutMultiplier = currentMultiplier;
            }
            
            // Проверяем, что мультипликатор не превышает точку краша
            if (cashoutMultiplier > crashPoint)
            {
                Debug.LogWarning($"Cashout multiplier {cashoutMultiplier:F2} exceeds crash point {crashPoint:F2}!");
                return false;
            }
            
            // Рассчитываем выигрыш за 50% ставки
            float halfBetAmount = playerBet.betAmount * 0.5f;
            float winAmount = halfBetAmount * cashoutMultiplier;
            
            // Обрабатываем выигрыш за половину ставки
            ProcessBetWin(playerBet, cashoutMultiplier, 0.5f);
            
            // Уменьшаем ставку игрока
            playerBet.betAmount *= 0.5f;
            
            Debug.Log($"Player {playerId} partial cashout at {cashoutMultiplier:F2}x, won {winAmount:F2}");
            return true;
        }
        
        /// <summary>
        /// Проверяет автокешауты
        /// </summary>
        private void CheckAutoCashouts()
        {
            if (!enableAutoCashout) return;
            
            var playersToCashout = new System.Collections.Generic.List<string>();
            
            foreach (var kvp in activeBets)
            {
                var playerBet = kvp.Value;
                if (playerBet.autoBetSettings != null && 
                    playerBet.autoBetSettings.autoCashoutEnabled &&
                    currentMultiplier >= playerBet.autoBetSettings.autoCashoutMultiplier)
                {
                    playersToCashout.Add(kvp.Key);
                }
            }
            
            // Выполняем автокешауты
            foreach (var playerId in playersToCashout)
            {
                Cashout(playerId);
            }
        }
        
        /// <summary>
        /// Обрабатывает выигрыш ставки
        /// </summary>
        private void ProcessBetWin(PlayerBet playerBet, float cashoutMultiplier, float betFraction = 1f)
        {
            float winAmount = playerBet.betAmount * betFraction * cashoutMultiplier;
            
            Debug.Log($"Player {playerBet.playerId} won {winAmount:F2} at {cashoutMultiplier:F2}x");
            
            // Здесь можно добавить логику для отправки выигрыша игроку
            // например, через API или локальную систему валюты
        }
        
        /// <summary>
        /// Обрабатывает проигрыш ставки
        /// </summary>
        private void ProcessBetLoss(PlayerBet playerBet)
        {
            Debug.Log($"Player {playerBet.playerId} lost {playerBet.betAmount:F2}");
            
            // Здесь можно добавить логику для обработки проигрыша
        }
        
        /// <summary>
        /// Получает текущий мультипликатор
        /// </summary>
        public float GetCurrentMultiplier()
        {
            return currentMultiplier;
        }
        
        /// <summary>
        /// Получает точку краша
        /// </summary>
        public float GetCrashPoint()
        {
            return crashPoint;
        }
        
        /// <summary>
        /// Проверяет, запущена ли игра
        /// </summary>
        public bool IsGameRunning()
        {
            return isGameRunning;
        }
        
        /// <summary>
        /// Проверяет, произошел ли краш
        /// </summary>
        public bool HasCrashed()
        {
            return hasCrashed;
        }
        
        /// <summary>
        /// Получает время игры
        /// </summary>
        public float GetGameTime()
        {
            return gameTime;
        }
        
        /// <summary>
        /// Получает количество активных ставок
        /// </summary>
        public int GetActiveBetsCount()
        {
            return activeBets.Count;
        }
        
        /// <summary>
        /// Тестирует математическую модель
        /// </summary>
        [ContextMenu("Test Mathematical Model")]
        public void TestMathematicalModel()
        {
            Debug.Log("=== TESTING MATHEMATICAL MODEL ===");
            
            // Тестируем формулу мультипликатора
            MultiplierValidator.TestMultiplierFormula();
            
            // Тестируем RTP
            var rtpResult = RTPValidator.ValidateRTP(100000);
            RTPValidator.PrintValidationReport(rtpResult);
        }
    }
    
    /// <summary>
    /// Ставка игрока
    /// </summary>
    [System.Serializable]
    public class PlayerBet
    {
        public string playerId;
        public float betAmount;
        public AutoBetSettings autoBetSettings;
        public bool isActive;
        public float betTime;
    }
    
    /// <summary>
    /// Настройки автобетов
    /// </summary>
    [System.Serializable]
    public class AutoBetSettings
    {
        public bool autoBetEnabled = false;
        public float autoBetAmount = 1f;
        public int autoBetRounds = 10;
        public AutoBetStrategy strategy = AutoBetStrategy.Martingale;
        
        public bool autoCashoutEnabled = false;
        public float autoCashoutMultiplier = 2f;
        
        public bool partialCashoutEnabled = false;
        public float partialCashoutMultiplier = 1.5f;
    }
    
    /// <summary>
    /// Стратегии автобетов
    /// </summary>
    public enum AutoBetStrategy
    {
        Martingale,     // Удвоение после проигрыша
        AntiMartingale, // Удвоение после выигрыша
        Fixed,          // Фиксированная сумма
        Percentage      // Процент от баланса
    }
} 