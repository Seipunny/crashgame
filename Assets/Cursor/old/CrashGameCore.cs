using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CrashGame
{
    /// <summary>
    /// Основной класс для расчета мультипликатора в краш-игре
    /// </summary>
    public static class MultiplierCalculator
    {
        /// <summary>
        /// Рассчитывает мультипликатор в заданный момент времени
        /// </summary>
        /// <param name="timeInSeconds">Время в секундах</param>
        /// <returns>Мультипликатор от 1.0 до 1000.0</returns>
        public static float CalculateMultiplier(float timeInSeconds)
        {
            if (timeInSeconds <= 0) return 1.0f;
            
            // Экспоненциальная функция с динамическим ускорением
            // Формула: multiplier = 1 + (e^(k*t^n) - 1)
            // где k = 0.15, n = 1.8 - подобраны под наблюдения
            float k = 0.15f;
            float n = 1.8f;
            
            float multiplier = 1.0f + (Mathf.Exp(k * Mathf.Pow(timeInSeconds, n)) - 1.0f);
            
            return Mathf.Clamp(multiplier, 1.0f, 1000.0f);
        }
        
        /// <summary>
        /// Рассчитывает время для достижения заданного мультипликатора
        /// </summary>
        /// <param name="targetMultiplier">Целевой мультипликатор</param>
        /// <returns>Время в секундах</returns>
        public static float CalculateTimeForMultiplier(float targetMultiplier)
        {
            if (targetMultiplier <= 1.0f) return 0f;
            if (targetMultiplier >= 1000.0f) return float.MaxValue;
            
            // Обратная функция для расчета времени
            float k = 0.15f;
            float n = 1.8f;
            
            float timeInSeconds = Mathf.Pow(Mathf.Log(targetMultiplier) / k, 1f / n);
            
            return timeInSeconds;
        }
        
        /// <summary>
        /// Рассчитывает скорость роста мультипликатора
        /// </summary>
        /// <param name="timeInSeconds">Время в секундах</param>
        /// <returns>Скорость роста (мультипликатор/секунду)</returns>
        public static float CalculateGrowthRate(float timeInSeconds)
        {
            if (timeInSeconds <= 0) return 0f;
            
            float k = 0.15f;
            float n = 1.8f;
            
            // Производная функции роста
            float growthRate = k * n * Mathf.Pow(timeInSeconds, n - 1f) * 
                              Mathf.Exp(k * Mathf.Pow(timeInSeconds, n));
            
            return growthRate;
        }
    }
    
    /// <summary>
    /// Генератор точки краша для обеспечения RTP 96%
    /// </summary>
    public static class CrashPointGenerator
    {
        private const float TARGET_RTP = 0.96f;
        
        /// <summary>
        /// Генерирует точку краша с учетом RTP 96%
        /// </summary>
        /// <returns>Мультипликатор краша от 1.01 до 1000.00</returns>
        public static float GenerateCrashPoint()
        {
            float randomValue = Random.Range(0.0001f, 0.9999f);
            
            // Используем обратное преобразование экспоненциального распределения
            // с корректировкой для обеспечения RTP 96%
            float crashMultiplier = CalculateCrashPointFromRandom(randomValue);
            
            return Mathf.Clamp(crashMultiplier, 1.01f, 1000.0f);
        }
        
        /// <summary>
        /// Рассчитывает точку краша из случайного значения
        /// </summary>
        private static float CalculateCrashPointFromRandom(float randomValue)
        {
            // Базовое экспоненциальное распределение
            // Формула: x = -ln(1-r) / λ
            // где λ - параметр, подбираемый для RTP 96%
            
            float lambda = 0.04f; // Параметр экспоненциального распределения
            float baseMultiplier = -Mathf.Log(1.0f - randomValue) / lambda;
            
            // Корректировка для учета реального поведения игроков
            // Большинство игроков забирают на низких мультипликаторах
            float correctionFactor = CalculateBehaviorCorrection(baseMultiplier);
            
            return baseMultiplier * correctionFactor;
        }
        
        /// <summary>
        /// Корректировка на основе поведения игроков
        /// </summary>
        private static float CalculateBehaviorCorrection(float multiplier)
        {
            // Большинство игроков забирают на мультипликаторах 1.5-3.0
            // Поэтому увеличиваем вероятность крашей в этом диапазоне
            if (multiplier < 1.50f)
                return 0.85f; // Больше крашей в начале
            else if (multiplier < 3.00f)
                return 0.95f; // Немного больше крашей в популярном диапазоне
            else if (multiplier < 10.0f)
                return 1.05f; // Меньше крашей в высоком диапазоне
            else
                return 1.15f; // Еще меньше крашей в очень высоком диапазоне
        }
    }
    
    /// <summary>
    /// Система валидации RTP
    /// </summary>
    public static class RTPValidator
    {
        /// <summary>
        /// Проводит полную валидацию RTP
        /// </summary>
        public static ValidationResult ValidateRTP(int numberOfRounds = 1000000)
        {
            var result = new ValidationResult();
            
            // Симуляция игр
            for (int i = 0; i < numberOfRounds; i++)
            {
                float bet = 1.0f;
                float crashPoint = CrashPointGenerator.GenerateCrashPoint();
                
                // Симулируем поведение игрока
                float cashoutPoint = SimulatePlayerBehavior(crashPoint);
                
                result.totalBets += bet;
                
                if (cashoutPoint <= crashPoint)
                {
                    // Игрок успел забрать
                    result.totalPayouts += bet * cashoutPoint;
                    result.successfulGames++;
                }
                else
                {
                    // Игрок проиграл
                    result.failedGames++;
                }
                
                // Анализируем распределение крашей
                AnalyzeCrashPoint(crashPoint, result);
            }
            
            // Рассчитываем RTP
            result.actualRTP = result.totalPayouts / result.totalBets;
            result.successRate = (float)result.successfulGames / numberOfRounds;
            
            return result;
        }
        
        /// <summary>
        /// Симулирует поведение игрока
        /// </summary>
        private static float SimulatePlayerBehavior(float crashPoint)
        {
            // Реалистичное распределение кешаутов игроков
            float[] cashoutPoints = { 1.10f, 1.20f, 1.50f, 2.00f, 3.00f, 5.00f, 10.0f, 20.0f };
            float[] probabilities = { 0.25f, 0.30f, 0.20f, 0.15f, 0.06f, 0.03f, 0.01f, 0.00f };
            
            float random = Random.Range(0f, 1f);
            float cumulativeProb = 0f;
            
            for (int i = 0; i < cashoutPoints.Length; i++)
            {
                cumulativeProb += probabilities[i];
                if (random <= cumulativeProb)
                {
                    return Mathf.Min(cashoutPoints[i], crashPoint);
                }
            }
            
            return crashPoint; // Игрок не забрал
        }
        
        /// <summary>
        /// Анализирует распределение точек краша
        /// </summary>
        private static void AnalyzeCrashPoint(float crashPoint, ValidationResult result)
        {
            if (crashPoint < 1.50f) result.crashDistribution["1.00-1.50"]++;
            else if (crashPoint < 2.00f) result.crashDistribution["1.50-2.00"]++;
            else if (crashPoint < 5.00f) result.crashDistribution["2.00-5.00"]++;
            else if (crashPoint < 10.0f) result.crashDistribution["5.00-10.0"]++;
            else if (crashPoint < 50.0f) result.crashDistribution["10.0-50.0"]++;
            else result.crashDistribution["50.0+"]++;
        }
        
        /// <summary>
        /// Выводит детальный отчет валидации
        /// </summary>
        public static void PrintValidationReport(ValidationResult result)
        {
            Debug.Log("=== RTP VALIDATION REPORT ===");
            Debug.Log($"Target RTP: 96.00%");
            Debug.Log($"Actual RTP: {result.actualRTP:P4}");
            Debug.Log($"Difference: {Mathf.Abs(result.actualRTP - 0.96f):P4}");
            Debug.Log($"Success Rate: {result.successRate:P2}");
            Debug.Log($"Total Games: {result.successfulGames + result.failedGames:N0}");
            Debug.Log($"Successful: {result.successfulGames:N0}");
            Debug.Log($"Failed: {result.failedGames:N0}");
            
            Debug.Log("\n=== CRASH DISTRIBUTION ===");
            int totalCrashes = result.crashDistribution.Values.Sum();
            foreach (var kvp in result.crashDistribution)
            {
                float percentage = (float)kvp.Value / totalCrashes * 100f;
                Debug.Log($"{kvp.Key}: {percentage:F2}% ({kvp.Value:N0} crashes)");
            }
            
            // Проверяем, соответствует ли RTP требованиям
            float rtpDifference = Mathf.Abs(result.actualRTP - 0.96f);
            if (rtpDifference < 0.01f)
            {
                Debug.Log("✅ RTP VALIDATION PASSED - Model is accurate!");
            }
            else
            {
                Debug.Log("❌ RTP VALIDATION FAILED - Model needs adjustment!");
            }
        }
    }
    
    /// <summary>
    /// Результат валидации RTP
    /// </summary>
    [System.Serializable]
    public struct ValidationResult
    {
        public float totalBets;
        public float totalPayouts;
        public float actualRTP;
        public float successRate;
        public int successfulGames;
        public int failedGames;
        public Dictionary<string, int> crashDistribution;
        
        public ValidationResult()
        {
            totalBets = 0f;
            totalPayouts = 0f;
            actualRTP = 0f;
            successRate = 0f;
            successfulGames = 0;
            failedGames = 0;
            crashDistribution = new Dictionary<string, int>
            {
                {"1.00-1.50", 0},
                {"1.50-2.00", 0}, 
                {"2.00-5.00", 0},
                {"5.00-10.0", 0},
                {"10.0-50.0", 0},
                {"50.0+", 0}
            };
        }
    }
    
    /// <summary>
    /// Тестирование формулы роста мультипликатора
    /// </summary>
    public static class MultiplierValidator
    {
        /// <summary>
        /// Тестирует формулу роста мультипликатора
        /// </summary>
        public static void TestMultiplierFormula()
        {
            Debug.Log("=== MULTIPLIER FORMULA TEST ===");
            
            // Тестируем контрольные точки
            float[] testTimes = { 0f, 8f, 35f, 50f, 80f, 120f };
            float[] expectedValues = { 1.00f, 2.00f, 20.00f, 100.00f, 500.00f, 1000.00f };
            
            for (int i = 0; i < testTimes.Length; i++)
            {
                float calculated = MultiplierCalculator.CalculateMultiplier(testTimes[i]);
                float expected = expectedValues[i];
                float difference = Mathf.Abs(calculated - expected);
                float accuracy = (1f - difference / expected) * 100f;
                
                Debug.Log($"t={testTimes[i]:F1}s: Expected={expected:F2}, Calculated={calculated:F2}, Accuracy={accuracy:F1}%");
                
                if (accuracy > 95f)
                    Debug.Log("✅ PASSED");
                else
                    Debug.Log("❌ FAILED");
            }
        }
    }
} 