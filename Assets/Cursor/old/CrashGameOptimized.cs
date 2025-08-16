using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CrashGame
{
    /// <summary>
    /// Оптимизированный калькулятор мультипликатора с кэшированием
    /// </summary>
    public static class OptimizedMultiplierCalculator
    {
        // Кэш для часто используемых значений
        private static readonly Dictionary<float, float> multiplierCache = new Dictionary<float, float>();
        private static readonly Dictionary<float, float> timeCache = new Dictionary<float, float>();
        
        // Параметры формулы
        private const float K = 0.15f;
        private const float N = 1.8f;
        private const float CACHE_PRECISION = 0.1f; // Кэшируем с точностью до 0.1 секунды
        
        // Статистика кэша
        private static int cacheHits = 0;
        private static int cacheMisses = 0;
        
        /// <summary>
        /// Рассчитывает мультипликатор с кэшированием
        /// </summary>
        public static float CalculateMultiplier(float timeInSeconds)
        {
            if (timeInSeconds <= 0) return 1.0f;
            
            // Округляем время для кэширования
            float roundedTime = Mathf.Round(timeInSeconds / CACHE_PRECISION) * CACHE_PRECISION;
            
            // Проверяем кэш
            if (multiplierCache.TryGetValue(roundedTime, out float cachedMultiplier))
            {
                cacheHits++;
                return cachedMultiplier;
            }
            
            cacheMisses++;
            
            // Рассчитываем новое значение
            float multiplier = 1.0f + (Mathf.Exp(K * Mathf.Pow(timeInSeconds, N)) - 1.0f);
            multiplier = Mathf.Clamp(multiplier, 1.0f, 1000.0f);
            
            // Сохраняем в кэш
            multiplierCache[roundedTime] = multiplier;
            
            // Ограничиваем размер кэша
            if (multiplierCache.Count > 10000)
            {
                CleanupCache();
            }
            
            return multiplier;
        }
        
        /// <summary>
        /// Рассчитывает время для мультипликатора с кэшированием
        /// </summary>
        public static float CalculateTimeForMultiplier(float targetMultiplier)
        {
            if (targetMultiplier <= 1.0f) return 0f;
            if (targetMultiplier >= 1000.0f) return float.MaxValue;
            
            // Округляем мультипликатор для кэширования
            float roundedMultiplier = Mathf.Round(targetMultiplier * 10f) / 10f;
            
            // Проверяем кэш
            if (timeCache.TryGetValue(roundedMultiplier, out float cachedTime))
            {
                return cachedTime;
            }
            
            // Рассчитываем новое значение
            float timeInSeconds = Mathf.Pow(Mathf.Log(targetMultiplier) / K, 1f / N);
            
            // Сохраняем в кэш
            timeCache[roundedMultiplier] = timeInSeconds;
            
            // Ограничиваем размер кэша
            if (timeCache.Count > 10000)
            {
                CleanupTimeCache();
            }
            
            return timeInSeconds;
        }
        
        /// <summary>
        /// Очищает кэш мультипликаторов
        /// </summary>
        private static void CleanupCache()
        {
            var keysToRemove = multiplierCache.Keys.Take(5000).ToList();
            foreach (var key in keysToRemove)
            {
                multiplierCache.Remove(key);
            }
        }
        
        /// <summary>
        /// Очищает кэш времени
        /// </summary>
        private static void CleanupTimeCache()
        {
            var keysToRemove = timeCache.Keys.Take(5000).ToList();
            foreach (var key in keysToRemove)
            {
                timeCache.Remove(key);
            }
        }
        
        /// <summary>
        /// Получает статистику кэша
        /// </summary>
        public static string GetCacheStats()
        {
            float hitRate = cacheHits + cacheMisses > 0 ? (float)cacheHits / (cacheHits + cacheMisses) * 100f : 0f;
            return $"Cache Hits: {cacheHits}, Misses: {cacheMisses}, Hit Rate: {hitRate:F1}%, Multiplier Cache Size: {multiplierCache.Count}, Time Cache Size: {timeCache.Count}";
        }
        
        /// <summary>
        /// Очищает все кэши
        /// </summary>
        public static void ClearAllCaches()
        {
            multiplierCache.Clear();
            timeCache.Clear();
            cacheHits = 0;
            cacheMisses = 0;
        }
    }
    
    /// <summary>
    /// Оптимизированный генератор точек краша
    /// </summary>
    public static class OptimizedCrashPointGenerator
    {
        // Предварительно рассчитанные значения для ускорения
        private static readonly float[] cashoutPoints = { 1.10f, 1.20f, 1.50f, 2.00f, 3.00f, 5.00f, 10.0f, 20.0f };
        private static readonly float[] probabilities = { 0.25f, 0.30f, 0.20f, 0.15f, 0.06f, 0.03f, 0.01f, 0.00f };
        private static readonly float[] cumulativeProbabilities;
        
        // Кэш для случайных значений
        private static readonly Queue<float> randomValueCache = new Queue<float>();
        private const int CACHE_SIZE = 1000;
        
        static OptimizedCrashPointGenerator()
        {
            // Предварительно рассчитываем кумулятивные вероятности
            cumulativeProbabilities = new float[probabilities.Length];
            float cumulative = 0f;
            for (int i = 0; i < probabilities.Length; i++)
            {
                cumulative += probabilities[i];
                cumulativeProbabilities[i] = cumulative;
            }
            
            // Предзаполняем кэш случайных значений
            PrefillRandomCache();
        }
        
        /// <summary>
        /// Предзаполняет кэш случайными значениями
        /// </summary>
        private static void PrefillRandomCache()
        {
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                randomValueCache.Enqueue(Random.Range(0.0001f, 0.9999f));
            }
        }
        
        /// <summary>
        /// Генерирует точку краша с оптимизацией
        /// </summary>
        public static float GenerateCrashPoint()
        {
            // Получаем случайное значение из кэша или генерируем новое
            float randomValue;
            if (randomValueCache.Count > 0)
            {
                randomValue = randomValueCache.Dequeue();
            }
            else
            {
                randomValue = Random.Range(0.0001f, 0.9999f);
            }
            
            // Добавляем новое значение в кэш
            randomValueCache.Enqueue(Random.Range(0.0001f, 0.9999f));
            
            // Рассчитываем точку краша
            float crashMultiplier = CalculateCrashPointFromRandom(randomValue);
            return Mathf.Clamp(crashMultiplier, 1.01f, 1000.0f);
        }
        
        /// <summary>
        /// Оптимизированный расчет точки краша
        /// </summary>
        private static float CalculateCrashPointFromRandom(float randomValue)
        {
            const float lambda = 0.04f;
            float baseMultiplier = -Mathf.Log(1.0f - randomValue) / lambda;
            float correctionFactor = GetBehaviorCorrection(baseMultiplier);
            return baseMultiplier * correctionFactor;
        }
        
        /// <summary>
        /// Оптимизированная коррекция поведения игроков
        /// </summary>
        private static float GetBehaviorCorrection(float multiplier)
        {
            // Используем switch для быстрого определения диапазона
            if (multiplier < 1.50f) return 0.85f;
            if (multiplier < 3.00f) return 0.95f;
            if (multiplier < 10.0f) return 1.05f;
            return 1.15f;
        }
        
        /// <summary>
        /// Оптимизированная симуляция поведения игрока
        /// </summary>
        public static float SimulatePlayerBehavior(float crashPoint)
        {
            float random = Random.Range(0f, 1f);
            
            // Используем бинарный поиск для быстрого нахождения индекса
            int index = BinarySearchProbability(random);
            
            if (index >= 0 && index < cashoutPoints.Length)
            {
                return Mathf.Min(cashoutPoints[index], crashPoint);
            }
            
            return crashPoint;
        }
        
        /// <summary>
        /// Бинарный поиск для нахождения индекса вероятности
        /// </summary>
        private static int BinarySearchProbability(float target)
        {
            int left = 0;
            int right = cumulativeProbabilities.Length - 1;
            
            while (left <= right)
            {
                int mid = (left + right) / 2;
                
                if (cumulativeProbabilities[mid] >= target)
                {
                    if (mid == 0 || cumulativeProbabilities[mid - 1] < target)
                    {
                        return mid;
                    }
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }
            
            return -1;
        }
    }
    
    /// <summary>
    /// Object Pool для оптимизации памяти
    /// </summary>
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> pool;
        private readonly int maxSize;
        
        public ObjectPool(int initialSize = 10, int maxSize = 100)
        {
            this.maxSize = maxSize;
            pool = new Stack<T>(initialSize);
            
            // Предзаполняем пул
            for (int i = 0; i < initialSize; i++)
            {
                pool.Push(new T());
            }
        }
        
        /// <summary>
        /// Получает объект из пула
        /// </summary>
        public T Get()
        {
            return pool.Count > 0 ? pool.Pop() : new T();
        }
        
        /// <summary>
        /// Возвращает объект в пул
        /// </summary>
        public void Return(T item)
        {
            if (pool.Count < maxSize)
            {
                pool.Push(item);
            }
        }
        
        /// <summary>
        /// Получает количество объектов в пуле
        /// </summary>
        public int Count => pool.Count;
    }
    
    /// <summary>
    /// Оптимизированный контроллер игры с Object Pooling
    /// </summary>
    public class OptimizedCrashGameController : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private int maxActiveBets = 1000;
        [SerializeField] private float updateInterval = 0.016f; // 60 FPS
        [SerializeField] private bool useObjectPooling = true;
        
        // Object Pool для ставок игроков
        private ObjectPool<PlayerBet> betPool;
        
        // Оптимизированные коллекции
        private Dictionary<string, PlayerBet> activeBets;
        private List<string> playersToCashout;
        
        // Кэшированные значения
        private float lastUpdateTime;
        private float currentMultiplier = 1f;
        private float crashPoint = 1f;
        private float gameTime = 0f;
        private bool isGameRunning = false;
        private bool hasCrashed = false;
        
        // События
        public System.Action<float> OnMultiplierChanged;
        public System.Action<float> OnCrashPointGenerated;
        public System.Action OnGameStarted;
        public System.Action OnGameCrashed;
        public System.Action OnPlayerCashout;
        
        private void Awake()
        {
            // Инициализируем Object Pool
            if (useObjectPooling)
            {
                betPool = new ObjectPool<PlayerBet>(100, maxActiveBets);
            }
            
            // Инициализируем коллекции
            activeBets = new Dictionary<string, PlayerBet>(maxActiveBets);
            playersToCashout = new List<string>(maxActiveBets);
        }
        
        /// <summary>
        /// Начинает новый раунд с оптимизацией
        /// </summary>
        public void StartNewRound()
        {
            if (isGameRunning) return;
            
            // Генерируем точку краша
            crashPoint = OptimizedCrashPointGenerator.GenerateCrashPoint();
            
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
        /// Оптимизированная корутина обновления
        /// </summary>
        private System.Collections.IEnumerator OptimizedUpdateCoroutine()
        {
            while (isGameRunning && !hasCrashed)
            {
                // Обновляем только с заданным интервалом
                if (Time.time - lastUpdateTime >= updateInterval)
                {
                    UpdateGame();
                    lastUpdateTime = Time.time;
                }
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Оптимизированное обновление игры
        /// </summary>
        private void UpdateGame()
        {
            gameTime += updateInterval;
            
            // Используем кэшированный калькулятор
            float newMultiplier = OptimizedMultiplierCalculator.CalculateMultiplier(gameTime);
            
            if (newMultiplier >= crashPoint)
            {
                CrashGame();
                return;
            }
            
            currentMultiplier = newMultiplier;
            OnMultiplierChanged?.Invoke(currentMultiplier);
            
            // Проверяем автокешауты
            CheckAutoCashouts();
        }
        
        /// <summary>
        /// Оптимизированная проверка автокешаутов
        /// </summary>
        private void CheckAutoCashouts()
        {
            if (activeBets.Count == 0) return;
            
            playersToCashout.Clear();
            
            foreach (var kvp in activeBets)
            {
                var playerBet = kvp.Value;
                if (playerBet.autoBetSettings?.autoCashoutEnabled == true &&
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
        /// Размещает ставку с оптимизацией
        /// </summary>
        public bool PlaceBet(string playerId, float betAmount, AutoBetSettings autoBetSettings = null)
        {
            if (!isGameRunning || hasCrashed || activeBets.Count >= maxActiveBets)
                return false;
            
            // Получаем объект из пула или создаем новый
            PlayerBet playerBet = useObjectPooling ? betPool.Get() : new PlayerBet();
            
            // Инициализируем данные
            playerBet.playerId = playerId;
            playerBet.betAmount = betAmount;
            playerBet.autoBetSettings = autoBetSettings;
            playerBet.isActive = true;
            playerBet.betTime = Time.time;
            
            activeBets[playerId] = playerBet;
            return true;
        }
        
        /// <summary>
        /// Кешаут с оптимизацией
        /// </summary>
        public bool Cashout(string playerId, float cashoutMultiplier = 0f)
        {
            if (!activeBets.TryGetValue(playerId, out PlayerBet playerBet))
                return false;
            
            if (cashoutMultiplier <= 0f)
                cashoutMultiplier = currentMultiplier;
            
            if (cashoutMultiplier > crashPoint)
                return false;
            
            // Обрабатываем выигрыш
            ProcessBetWin(playerBet, cashoutMultiplier);
            
            // Возвращаем объект в пул
            if (useObjectPooling)
            {
                betPool.Return(playerBet);
            }
            
            activeBets.Remove(playerId);
            OnPlayerCashout?.Invoke();
            
            return true;
        }
        
        /// <summary>
        /// Очищает активные ставки
        /// </summary>
        private void ClearActiveBets()
        {
            if (useObjectPooling)
            {
                foreach (var bet in activeBets.Values)
                {
                    betPool.Return(bet);
                }
            }
            
            activeBets.Clear();
        }
        
        /// <summary>
        /// Крашит игру
        /// </summary>
        private void CrashGame()
        {
            if (hasCrashed) return;
            
            hasCrashed = true;
            isGameRunning = false;
            
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
            OnGameCrashed?.Invoke();
        }
        
        /// <summary>
        /// Обрабатывает выигрыш
        /// </summary>
        private void ProcessBetWin(PlayerBet playerBet, float cashoutMultiplier, float betFraction = 1f)
        {
            float winAmount = playerBet.betAmount * betFraction * cashoutMultiplier;
            // Логика обработки выигрыша
        }
        
        /// <summary>
        /// Обрабатывает проигрыш
        /// </summary>
        private void ProcessBetLoss(PlayerBet playerBet)
        {
            // Логика обработки проигрыша
        }
        
        /// <summary>
        /// Получает статистику производительности
        /// </summary>
        public string GetPerformanceStats()
        {
            return $"Active Bets: {activeBets.Count}/{maxActiveBets}, " +
                   $"Bet Pool Size: {(useObjectPooling ? betPool.Count.ToString() : "N/A")}, " +
                   $"Update Interval: {updateInterval:F3}s, " +
                   $"Multiplier Cache: {OptimizedMultiplierCalculator.GetCacheStats()}";
        }
        
        /// <summary>
        /// Очищает кэши при завершении
        /// </summary>
        private void OnDestroy()
        {
            OptimizedMultiplierCalculator.ClearAllCaches();
        }
    }
} 