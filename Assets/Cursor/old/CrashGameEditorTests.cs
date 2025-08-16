using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

namespace CrashGame.EditorTests
{
    /// <summary>
    /// Тесты для редактора Unity
    /// </summary>
    public class CrashGameEditorTests
    {
        [Test]
        public void MultiplierCalculator_EditorMode_WorksCorrectly()
        {
            // Тестируем в режиме редактора
            float result = MultiplierCalculator.CalculateMultiplier(8f);
            Assert.AreEqual(2.0f, result, 0.1f, "Multiplier calculation should work in editor mode");
        }
        
        [Test]
        public void CrashPointGenerator_EditorMode_GeneratesValidPoints()
        {
            for (int i = 0; i < 100; i++)
            {
                float crashPoint = CrashPointGenerator.GenerateCrashPoint();
                Assert.GreaterOrEqual(crashPoint, 1.01f, "Crash point should be >= 1.01x");
                Assert.LessOrEqual(crashPoint, 1000.0f, "Crash point should be <= 1000x");
            }
        }
        
        [Test]
        public void RTPValidator_EditorMode_ValidatesCorrectly()
        {
            var result = RTPValidator.ValidateRTP(10000);
            Assert.GreaterOrEqual(result.actualRTP, 0.94f, "RTP should be >= 94%");
            Assert.LessOrEqual(result.actualRTP, 0.98f, "RTP should be <= 98%");
        }
        
        [Test]
        public void UnityRandomIntegration_EditorMode_WorksWithoutMonoBehaviour()
        {
            UnityRandomIntegration.InitializeProvablyFair("editor-test-seed", "editor-client-seed");
            
            float crashPoint = UnityRandomIntegration.GenerateProvablyFairCrashPoint(1);
            Assert.GreaterOrEqual(crashPoint, 1.01f, "Provably fair crash point should be >= 1.01x");
            Assert.LessOrEqual(crashPoint, 1000.0f, "Provably fair crash point should be <= 1000x");
        }
    }
    
    /// <summary>
    /// Инструменты для тестирования в редакторе
    /// </summary>
    public static class CrashGameEditorTools
    {
        [MenuItem("Crash Game/Test Mathematical Model")]
        public static void TestMathematicalModel()
        {
            Debug.Log("=== TESTING MATHEMATICAL MODEL IN EDITOR ===");
            
            // Тестируем формулу мультипликатора
            TestMultiplierFormula();
            
            // Тестируем RTP
            TestRTPValidation();
            
            // Тестируем Provably Fair
            TestProvablyFair();
            
            Debug.Log("=== EDITOR TESTS COMPLETED ===");
        }
        
        [MenuItem("Crash Game/Validate RTP")]
        public static void TestRTPValidation()
        {
            Debug.Log("=== RTP VALIDATION IN EDITOR ===");
            
            var result = RTPValidator.ValidateRTP(100000);
            RTPValidator.PrintValidationReport(result);
            
            float rtpDifference = Mathf.Abs(result.actualRTP - 0.96f);
            if (rtpDifference < 0.01f)
            {
                Debug.Log("✅ RTP VALIDATION PASSED - Model is accurate!");
            }
            else
            {
                Debug.LogWarning($"⚠️ RTP VALIDATION WARNING - Difference: {rtpDifference:P2}");
            }
        }
        
        [MenuItem("Crash Game/Test Multiplier Formula")]
        public static void TestMultiplierFormula()
        {
            Debug.Log("=== MULTIPLIER FORMULA TEST IN EDITOR ===");
            
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
                    Debug.LogWarning("❌ FAILED");
            }
        }
        
        [MenuItem("Crash Game/Test Provably Fair")]
        public static void TestProvablyFair()
        {
            Debug.Log("=== PROVABLY FAIR TEST IN EDITOR ===");
            
            UnityRandomIntegration.InitializeProvablyFair("editor-test-seed", "editor-client-seed");
            
            for (int i = 1; i <= 5; i++)
            {
                float crashPoint = UnityRandomIntegration.GenerateProvablyFairCrashPoint(i);
                bool isValid = UnityRandomIntegration.VerifyRoundResult(i, crashPoint);
                
                Debug.Log($"Round {i}: Crash={crashPoint:F2}x, Valid={isValid}");
            }
        }
        
        [MenuItem("Crash Game/Performance Test")]
        public static void PerformanceTest()
        {
            Debug.Log("=== PERFORMANCE TEST IN EDITOR ===");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Тест мультипликатора
            for (int i = 0; i < 100000; i++)
            {
                float time = (i % 1200) * 0.1f;
                MultiplierCalculator.CalculateMultiplier(time);
            }
            
            stopwatch.Stop();
            Debug.Log($"100,000 multiplier calculations: {stopwatch.ElapsedMilliseconds}ms");
            
            // Тест генерации крашей
            stopwatch.Restart();
            for (int i = 0; i < 10000; i++)
            {
                CrashPointGenerator.GenerateCrashPoint();
            }
            stopwatch.Stop();
            Debug.Log($"10,000 crash point generations: {stopwatch.ElapsedMilliseconds}ms");
        }
        
