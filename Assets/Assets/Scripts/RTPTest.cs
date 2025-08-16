using UnityEngine;

/// <summary>
/// Простой тест для проверки исправленного RTP
/// </summary>
public class RTPTest : MonoBehaviour
{
    [Header("Настройки теста")]
    public bool runOnStart = true;
    public int testRounds = 10000;
    
    private void Start()
    {
        if (runOnStart)
        {
            TestRTP();
        }
    }
    
    [ContextMenu("Тест RTP")]
    public void TestRTP()
    {
        Debug.Log("🎯 ТЕСТ ИСПРАВЛЕННОГО RTP");
        Debug.Log("=".PadRight(50, '='));
        
        var result = RTPValidator.ValidateRTP(testRounds);
        
        Debug.Log($"📊 Результаты теста ({testRounds:N0} раундов):");
        Debug.Log($"🎯 RTP: {result.actualRTP:P4}");
        Debug.Log($"📈 Винрейт: {result.successRate:P2}");
        Debug.Log($"🔥 Максимальный краш: x{result.maxCrashPoint:F2}");
        Debug.Log($"💰 Максимальный выигрыш: {result.maxWin:F2}");
        
        // Проверяем, соответствует ли RTP требованиям
        float targetRTP = 0.96f;
        float deviation = Mathf.Abs(result.actualRTP - targetRTP);
        
        if (deviation <= 0.01f) // ±1%
        {
            Debug.Log("✅ RTP В НОРМЕ! Отклонение в пределах ±1%");
        }
        else if (deviation <= 0.02f) // ±2%
        {
            Debug.Log("⚠️ RTP ПРИЕМЛЕМО! Отклонение в пределах ±2%");
        }
        else
        {
            Debug.LogError("❌ RTP НЕ В НОРМЕ! Требуется доработка");
        }
        
        Debug.Log($"🎯 Целевой RTP: {targetRTP:P2}");
        Debug.Log($"📊 Фактический RTP: {result.actualRTP:P4}");
        Debug.Log($"📏 Отклонение: {deviation:P4}");
        
        Debug.Log("=".PadRight(50, '='));
    }
} 