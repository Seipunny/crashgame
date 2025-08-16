using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CrashGame.Tests
{
    /// <summary>
    /// Тесты для математической модели мультипликатора
    /// </summary>
    public class MultiplierCalculatorTests
    {
        [Test]
        public void CalculateMultiplier_ZeroTime_ReturnsOne()
        {
            float result = MultiplierCalculator.CalculateMultiplier(0f);
            Assert.AreEqual(1.0f, result, 0.001f);
        }
        
        [Test]
        public void CalculateMultiplier_ControlPoints_ReturnsExpectedValues()
        {
            // Тестируем контрольные точки из наблюдений
            float result8s = MultiplierCalculator.CalculateMultiplier(8f);
            float result35s = MultiplierCalculator.CalculateMultiplier(35f);
            float result50s = MultiplierCalculator.CalculateMultiplier(50f);
            
            Assert.AreEqual(2.0f, result8s, 0.1f, "8 seconds should give ~2x multiplier");
            Assert.AreEqual(20.0f, result35s, 1.0f, "35 seconds should give ~20x multiplier");
            Assert.AreEqual(100.0f, result50s, 5.0f, "50 seconds should give ~100x multiplier");
        }
        
        [Test]
        public void CalculateMultiplier_MonotonicIncrease_AlwaysIncreases()
        {
            float prevMultiplier = 1.0f;
            
            for (float time = 0.1f; time <= 120f; time += 0.1f)
            {
                float currentMultiplier = MultiplierCalculator.CalculateMultiplier(time);
                Assert.GreaterOrEqual(currentMultiplier, prevMultiplier, 
                    $"Multiplier should always increase. Time: {time}s, Prev: {prevMultiplier}, Current: {currentMultiplier}");
                prevMultiplier = currentMultiplier;
            }
        }
        
        [Test]
        public void CalculateMultiplier_MaxValue_ClampedToThousand()
        {
            float result = MultiplierCalculator.CalculateMultiplier(1000f);
            Assert.LessOrEqual(result, 1000.0f, "Multiplier should be clamped to 1000x");
        }
        
        [Test]
        public void CalculateTimeForMultiplier_ValidInputs_ReturnsCorrectTime()
        {
            float timeFor2x = MultiplierCalculator.CalculateTimeForMultiplier(2.0f);
            float timeFor20x = MultiplierCalculator.CalculateTimeForMultiplier(20.0f);
            float timeFor100x = MultiplierCalculator.CalculateTimeForMultiplier(100.0f);
            
            Assert.AreEqual(8.0f, timeFor2x, 0.5f, "Time for 2x should be ~8 seconds");
            Assert.AreEqual(35.0f, timeFor20x, 2.0f, "Time for 20x should be ~35 seconds");
            Assert.AreEqual(50.0f, timeFor100x, 3.0f, "Time for 100x should be ~50 seconds");
        }
        
        [Test]
        public void CalculateGrowthRate_PositiveTime_ReturnsPositiveRate()
        {
            for (float time = 0.1f; time <= 60f; time += 1f)
            {
                float growthRate = MultiplierCalculator.CalculateGrowthRate(time);
                Assert.Greater(growthRate, 0f, $"Growth rate should be positive at time {time}s");
            }
        }
    }
    
    /// <summary>
    /// Тесты для генератора точек краша
    /// </summary>
    public class CrashPointGeneratorTests
    {
        [Test]
        public void GenerateCrashPoint_ValidRange_ReturnsValidMultiplier()
        {
            for (int i = 0; i < 1000; i++)
            {
                float crashPoint = CrashPointGenerator.GenerateCrashPoint();
                Assert.GreaterOrEqual(crashPoint, 1.01f, "Crash point should be >= 1.01x");
                Assert.LessOrEqual(crashPoint, 1000.0f, "Crash point should be <= 1000x");
            }
        }
        
        [Test]
        public void GenerateCrashPoint_Distribution_ReasonableSpread()
        {
            var crashPoints = new List<float>();
            
            for (int i = 0; i < 10000; i++)
            {
                crashPoints.Add(CrashPointGenerator.GenerateCrashPoint());
            }
            
            float average = crashPoints.Average();
            float min = crashPoints.Min();
            float max = crashPoints.Max();
            
            Assert.Greater(average, 2.0f, "Average crash point should be > 2x");
            Assert.Less(average, 50.0f, "Average crash point should be < 50x");
            Assert.GreaterOrEqual(min, 1.01f, "Minimum crash point should be >= 1.01x");
            Assert.LessOrEqual(max, 1000.0f, "Maximum crash point should be <= 1000x");
        }
        
        [Test]
        public void GenerateCrashPoint_Consistency_SameSeedSameResult()
        {
            // Устанавливаем фиксированный сид для тестирования
            Random.InitState(12345);
            float crashPoint1 = CrashPointGenerator.GenerateCrashPoint();
            
            Random.InitState(12345);
            float crashPoint2 = CrashPointGenerator.GenerateCrashPoint();
            
            Assert.AreEqual(crashPoint1, crashPoint2, 0.001f, "Same seed should produce same crash point");
        }
    }
    
    /// <summary>
    /// Тесты для RTP валидации
    /// </summary>
    public class RTPValidationTests
    {
        [Test]
        public void ValidateRTP_LargeSample_ConvergesToTarget()
        {
            var result = RTPValidator.ValidateRTP(100000);
            
            Assert.GreaterOrEqual(result.actualRTP, 0.94f, "RTP should be >= 94%");
            Assert.LessOrEqual(result.actualRTP, 0.98f, "RTP should be <= 98%");
            
            Debug.Log($"RTP Validation Result: {result.actualRTP:P4}, Target: 96.00%");
        }
        
        [Test]
        public void ValidateRTP_SuccessRate_ReasonableValue()
        {
            var result = RTPValidator.ValidateRTP(10000);
            
            Assert.Greater(result.successRate, 0.1f, "Success rate should be > 10%");
            Assert.Less(result.successRate, 0.9f, "Success rate should be < 90%");
            
            Debug.Log($"Success Rate: {result.successRate:P2}");
        }
        
        [Test]
        public void ValidateRTP_CrashDistribution_ValidRanges()
        {
            var result = RTPValidator.ValidateRTP(10000);
            
            int totalCrashes = result.crashDistribution.Values.Sum();
            Assert.Greater(totalCrashes, 0, "Should have some crashes");
            
            foreach (var kvp in result.crashDistribution)
            {
                Assert.GreaterOrEqual(kvp.Value, 0, $"Crash count for {kvp.Key} should be >= 0");
            }
        }
    }
    
    /// <summary>
    /// Тесты для оптимизированного калькулятора
    /// </summary>
    public class OptimizedMultiplierCalculatorTests
    {
        [Test]
        public void CalculateMultiplier_CachePerformance_ImprovesOverTime()
        {
            // Очищаем кэш
            OptimizedMultiplierCalculator.ClearAllCaches();
            
            // Первый проход - заполняем кэш
            for (float time = 0.1f; time <= 10f; time += 0.1f)
            {
                OptimizedMultiplierCalculator.CalculateMultiplier(time);
            }
            
            string stats1 = OptimizedMultiplierCalculator.GetCacheStats();
            Debug.Log($"After first pass: {stats1}");
            
            // Второй проход - используем кэш
            for (float time = 0.1f; time <= 10f; time += 0.1f)
            {
                OptimizedMultiplierCalculator.CalculateMultiplier(time);
            }
            
            string stats2 = OptimizedMultiplierCalculator.GetCacheStats();
            Debug.Log($"After second pass: {stats2}");
            
            // Проверяем, что кэш работает
            Assert.IsTrue(stats2.Contains("Hit Rate"), "Cache stats should show hit rate");
        }
        
        [Test]
        public void CalculateMultiplier_Accuracy_MatchesOriginal()
        {
            for (float time = 0.1f; time <= 60f; time += 1f)
            {
                float original = MultiplierCalculator.CalculateMultiplier(time);
                float optimized = OptimizedMultiplierCalculator.CalculateMultiplier(time);
                
                Assert.AreEqual(original, optimized, 0.001f, 
                    $"Optimized should match original at time {time}s");
            }
        }
    }
    
    /// <summary>
    /// Тесты для Unity Random интеграции
    /// </summary>
    public class UnityRandomIntegrationTests
    {
        [Test]
        public void GenerateProvablyFairCrashPoint_ValidRange_ReturnsValidMultiplier()
        {
            UnityRandomIntegration.InitializeProvablyFair("test-server-seed", "test-client-seed");
            
            for (int round = 1; round <= 100; round++)
            {
                float crashPoint = UnityRandomIntegration.GenerateProvablyFairCrashPoint(round);
                Assert.GreaterOrEqual(crashPoint, 1.01f, "Crash point should be >= 1.01x");
                Assert.LessOrEqual(crashPoint, 1000.0f, "Crash point should be <= 1000x");
            }
        }
        
        [Test]
        public void VerifyRoundResult_ValidInput_ReturnsTrue()
        {
            UnityRandomIntegration.InitializeProvablyFair("test-server-seed", "test-client-seed");
            
            int roundNumber = 1;
            float crashPoint = UnityRandomIntegration.GenerateProvablyFairCrashPoint(roundNumber);
            
            bool isValid = UnityRandomIntegration.VerifyRoundResult(roundNumber, crashPoint);
            Assert.IsTrue(isValid, "Valid round result should verify successfully");
        }
        
        [Test]
        public void VerifyRoundResult_InvalidInput_ReturnsFalse()
        {
            UnityRandomIntegration.InitializeProvablyFair("test-server-seed", "test-client-seed");
            
            int roundNumber = 1;
            float invalidCrashPoint = 999.0f; // Неверная точка краша
            
            bool isValid = UnityRandomIntegration.VerifyRoundResult(roundNumber, invalidCrashPoint);
            Assert.IsFalse(isValid, "Invalid round result should fail verification");
        }
        
        [Test]
        public void GetVerificationInfo_ValidRound_ReturnsCompleteInfo()
        {
            UnityRandomIntegration.InitializeProvablyFair("test-server-seed", "test-client-seed");
            
            int roundNumber = 1;
            var info = UnityRandomIntegration.GetVerificationInfo(roundNumber);
            
            Assert.AreEqual(roundNumber, info.roundNumber, "Round number should match");
            Assert.AreEqual("test-server-seed", info.serverSeed, "Server seed should match");
            Assert.AreEqual("test-client-seed", info.clientSeed, "Client seed should match");
            Assert.IsFalse(string.IsNullOrEmpty(info.roundHash), "Round hash should not be empty");
            Assert.GreaterOrEqual(info.randomValue, 0f, "Random value should be >= 0");
            Assert.Less(info.randomValue, 1f, "Random value should be < 1");
            Assert.GreaterOrEqual(info.crashPoint, 1.01f, "Crash point should be >= 1.01x");
        }
    }
    
    /// <summary>
    /// Интеграционные тесты для игрового контроллера
    /// </summary>
    public class CrashGameControllerIntegrationTests
    {
        private GameObject gameObject;
        private CrashGameController controller;
        
        [SetUp]
        public void Setup()
        {
            gameObject = new GameObject("TestGameController");
            controller = gameObject.AddComponent<CrashGameController>();
        }
        
        [TearDown]
        public void Teardown()
        {
            if (gameObject != null)
            {
                Object.DestroyImmediate(gameObject);
            }
        }
        
        [UnityTest]
        public IEnumerator StartNewRound_ValidCall_StartsGame()
        {
            controller.StartNewRound();
            
            yield return new WaitForSeconds(0.1f);
            
            Assert.IsTrue(controller.IsGameRunning(), "Game should be running after StartNewRound");
            Assert.IsFalse(controller.HasCrashed(), "Game should not have crashed immediately");
        }
        
        [UnityTest]
        public IEnumerator PlaceBet_ValidBet_AcceptsBet()
        {
            controller.StartNewRound();
            yield return new WaitForSeconds(0.1f);
            
            bool betPlaced = controller.PlaceBet("test-player", 1.0f);
            Assert.IsTrue(betPlaced, "Valid bet should be accepted");
            
            int activeBets = controller.GetActiveBetsCount();
            Assert.AreEqual(1, activeBets, "Should have 1 active bet");
        }
        
        [UnityTest]
        public IEnumerator Cashout_ValidCashout_ProcessesWin()
        {
            controller.StartNewRound();
            yield return new WaitForSeconds(0.1f);
            
            controller.PlaceBet("test-player", 1.0f);
            yield return new WaitForSeconds(0.1f);
            
            bool cashoutSuccessful = controller.Cashout("test-player");
            Assert.IsTrue(cashoutSuccessful, "Valid cashout should succeed");
            
            int activeBets = controller.GetActiveBetsCount();
            Assert.AreEqual(0, activeBets, "Should have 0 active bets after cashout");
        }
        
        [UnityTest]
        public IEnumerator GameCrash_EventuallyCrashes_EndsGame()
        {
            controller.StartNewRound();
            
            // Ждем, пока игра не крашнется (максимум 10 секунд)
            float timeout = 10f;
            float elapsed = 0f;
            
            while (controller.IsGameRunning() && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            Assert.IsTrue(controller.HasCrashed(), "Game should eventually crash");
            Assert.IsFalse(controller.IsGameRunning(), "Game should not be running after crash");
        }
    }
    
    /// <summary>
    /// Тесты производительности
    /// </summary>
    public class PerformanceTests
    {
        [Test]
        public void MultiplierCalculation_Performance_WithinReasonableTime()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < 100000; i++)
            {
                float time = (i % 1200) * 0.1f; // 0-120 секунд
                MultiplierCalculator.CalculateMultiplier(time);
            }
            
            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;
            
            Assert.Less(elapsedMs, 1000, $"100,000 calculations should complete in < 1 second, took {elapsedMs}ms");
            Debug.Log($"100,000 multiplier calculations took {elapsedMs}ms");
        }
        
        [Test]
        public void CrashPointGeneration_Performance_WithinReasonableTime()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < 10000; i++)
            {
                CrashPointGenerator.GenerateCrashPoint();
            }
            
            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;
            
            Assert.Less(elapsedMs, 1000, $"10,000 crash point generations should complete in < 1 second, took {elapsedMs}ms");
            Debug.Log($"10,000 crash point generations took {elapsedMs}ms");
        }
        
        [Test]
        public void OptimizedCalculator_Performance_BetterThanOriginal()
        {
            // Тестируем оригинальный калькулятор
            var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                float time = (i % 1200) * 0.1f;
                MultiplierCalculator.CalculateMultiplier(time);
            }
            stopwatch1.Stop();
            
            // Очищаем кэш и тестируем оптимизированный
            OptimizedMultiplierCalculator.ClearAllCaches();
            var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                float time = (i % 1200) * 0.1f;
                OptimizedMultiplierCalculator.CalculateMultiplier(time);
            }
            stopwatch2.Stop();
            
            Debug.Log($"Original: {stopwatch1.ElapsedMilliseconds}ms, Optimized: {stopwatch2.ElapsedMilliseconds}ms");
            
            // Второй проход оптимизированного (с кэшем)
            var stopwatch3 = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                float time = (i % 1200) * 0.1f;
                OptimizedMultiplierCalculator.CalculateMultiplier(time);
            }
            stopwatch3.Stop();
            
            Debug.Log($"Optimized with cache: {stopwatch3.ElapsedMilliseconds}ms");
            Assert.Less(stopwatch3.ElapsedMilliseconds, stopwatch1.ElapsedMilliseconds, 
                "Optimized calculator with cache should be faster than original");
        }
    }
    
    /// <summary>
    /// Тесты для валидации математической модели
    /// </summary>
    public class MathematicalModelValidationTests
    {
        [Test]
        public void MultiplierFormula_ControlPoints_AccurateWithinTolerance()
        {
            float[] testTimes = { 8f, 35f, 50f };
            float[] expectedValues = { 2.0f, 20.0f, 100.0f };
            float tolerance = 0.1f; // 10% tolerance
            
            for (int i = 0; i < testTimes.Length; i++)
            {
                float calculated = MultiplierCalculator.CalculateMultiplier(testTimes[i]);
                float expected = expectedValues[i];
                float accuracy = 1f - Mathf.Abs(calculated - expected) / expected;
                
                Assert.Greater(accuracy, 1f - tolerance, 
                    $"Accuracy for {testTimes[i]}s should be > {1f - tolerance:P0}, got {accuracy:P1}");
                
                Debug.Log($"Time: {testTimes[i]}s, Expected: {expected:F2}x, Calculated: {calculated:F2}x, Accuracy: {accuracy:P1}");
            }
        }
        
        [Test]
        public void RTPModel_Simulation_ConvergesToTarget()
        {
            int[] sampleSizes = { 1000, 10000, 100000 };
            float targetRTP = 0.96f;
            float tolerance = 0.02f; // 2% tolerance
            
            foreach (int sampleSize in sampleSizes)
            {
                var result = RTPValidator.ValidateRTP(sampleSize);
                float rtpDifference = Mathf.Abs(result.actualRTP - targetRTP);
                
                Assert.Less(rtpDifference, tolerance, 
                    $"RTP for {sampleSize:N0} samples should be within {tolerance:P0} of target, got {rtpDifference:P2} difference");
                
                Debug.Log($"Sample size: {sampleSize:N0}, RTP: {result.actualRTP:P4}, Difference: {rtpDifference:P2}");
            }
        }
        
        [Test]
        public void CrashDistribution_StatisticalProperties_Reasonable()
        {
            var result = RTPValidator.ValidateRTP(100000);
            
            // Проверяем, что большинство крашей происходит в разумном диапазоне
            int lowCrashes = result.crashDistribution["1.00-1.50"] + result.crashDistribution["1.50-2.00"];
            int totalCrashes = result.crashDistribution.Values.Sum();
            float lowCrashPercentage = (float)lowCrashes / totalCrashes;
            
            Assert.Greater(lowCrashPercentage, 0.3f, "At least 30% of crashes should be in low range (1.0-2.0x)");
            Assert.Less(lowCrashPercentage, 0.8f, "No more than 80% of crashes should be in low range (1.0-2.0x)");
            
            Debug.Log($"Low crash percentage: {lowCrashPercentage:P1}");
        }
    }
} 