        [MenuItem("Crash Game/Generate Test Report")]
        public static void GenerateTestReport()
        {
            Debug.Log("=== GENERATING COMPREHENSIVE TEST REPORT ===");
            
            // 1. Тест мультипликатора
            TestMultiplierFormula();
            
            // 2. Тест RTP
            TestRTPValidation();
            
            // 3. Тест Provably Fair
            TestProvablyFair();
            
            // 4. Тест производительности
            PerformanceTest();
            
            // 5. Тест распределения крашей
            TestCrashDistribution();
            
            Debug.Log("=== COMPREHENSIVE TEST REPORT COMPLETED ===");
        }
        
        [MenuItem("Crash Game/Test Crash Distribution")]
        public static void TestCrashDistribution()
        {
            Debug.Log("=== CRASH DISTRIBUTION TEST IN EDITOR ===");
            
            var crashPoints = new List<float>();
            
            for (int i = 0; i < 10000; i++)
            {
                crashPoints.Add(CrashPointGenerator.GenerateCrashPoint());
            }
            
            // Анализируем распределение
            var distribution = new Dictionary<string, int>
            {
                {"1.00-1.50", 0},
                {"1.50-2.00", 0}, 
                {"2.00-5.00", 0},
                {"5.00-10.0", 0},
                {"10.0-50.0", 0},
                {"50.0+", 0}
            };
            
            foreach (float crashPoint in crashPoints)
            {
                if (crashPoint < 1.50f) distribution["1.00-1.50"]++;
                else if (crashPoint < 2.00f) distribution["1.50-2.00"]++;
                else if (crashPoint < 5.00f) distribution["2.00-5.00"]++;
                else if (crashPoint < 10.0f) distribution["5.00-10.0"]++;
                else if (crashPoint < 50.0f) distribution["10.0-50.0"]++;
                else distribution["50.0+"]++;
            }
            
            Debug.Log("Crash Distribution:");
            foreach (var kvp in distribution)
            {
                float percentage = (float)kvp.Value / crashPoints.Count * 100f;
                Debug.Log($"{kvp.Key}: {percentage:F2}% ({kvp.Value:N0} crashes)");
            }
            
            float average = crashPoints.Average();
            float min = crashPoints.Min();
            float max = crashPoints.Max();
            
            Debug.Log($"Average: {average:F2}x, Min: {min:F2}x, Max: {max:F2}x");
        }
        
        [MenuItem("Crash Game/Clear All Caches")]
        public static void ClearAllCaches()
        {
            OptimizedMultiplierCalculator.ClearAllCaches();
            UnityRandomIntegration.ClearHashCache();
            Debug.Log("All caches cleared");
        }
        
        [MenuItem("Crash Game/Export Test Data")]
        public static void ExportTestData()
        {
            Debug.Log("=== EXPORTING TEST DATA ===");
            
            var testData = new List<string>
            {
                "Time,Multiplier,CrashPoint"
            };
            
            for (float time = 0f; time <= 120f; time += 0.1f)
            {
                float multiplier = MultiplierCalculator.CalculateMultiplier(time);
                float crashPoint = CrashPointGenerator.GenerateCrashPoint();
                
                testData.Add($"{time:F1},{multiplier:F6},{crashPoint:F6}");
            }
            
            string csvData = string.Join("\n", testData);
            string filePath = "Assets/CrashGame_TestData.csv";
            
            System.IO.File.WriteAllText(filePath, csvData);
            Debug.Log($"Test data exported to: {filePath}");
            
            AssetDatabase.Refresh();
        }
    }
    
    /// <summary>
    /// Кастомный редактор для тестирования
    /// </summary>
    [CustomEditor(typeof(CrashGameController))]
    public class CrashGameControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            CrashGameController controller = (CrashGameController)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Testing Tools", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Test Mathematical Model"))
            {
                controller.TestMathematicalModel();
            }
            
            if (GUILayout.Button("Start New Round"))
            {
                controller.StartNewRound();
            }
            
            if (GUILayout.Button("Place Test Bet"))
            {
                controller.PlaceBet("test-player", 1.0f);
            }
            
            if (GUILayout.Button("Cashout Test Bet"))
            {
                controller.Cashout("test-player");
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Game Status", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Current Multiplier", controller.GetCurrentMultiplier());
            EditorGUILayout.FloatField("Crash Point", controller.GetCrashPoint());
            EditorGUILayout.Toggle("Game Running", controller.IsGameRunning());
            EditorGUILayout.Toggle("Has Crashed", controller.HasCrashed());
            EditorGUILayout.IntField("Active Bets", controller.GetActiveBetsCount());
            EditorGUILayout.FloatField("Game Time", controller.GetGameTime());
            EditorGUI.EndDisabledGroup();
        }
    }
} 