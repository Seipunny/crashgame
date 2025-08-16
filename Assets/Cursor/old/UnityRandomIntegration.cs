using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;

namespace CrashGame
{
    /// <summary>
    /// Интеграция Unity Random с Provably Fair системой
    /// </summary>
    public static class UnityRandomIntegration
    {
        // Настройки для Provably Fair
        private static string serverSeed = "";
        private static string clientSeed = "";
        private static int nonce = 0;
        
        // Кэш для хешей
        private static readonly System.Collections.Generic.Dictionary<string, string> hashCache = 
            new System.Collections.Generic.Dictionary<string, string>();
        
        /// <summary>
        /// Инициализирует Provably Fair систему
        /// </summary>
        public static void InitializeProvablyFair(string serverSeed, string clientSeed = "")
        {
            UnityRandomIntegration.serverSeed = serverSeed;
            UnityRandomIntegration.clientSeed = clientSeed;
            UnityRandomIntegration.nonce = 0;
            hashCache.Clear();
            
            Debug.Log($"Provably Fair initialized - Server Seed: {serverSeed}, Client Seed: {clientSeed}");
        }
        
        /// <summary>
        /// Генерирует хеш для раунда
        /// </summary>
        public static string GenerateRoundHash(int roundNumber)
        {
            string combinedSeed = $"{serverSeed}-{clientSeed}-{roundNumber}-{nonce}";
            
            // Проверяем кэш
            if (hashCache.TryGetValue(combinedSeed, out string cachedHash))
            {
                return cachedHash;
            }
            
            // Генерируем SHA-256 хеш
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(combinedSeed);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                
                // Сохраняем в кэш
                hashCache[combinedSeed] = hash;
                
                return hash;
            }
        }
        
        /// <summary>
        /// Конвертирует хеш в случайное число от 0 до 1
        /// </summary>
        public static float HashToRandom(string hash)
        {
            // Берем первые 8 символов хеша
            string hexSubstring = hash.Substring(0, 8);
            
            // Конвертируем в число
            uint number = Convert.ToUInt32(hexSubstring, 16);
            
            // Нормализуем к диапазону [0, 1)
            return (float)number / uint.MaxValue;
        }
        
        /// <summary>
        /// Генерирует точку краша с Provably Fair
        /// </summary>
        public static float GenerateProvablyFairCrashPoint(int roundNumber)
        {
            // Генерируем хеш для раунда
            string roundHash = GenerateRoundHash(roundNumber);
            
            // Конвертируем в случайное число
            float randomValue = HashToRandom(roundHash);
            
            // Используем нашу математическую модель
            float crashMultiplier = CalculateCrashPointFromRandom(randomValue);
            
            Debug.Log($"Provably Fair Crash Point - Round: {roundNumber}, Hash: {roundHash}, Random: {randomValue:F6}, Crash: {crashMultiplier:F2}x");
            
            return Mathf.Clamp(crashMultiplier, 1.01f, 1000.0f);
        }
        
        /// <summary>
        /// Рассчитывает точку краша из случайного значения
        /// </summary>
        private static float CalculateCrashPointFromRandom(float randomValue)
        {
            const float lambda = 0.04f;
            float baseMultiplier = -Mathf.Log(1.0f - randomValue) / lambda;
            float correctionFactor = GetBehaviorCorrection(baseMultiplier);
            return baseMultiplier * correctionFactor;
        }
        
        /// <summary>
        /// Коррекция на основе поведения игроков
        /// </summary>
        private static float GetBehaviorCorrection(float multiplier)
        {
            if (multiplier < 1.50f) return 0.85f;
            if (multiplier < 3.00f) return 0.95f;
            if (multiplier < 10.0f) return 1.05f;
            return 1.15f;
        }
        
        /// <summary>
        /// Верифицирует результат раунда
        /// </summary>
        public static bool VerifyRoundResult(int roundNumber, float crashPoint, string providedHash = "")
        {
            string expectedHash = GenerateRoundHash(roundNumber);
            float expectedCrashPoint = GenerateProvablyFairCrashPoint(roundNumber);
            
            bool hashValid = string.IsNullOrEmpty(providedHash) || providedHash == expectedHash;
            bool crashValid = Mathf.Abs(crashPoint - expectedCrashPoint) < 0.01f;
            
            Debug.Log($"Round Verification - Round: {roundNumber}, Hash Valid: {hashValid}, Crash Valid: {crashValid}");
            
            return hashValid && crashValid;
        }
        
        /// <summary>
        /// Получает информацию для верификации
        /// </summary>
        public static ProvablyFairInfo GetVerificationInfo(int roundNumber)
        {
            string roundHash = GenerateRoundHash(roundNumber);
            float randomValue = HashToRandom(roundHash);
            float crashPoint = GenerateProvablyFairCrashPoint(roundNumber);
            
            return new ProvablyFairInfo
            {
                roundNumber = roundNumber,
                serverSeed = serverSeed,
                clientSeed = clientSeed,
                nonce = nonce,
                roundHash = roundHash,
                randomValue = randomValue,
                crashPoint = crashPoint
            };
        }
        
