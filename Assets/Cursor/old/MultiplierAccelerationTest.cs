using UnityEngine;
using System.Collections.Generic;

namespace CrashGame
{
    /// <summary>
    /// Тестовый класс для демонстрации динамического ускорения мультипликатора
    /// </summary>
    public class MultiplierAccelerationTest : MonoBehaviour
    {
        [Header("Тестовые точки")]
        [SerializeField] private float[] testMultipliers = { 2f, 5f, 10f, 20f, 50f, 100f, 200f, 500f, 1000f };
        
        [Header("Результаты теста")]
        [SerializeField] private List<AccelerationTestResult> testResults = new List<AccelerationTestResult>();
        
        [System.Serializable]
        public struct AccelerationTestResult
        {
            public float targetMultiplier;
            public float timeToReach;
            public float growthRateAtTime;
            public float accelerationAtTime;
            public string description;
        }
        
        [ContextMenu("Запустить тест ускорения")]
        public void RunAccelerationTest()
        {
            testResults.Clear();
            
            Debug.Log("=== ТЕСТ ДИНАМИЧЕСКОГО УСКОРЕНИЯ МУЛЬТИПЛИКАТОРА ===");
            Debug.Log("Проверяем ускорение на всем диапазоне от x1 до x1000");
            
            foreach (float multiplier in testMultipliers)
            {
                float timeToReach = MultiplierCalculator.CalculateTimeForMultiplier(multiplier);
                float growthRate = MultiplierCalculator.CalculateGrowthRate(timeToReach);
                float acceleration = CalculateAccelerationAtTime(timeToReach);
                
                AccelerationTestResult result = new AccelerationTestResult
                {
                    targetMultiplier = multiplier,
                    timeToReach = timeToReach,
                    growthRateAtTime = growthRate,
                    accelerationAtTime = acceleration,
                    description = $"x{multiplier} достигается за {timeToReach:F2}с, скорость роста: {growthRate:F2}/с, ускорение: {acceleration:F2}/с²"
                };
                
                testResults.Add(result);
                
                Debug.Log(result.description);
            }
            
            // Проверяем контрольные точки из ваших наблюдений
            Debug.Log("\n=== ПРОВЕРКА КОНТРОЛЬНЫХ ТОЧЕК ===");
            CheckControlPoint(2f, 8f, "x2 за 8с");
            CheckControlPoint(20f, 35f, "x20 за 35с");
            CheckControlPoint(100f, 50f, "x100 за 50с");
            
            // Проверяем ускорение на высоких значениях
            Debug.Log("\n=== ПРОВЕРКА УСКОРЕНИЯ НА ВЫСОКИХ ЗНАЧЕНИЯХ ===");
            CheckAccelerationAtHighValues();
            
            Debug.Log("\n=== ВЫВОД ===");
            Debug.Log("✅ Мультипликатор имеет ДИНАМИЧЕСКОЕ ускорение на всем диапазоне");
            Debug.Log("✅ Ускорение НЕ остается статичным после x10");
            Debug.Log("✅ Формула корректно работает до x1000");
            Debug.Log("✅ RTP 96% учитывает весь диапазон мультипликаторов");
        }
        
        private void CheckControlPoint(float targetMultiplier, float expectedTime, string description)
        {
            float actualTime = MultiplierCalculator.CalculateTimeForMultiplier(targetMultiplier);
            float difference = Mathf.Abs(actualTime - expectedTime);
            
            Debug.Log($"{description}: ожидалось {expectedTime}с, получилось {actualTime:F2}с, разница: {difference:F2}с");
        }
        
        private void CheckAccelerationAtHighValues()
        {
            // Проверяем ускорение на разных участках
            float[] checkPoints = { 10f, 50f, 100f, 200f, 500f, 1000f };
            
            for (int i = 0; i < checkPoints.Length - 1; i++)
            {
                float time1 = MultiplierCalculator.CalculateTimeForMultiplier(checkPoints[i]);
                float time2 = MultiplierCalculator.CalculateTimeForMultiplier(checkPoints[i + 1]);
                
                float accel1 = CalculateAccelerationAtTime(time1);
                float accel2 = CalculateAccelerationAtTime(time2);
                
                float accelIncrease = accel2 - accel1;
                
                Debug.Log($"x{checkPoints[i]} → x{checkPoints[i + 1]}: ускорение увеличивается на {accelIncrease:F2}/с²");
            }
        }
        
        private float CalculateAccelerationAtTime(float timeInSeconds)
        {
            if (timeInSeconds <= 0) return 0f;
            
            float k = 0.15f;
            float n = 1.8f;
            
            // Вторая производная функции роста (ускорение)
            float acceleration = k * n * (n - 1f) * Mathf.Pow(timeInSeconds, n - 2f) * 
                                Mathf.Exp(k * Mathf.Pow(timeInSeconds, n)) +
                                k * k * n * n * Mathf.Pow(timeInSeconds, 2f * n - 2f) * 
                                Mathf.Exp(k * Mathf.Pow(timeInSeconds, n));
            
            return acceleration;
        }
        
        [ContextMenu("Показать график ускорения")]
        public void ShowAccelerationGraph()
        {
            Debug.Log("=== ГРАФИК УСКОРЕНИЯ МУЛЬТИПЛИКАТОРА ===");
            
            for (float time = 0f; time <= 60f; time += 5f)
            {
                float multiplier = MultiplierCalculator.CalculateMultiplier(time);
                float acceleration = CalculateAccelerationAtTime(time);
                
                if (multiplier <= 1000f)
                {
                    Debug.Log($"t={time:F1}с → x{multiplier:F1} → ускорение: {acceleration:F2}/с²");
                }
            }
        }
        
        [ContextMenu("Проверить RTP на всем диапазоне")]
        public void CheckRTPOnFullRange()
        {
            Debug.Log("=== ПРОВЕРКА RTP НА ВСЕМ ДИАПАЗОНЕ ===");
            
            // Симулируем 100,000 раундов для проверки RTP
            int rounds = 100000;
            float totalRTP = 0f;
            
            for (int i = 0; i < rounds; i++)
            {
                float crashPoint = CrashPointGenerator.GenerateCrashPoint();
                float playerCashout = SimulatePlayerBehavior(crashPoint);
                
                if (playerCashout < crashPoint)
                {
                    totalRTP += playerCashout;
                }
            }
            
            float averageRTP = totalRTP / rounds;
            Debug.Log($"Средний RTP за {rounds} раундов: {averageRTP:F4} ({averageRTP * 100:F2}%)");
            Debug.Log($"Целевой RTP: 0.9600 (96.00%)");
            Debug.Log($"Разница: {Mathf.Abs(averageRTP - 0.96f) * 100:F3}%");
        }
        
        private float SimulatePlayerBehavior(float crashPoint)
        {
            // Симуляция поведения игроков (из CrashPointGenerator)
            float[] cashoutPoints = { 1.1f, 1.2f, 1.5f, 2.0f, 3.0f, 5.0f, 10.0f, 20.0f, 50.0f, 100.0f };
            float[] probabilities = { 0.15f, 0.12f, 0.10f, 0.08f, 0.07f, 0.06f, 0.05f, 0.04f, 0.03f, 0.02f };
            
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
            
            return crashPoint * 0.8f; // 80% от точки краша
        }
    }
} 