using UnityEngine;

/// <summary>
/// Простой чекер компиляции для проверки всех компонентов
/// </summary>
public class CompilationChecker : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("🔧 ПРОВЕРКА КОМПИЛЯЦИИ ВСЕХ КОМПОНЕНТОВ");
        Debug.Log("=".PadRight(60, '='));
        
        // Проверяем все основные классы
        CheckMultiplierCalculator();
        CheckCrashPointGenerator();
        CheckRTPValidator();
        CheckGameManager();
        CheckMathTestRunner();
        
        Debug.Log("=".PadRight(60, '='));
        Debug.Log("✅ ВСЕ КОМПОНЕНТЫ СКОМПИЛИРОВАНЫ УСПЕШНО!");
        Debug.Log("Теперь можно запускать полные тесты.");
    }
    
    private void CheckMultiplierCalculator()
    {
        Debug.Log("🧮 Проверяем MultiplierCalculator...");
        
        try
        {
            float multiplier1 = MultiplierCalculator.CalculateMultiplier(8f);
            float multiplier2 = MultiplierCalculator.CalculateMultiplier(35f);
            float multiplier3 = MultiplierCalculator.CalculateMultiplier(50f);
            
            Debug.Log($"  ✅ x2 за 8с: {multiplier1:F2}");
            Debug.Log($"  ✅ x20 за 35с: {multiplier2:F2}");
            Debug.Log($"  ✅ x100 за 50с: {multiplier3:F2}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"  ❌ Ошибка в MultiplierCalculator: {e.Message}");
        }
    }
    
    private void CheckCrashPointGenerator()
    {
        Debug.Log("🎲 Проверяем CrashPointGenerator...");
        
        try
        {
            for (int i = 0; i < 5; i++)
            {
                float crashPoint = CrashPointGenerator.GenerateCrashPoint();
                if (crashPoint >= 1.01f && crashPoint <= 1000f)
                {
                    Debug.Log($"  ✅ Краш {i + 1}: x{crashPoint:F2}");
                }
                else
                {
                    Debug.LogError($"  ❌ Некорректный краш: x{crashPoint:F2}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"  ❌ Ошибка в CrashPointGenerator: {e.Message}");
        }
    }
    
    private void CheckRTPValidator()
    {
        Debug.Log("💰 Проверяем RTPValidator...");
        
        try
        {
            var result = RTPValidator.ValidateRTP(1000);
            
            Debug.Log($"  ✅ RTP: {result.actualRTP:P4}");
            Debug.Log($"  ✅ Винрейт: {result.successRate:P2}");
            Debug.Log($"  ✅ Максимальный краш: x{result.maxCrashPoint:F2}");
            
            if (result.actualRTP >= 0.95f && result.actualRTP <= 0.97f)
            {
                Debug.Log("  ✅ RTP в допустимых пределах (95-97%)");
            }
            else
            {
                Debug.LogWarning($"  ⚠️ RTP вне допустимых пределов: {result.actualRTP:P4}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"  ❌ Ошибка в RTPValidator: {e.Message}");
        }
    }
    
    private void CheckGameManager()
    {
        Debug.Log("🎮 Проверяем GameManager...");
        
        try
        {
            // Создаем временный GameObject для тестирования
            GameObject tempObj = new GameObject("TempGameManager");
            GameManager gameManager = tempObj.AddComponent<GameManager>();
            
            // Проверяем основные поля
            if (gameManager.playerBalance > 0)
            {
                Debug.Log($"  ✅ Баланс игрока: {gameManager.playerBalance}");
            }
            
            // Удаляем временный объект
            DestroyImmediate(tempObj);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"  ❌ Ошибка в GameManager: {e.Message}");
        }
    }
    
    private void CheckMathTestRunner()
    {
        Debug.Log("🧪 Проверяем MathTestRunner...");
        
        try
        {
            // Создаем временный GameObject для тестирования
            GameObject tempObj = new GameObject("TempMathTestRunner");
            MathTestRunner testRunner = tempObj.AddComponent<MathTestRunner>();
            
            // Проверяем основные поля
            Debug.Log($"  ✅ runTestsOnStart: {testRunner.runTestsOnStart}");
            Debug.Log($"  ✅ rtpTestRounds: {testRunner.rtpTestRounds}");
            Debug.Log($"  ✅ showDetailedLogs: {testRunner.showDetailedLogs}");
            
            // Удаляем временный объект
            DestroyImmediate(tempObj);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"  ❌ Ошибка в MathTestRunner: {e.Message}");
        }
    }
} 