using UnityEngine;

/// <summary>
/// Вспомогательный скрипт для быстрой настройки тестовой сцены
/// </summary>
public class SceneSetupHelper : MonoBehaviour
{
    [Header("Автоматическая настройка")]
    public bool autoSetupOnStart = true;
    public bool createTestRunner = true;
    public bool createCompilationTest = false;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupTestScene();
        }
    }
    
    /// <summary>
    /// Автоматически настраивает тестовую сцену
    /// </summary>
    [ContextMenu("Настроить тестовую сцену")]
    public void SetupTestScene()
    {
        Debug.Log("🔧 НАСТРОЙКА ТЕСТОВОЙ СЦЕНЫ");
        Debug.Log("=".PadRight(50, '='));
        
        if (createTestRunner)
        {
            CreateMathTestRunner();
        }
        
        if (createCompilationTest)
        {
            CreateCompilationTest();
        }
        
        Debug.Log("✅ Сцена настроена для тестирования!");
        Debug.Log("Нажмите Play для запуска тестов.");
    }
    
    /// <summary>
    /// Создает GameObject с MathTestRunner
    /// </summary>
    private void CreateMathTestRunner()
    {
        // Проверяем, есть ли уже MathTestRunner
        MathTestRunner existingRunner = FindObjectOfType<MathTestRunner>();
        if (existingRunner != null)
        {
            Debug.Log("✅ MathTestRunner уже существует в сцене");
            return;
        }
        
        // Создаем новый GameObject
        GameObject testRunnerObj = new GameObject("MathTestRunner");
        MathTestRunner runner = testRunnerObj.AddComponent<MathTestRunner>();
        
        // Настраиваем параметры
        runner.runTestsOnStart = true;
        runner.rtpTestRounds = 10000;
        runner.showDetailedLogs = true;
        
        Debug.Log("✅ MathTestRunner создан и настроен");
    }
    
    /// <summary>
    /// Создает GameObject с CompilationTest
    /// </summary>
    private void CreateCompilationTest()
    {
        // Проверяем, есть ли уже CompilationTest
        CompilationTest existingTest = FindObjectOfType<CompilationTest>();
        if (existingTest != null)
        {
            Debug.Log("✅ CompilationTest уже существует в сцене");
            return;
        }
        
        // Создаем новый GameObject
        GameObject compilationTestObj = new GameObject("CompilationTest");
        compilationTestObj.AddComponent<CompilationTest>();
        
        Debug.Log("✅ CompilationTest создан");
    }
    
    /// <summary>
    /// Проверяет наличие всех необходимых компонентов
    /// </summary>
    [ContextMenu("Проверить настройки")]
    public void CheckSetup()
    {
        Debug.Log("🔍 ПРОВЕРКА НАСТРОЕК СЦЕНЫ");
        Debug.Log("=".PadRight(50, '='));
        
        // Проверяем MathTestRunner
        MathTestRunner runner = FindObjectOfType<MathTestRunner>();
        if (runner != null)
        {
            Debug.Log($"✅ MathTestRunner найден: {runner.name}");
            Debug.Log($"   runTestsOnStart: {runner.runTestsOnStart}");
            Debug.Log($"   rtpTestRounds: {runner.rtpTestRounds}");
            Debug.Log($"   showDetailedLogs: {runner.showDetailedLogs}");
        }
        else
        {
            Debug.Log("❌ MathTestRunner не найден");
        }
        
        // Проверяем CompilationTest
        CompilationTest compilationTest = FindObjectOfType<CompilationTest>();
        if (compilationTest != null)
        {
            Debug.Log($"✅ CompilationTest найден: {compilationTest.name}");
        }
        else
        {
            Debug.Log("ℹ️ CompilationTest не найден (необязательный)");
        }
        
        // Проверяем камеру
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Debug.Log($"✅ Главная камера найдена: {mainCamera.name}");
        }
        else
        {
            Debug.Log("❌ Главная камера не найдена");
        }
        
        Debug.Log("=".PadRight(50, '='));
    }
} 