using UnityEngine;

/// <summary>
/// Быстрая валидация всех компонентов математической модели
/// </summary>
public class QuickValidation : MonoBehaviour
{
    [Header("Настройки тестирования")]
    public bool runOnStart = true;
    public int testRounds = 1000; // Быстрый тест
    
    private void Start()
    {
        if (runOnStart)
        {
            ValidateAllComponents();
        }
    }
    
    [ContextMenu("Валидировать все компоненты")]
    public void ValidateAllComponents()
    {
        Debug.Log("🚀 БЫСТРАЯ ВАЛИДАЦИЯ МАТЕМАТИЧЕСКОЙ МОДЕЛИ");
        Debug.Log("=".PadRight(60, '='));
        
        bool allPassed = true;
        
        // Тест 1: MultiplierCalculator
        allPassed &= TestMultiplierCalculator();
        
        // Тест 2: CrashPointGenerator
        allPassed &= TestCrashPointGenerator();
        
        // Тест 3: RTPValidator
        allPassed &= TestRTPValidator();
        
        // Тест 4: GameManager
        allPassed &= TestGameManager();
        
        // Финальный результат
        Debug.Log("=".PadRight(60, '='));
        if (allPassed)
        {
            Debug.Log("✅ ВСЕ КОМПОНЕНТЫ РАБОТАЮТ КОРРЕКТНО!");
            Debug.Log("🎯 Математическая модель готова к использованию.");
        }
        else
        {
            Debug.LogError("❌ ОБНАРУЖЕНЫ ПРОБЛЕМЫ В КОМПОНЕНТАХ!");
        }
    }
    
    private bool TestMultiplierCalculator()
    {
        Debug.Log("🧮 Тестируем MultiplierCalculator...");
        
        try
        {
            float[] testTimes = { 0f, 8f, 35f, 50f };
            float[] expectedValues = { 1.00f, 2.00f, 20.00f, 100.00f };
            
            for (int i = 0; i < testTimes.Length; i++)
            {
                float calculated = MultiplierCalculator.CalculateMultiplier(testTimes[i]);
                float expected = expectedValues[i];
                float accuracy = (1f - Mathf.Abs(calculated - expected) / expected) * 100f;
                
                if (accuracy > 95f)
                {
                    Debug.Log($"  ✅ t={testTimes[i]}s: x{calculated:F2} (точность: {accuracy:F1}%)");
                }
                else
                {
                    Debug.LogError($"  ❌ t={testTimes[i]}s: ожидается x{expected:F2}, получено x{calculated:F2}");
                    return false;
                }
            }
            
            Debug.Log("  ✅ MultiplierCalculator работает корректно");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"  ❌ Ошибка в MultiplierCalculator: {e.Message}");
            return false;
        }
    }
    
    private bool TestCrashPointGenerator()
    {
        Debug.Log("🎲 Тестируем CrashPointGenerator...");
        
        try
        {
            float minCrash = float.MaxValue;
            float maxCrash = float.MinValue;
            float totalCrash = 0f;
            
            for (int i = 0; i < testRounds; i++)
            {
                float crashPoint = CrashPointGenerator.GenerateCrashPoint();
                
                if (crashPoint < 1.01f || crashPoint > 1000f)
                {
                    Debug.LogError($"  ❌ Некорректный краш: x{crashPoint:F2}");
                    return false;
                }
                
                minCrash = Mathf.Min(minCrash, crashPoint);
                maxCrash = Mathf.Max(maxCrash, crashPoint);
                totalCrash += crashPoint;
            }
            
            float avgCrash = totalCrash / testRounds;
            
            Debug.Log($"  ✅ Минимальный краш: x{minCrash:F2}");
            Debug.Log($"  ✅ Максимальный краш: x{maxCrash:F2}");
            Debug.Log($"  ✅ Средний краш: x{avgCrash:F2}");
            Debug.Log("  ✅ CrashPointGenerator работает корректно");
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"  ❌ Ошибка в CrashPointGenerator: {e.Message}");
            return false;
        }
    }
    
    private bool TestRTPValidator()
    {
        Debug.Log("💰 Тестируем RTPValidator...");
        
        try
        {
            var result = RTPValidator.ValidateRTP(testRounds);
            
            Debug.Log($"  ✅ RTP: {result.actualRTP:P4}");
            Debug.Log($"  ✅ Винрейт: {result.successRate:P2}");
            Debug.Log($"  ✅ Максимальный краш: x{result.maxCrashPoint:F2}");
            
            if (result.actualRTP >= 0.95f && result.actualRTP <= 0.97f)
            {
                Debug.Log("  ✅ RTP в допустимых пределах (95-97%)");
                return true;
            }
            else
            {
                Debug.LogWarning($"  ⚠️ RTP вне допустимых пределов: {result.actualRTP:P4}");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"  ❌ Ошибка в RTPValidator: {e.Message}");
            return false;
        }
    }
    
    private bool TestGameManager()
    {
        Debug.Log("🎮 Тестируем GameManager...");
        
        try
        {
            GameObject tempObj = new GameObject("TempGameManager");
            GameManager gameManager = tempObj.AddComponent<GameManager>();
            
            if (gameManager.playerBalance > 0)
            {
                Debug.Log($"  ✅ Баланс игрока: {gameManager.playerBalance}");
                DestroyImmediate(tempObj);
                return true;
            }
            else
            {
                Debug.LogError("  ❌ Некорректный баланс игрока");
                DestroyImmediate(tempObj);
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"  ❌ Ошибка в GameManager: {e.Message}");
            return false;
        }
    }
} 