        /// <summary>
        /// Увеличивает nonce для следующего раунда
        /// </summary>
        public static void IncrementNonce()
        {
            nonce++;
        }
        
        /// <summary>
        /// Очищает кэш хешей
        /// </summary>
        public static void ClearHashCache()
        {
            hashCache.Clear();
        }
    }
    
    /// <summary>
    /// Информация для Provably Fair верификации
    /// </summary>
    [System.Serializable]
    public struct ProvablyFairInfo
    {
        public int roundNumber;
        public string serverSeed;
        public string clientSeed;
        public int nonce;
        public string roundHash;
        public float randomValue;
        public float crashPoint;
        
        public override string ToString()
        {
            return $"Round {roundNumber}: Hash={roundHash}, Random={randomValue:F6}, Crash={crashPoint:F2}x";
        }
    }
    
    /// <summary>
    /// Контроллер Unity Random с Provably Fair
    /// </summary>
    public class UnityRandomController : MonoBehaviour
    {
        [Header("Provably Fair Settings")]
        [SerializeField] private string serverSeed = "your-server-seed-here";
        [SerializeField] private string clientSeed = "";
        [SerializeField] private bool enableProvablyFair = true;
        [SerializeField] private bool logVerification = true;
        
        [Header("Random Settings")]
        [SerializeField] private int currentRound = 0;
        [SerializeField] private bool useUnityRandomAsFallback = true;
        
        // События
        public System.Action<ProvablyFairInfo> OnRoundGenerated;
        public System.Action<int, float> OnCrashPointGenerated;
        
        private void Start()
        {
            if (enableProvablyFair)
            {
                UnityRandomIntegration.InitializeProvablyFair(serverSeed, clientSeed);
                Debug.Log("Unity Random Controller initialized with Provably Fair");
            }
            else
            {
                Debug.Log("Unity Random Controller initialized without Provably Fair");
            }
        }
        
        /// <summary>
        /// Генерирует точку краша для текущего раунда
        /// </summary>
        public float GenerateCrashPoint()
        {
            float crashPoint;
            
            if (enableProvablyFair)
            {
                crashPoint = UnityRandomIntegration.GenerateProvablyFairCrashPoint(currentRound);
                
                if (logVerification)
                {
                    var verificationInfo = UnityRandomIntegration.GetVerificationInfo(currentRound);
                    Debug.Log($"Provably Fair Crash Point: {verificationInfo}");
                }
            }
            else
            {
                // Используем обычный Unity Random
                float randomValue = Random.Range(0.0001f, 0.9999f);
                crashPoint = CalculateCrashPointFromRandom(randomValue);
                crashPoint = Mathf.Clamp(crashPoint, 1.01f, 1000.0f);
            }
            
            OnCrashPointGenerated?.Invoke(currentRound, crashPoint);
            return crashPoint;
        }
        
        /// <summary>
        /// Рассчитывает точку краша из случайного значения
        /// </summary>
        private float CalculateCrashPointFromRandom(float randomValue)
        {
            const float lambda = 0.04f;
            float baseMultiplier = -Mathf.Log(1.0f - randomValue) / lambda;
            float correctionFactor = GetBehaviorCorrection(baseMultiplier);
            return baseMultiplier * correctionFactor;
        }
        
        /// <summary>
        /// Коррекция на основе поведения игроков
        /// </summary>
        private float GetBehaviorCorrection(float multiplier)
        {
            if (multiplier < 1.50f) return 0.85f;
            if (multiplier < 3.00f) return 0.95f;
            if (multiplier < 10.0f) return 1.05f;
            return 1.15f;
        }
        
        /// <summary>
        /// Начинает новый раунд
        /// </summary>
        public void StartNewRound()
        {
            currentRound++;
            
            if (enableProvablyFair)
            {
                var verificationInfo = UnityRandomIntegration.GetVerificationInfo(currentRound);
                OnRoundGenerated?.Invoke(verificationInfo);
                
                Debug.Log($"New round started: {verificationInfo}");
            }
            else
            {
                Debug.Log($"New round started: {currentRound}");
            }
        }
        
        /// <summary>
        /// Завершает раунд
        /// </summary>
        public void EndRound()
        {
            if (enableProvablyFair)
            {
                UnityRandomIntegration.IncrementNonce();
            }
        }
        
        /// <summary>
        /// Верифицирует результат раунда
        /// </summary>
        public bool VerifyRoundResult(float crashPoint)
        {
            if (!enableProvablyFair) return true;
            
            bool isValid = UnityRandomIntegration.VerifyRoundResult(currentRound, crashPoint);
            
            if (logVerification)
            {
                Debug.Log($"Round {currentRound} verification: {(isValid ? "VALID" : "INVALID")}");
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Получает информацию для верификации
        /// </summary>
        public ProvablyFairInfo GetVerificationInfo()
        {
            return UnityRandomIntegration.GetVerificationInfo(currentRound);
        }
        
        /// <summary>
        /// Устанавливает серверный сид
        /// </summary>
        public void SetServerSeed(string newServerSeed)
        {
            serverSeed = newServerSeed;
            if (enableProvablyFair)
            {
                UnityRandomIntegration.InitializeProvablyFair(serverSeed, clientSeed);
            }
        }
        
        /// <summary>
        /// Устанавливает клиентский сид
        /// </summary>
        public void SetClientSeed(string newClientSeed)
        {
            clientSeed = newClientSeed;
            if (enableProvablyFair)
            {
                UnityRandomIntegration.InitializeProvablyFair(serverSeed, clientSeed);
            }
        }
        
        /// <summary>
        /// Включает/выключает Provably Fair
        /// </summary>
        public void SetProvablyFair(bool enabled)
        {
            enableProvablyFair = enabled;
            if (enabled)
            {
                UnityRandomIntegration.InitializeProvablyFair(serverSeed, clientSeed);
            }
        }
        
        /// <summary>
        /// Тестирует Provably Fair систему
        /// </summary>
        [ContextMenu("Test Provably Fair")]
        public void TestProvablyFair()
        {
            Debug.Log("=== TESTING PROVABLY FAIR SYSTEM ===");
            
            // Тестируем несколько раундов
            for (int i = 1; i <= 5; i++)
            {
                currentRound = i;
                float crashPoint = GenerateCrashPoint();
                bool isValid = VerifyRoundResult(crashPoint);
                
                Debug.Log($"Round {i}: Crash={crashPoint:F2}x, Valid={isValid}");
            }
            
            Debug.Log("=== PROVABLY FAIR TEST COMPLETED ===");
        }
        
        /// <summary>
        /// Очищает кэш при завершении
        /// </summary>
        private void OnDestroy()
        {
            UnityRandomIntegration.ClearHashCache();
        }
    }
    
    /// <summary>
    /// Расширенный контроллер игры с Unity Random интеграцией
    /// </summary>
    public class UnityRandomCrashGameController : OptimizedCrashGameController
    {
        [Header("Unity Random Integration")]
        [SerializeField] private UnityRandomController randomController;
        [SerializeField] private bool useProvablyFair = true;
        
        protected override void Start()
        {
            base.Start();
            
            // Инициализируем Unity Random контроллер
            if (randomController == null)
            {
                randomController = gameObject.AddComponent<UnityRandomController>();
            }
            
            // Подписываемся на события
            randomController.OnCrashPointGenerated += OnCrashPointGenerated;
        }
        
        /// <summary>
        /// Начинает новый раунд с Unity Random
        /// </summary>
        public override void StartNewRound()
        {
            if (isGameRunning) return;
            
            // Начинаем новый раунд в Unity Random контроллере
            randomController.StartNewRound();
            
            // Генерируем точку краша через Unity Random
            crashPoint = randomController.GenerateCrashPoint();
            
            // Сбрасываем состояние
            currentMultiplier = 1f;
            gameTime = 0f;
            isGameRunning = true;
            hasCrashed = false;
            lastUpdateTime = 0f;
            
            // Очищаем коллекции
            ClearActiveBets();
            
            // Запускаем оптимизированную корутину
            StartCoroutine(OptimizedUpdateCoroutine());
            
            // Вызываем события
            OnGameStarted?.Invoke();
            OnCrashPointGenerated?.Invoke(crashPoint);
        }
        
        /// <summary>
        /// Крашит игру с верификацией
        /// </summary>
        protected override void CrashGame()
        {
            if (hasCrashed) return;
            
            hasCrashed = true;
            isGameRunning = false;
            
            // Верифицируем результат
            if (useProvablyFair)
            {
                bool isValid = randomController.VerifyRoundResult(crashPoint);
                if (!isValid)
                {
                    Debug.LogError($"CRITICAL: Round verification failed! Expected crash point does not match!");
                }
            }
            
            // Обрабатываем проигрыши
            foreach (var bet in activeBets.Values)
            {
                ProcessBetLoss(bet);
                
                if (useObjectPooling)
                {
                    betPool.Return(bet);
                }
            }
            
            activeBets.Clear();
            
            // Завершаем раунд в Unity Random контроллере
            randomController.EndRound();
            
            OnGameCrashed?.Invoke();
        }
        
        /// <summary>
        /// Обрабатывает сгенерированную точку краша
        /// </summary>
        private void OnCrashPointGenerated(int roundNumber, float crashPoint)
        {
            Debug.Log($"Crash point generated for round {roundNumber}: {crashPoint:F2}x");
        }
        
        /// <summary>
        /// Получает информацию для верификации
        /// </summary>
        public ProvablyFairInfo GetVerificationInfo()
        {
            return randomController.GetVerificationInfo();
        }
        
        /// <summary>
        /// Устанавливает Provably Fair режим
        /// </summary>
        public void SetProvablyFair(bool enabled)
        {
            useProvablyFair = enabled;
            randomController.SetProvablyFair(enabled);
        }
    }
} 