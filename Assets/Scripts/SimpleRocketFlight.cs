using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SimpleRocketFlight : MonoBehaviour
{
    [Header("References")]
    public RectTransform rocket; // Use RectTransform for UI
    public LineRenderer parabolicLine; // Continuous parabolic line from start to current pos
    public RectTransform actionWindow; // 1400x1500 window
    public TextMeshProUGUI timerLabel; // Timer display in format "15,2s"
    public Material lineTrailMaterial; // Assign in Inspector for WebGL compatibility
    
    [Header("Animation Settings")]
    public Vector2 startPos = new Vector2(150, 150);
    public Vector2 endPos = new Vector2(1220, 1031);
    public float flightDuration = 3f;
    
    [Header("End Animation")]
    public float wobbleMinY = 900f;
    public float wobbleMaxY = 1038f;
    public float wobbleSpeed = 2f;
    
    private CryptCrashGamePrototype gameController;
    private bool isAnimating = false;
    private float animationTimer = 0f;
    private bool hasReachedEnd = false;
    private float lastRotation = 0f; // Store rotation for wobble phase
    private bool isCrashing = false;
    private float crashTimer = 0f;
    
    void Start()
    {
        gameController = FindObjectOfType<CryptCrashGamePrototype>();
        SetupParabolicLine();
        
        // Keep action window always active
        if (actionWindow != null)
            actionWindow.gameObject.SetActive(true);
            
        // Hide only rocket initially
        if (rocket != null)
            rocket.gameObject.SetActive(false);
    }
    
    void SetupParabolicLine()
    {
        if (parabolicLine != null)
        {
            // Load "Line" material from Resources folder
            Material lineMaterial = Resources.Load<Material>("Line");
            if (lineMaterial == null)
            {
                // Fallback to Inspector assigned material
                lineMaterial = lineTrailMaterial;
                Debug.Log("Using Inspector assigned material");
            }
            else
            {
                Debug.Log("Loaded 'Line' material from Resources folder");
            }
            
            // Final fallback - create material at runtime
            if (lineMaterial == null)
            {
                Shader lineShader = Shader.Find("Sprites/Default");
                if (lineShader == null)
                {
                    lineShader = Shader.Find("Unlit/Color");
                }
                lineMaterial = new Material(lineShader);
                lineMaterial.color = new Color(0.7f, 0.4f, 1f, 1f);
                Debug.LogWarning("No material found - using runtime fallback");
            }
            
            parabolicLine.material = lineMaterial;
            parabolicLine.startColor = new Color(0.7f, 0.4f, 1f, 1f); // Purple with full alpha
            parabolicLine.endColor = new Color(1f, 1f, 1f, 0f); // White with zero alpha
            parabolicLine.startWidth = 2.5f; // Half the previous size
            parabolicLine.endWidth = 1.5f; // Half the previous size
            parabolicLine.useWorldSpace = false; // Try screen space instead
            parabolicLine.sortingLayerName = "Default";
            parabolicLine.sortingOrder = 100; // Very high order
            parabolicLine.positionCount = 0;
            
            // Enable the LineRenderer
            parabolicLine.enabled = true;
            
            // Debug log to confirm setup
            Debug.Log($"Parabolic line setup: Material={lineMaterial?.name ?? "NULL"}, Enabled={parabolicLine.enabled}");
            Debug.Log($"LineRenderer: StartWidth={parabolicLine.startWidth}, EndWidth={parabolicLine.endWidth}, UseWorldSpace={parabolicLine.useWorldSpace}");
            Debug.Log($"LineRenderer Position Count: {parabolicLine.positionCount}");
        }
        else
        {
            Debug.LogError("Parabolic line is null! Please assign LineRenderer in inspector.");
        }
    }
    
    void Update()
    {
        bool shouldAnimate = gameController != null && gameController.isGameRunning;
        
        if (shouldAnimate && !isAnimating)
        {
            StartAnimation();
        }
        else if (!shouldAnimate && isAnimating && !isCrashing)
        {
            StartCrashAnimation();
        }
        
        if (isAnimating)
        {
            UpdateAnimation();
            UpdateTimer();
        }
        
        if (isCrashing)
        {
            UpdateCrashAnimation();
            UpdateTimer();
        }
        
        // Update timer even when not animating to show current game time
        if (gameController != null && gameController.isGameRunning)
        {
            UpdateTimer();
        }
        else if (timerLabel != null && !isCrashing)
        {
            timerLabel.text = "0,0s"; // Reset when not running
        }
    }
    
    
    void StartAnimation()
    {
        isAnimating = true;
        animationTimer = 0f;
        hasReachedEnd = false;
        lastRotation = 0f;
        
        // Position rocket at start and show it
        if (rocket != null)
        {
            rocket.anchoredPosition = startPos;
            rocket.gameObject.SetActive(true);
        }
        
        // Clear parabolic line
        if (parabolicLine != null)
            parabolicLine.positionCount = 0;
    }
    
    void StartCrashAnimation()
    {
        isCrashing = true;
        isAnimating = false;
        crashTimer = 0f;
    }
    
    void UpdateCrashAnimation()
    {
        if (rocket == null) return;
        
        crashTimer += Time.deltaTime;
        float crashDuration = 1f; // 1 second crash animation
        
        if (crashTimer < crashDuration)
        {
            // Crash effects
            float progress = crashTimer / crashDuration;
            
            // 1. Rapid spinning
            float spinSpeed = 720f; // 2 full rotations per second
            float currentSpin = lastRotation + (spinSpeed * crashTimer);
            rocket.rotation = Quaternion.AngleAxis(currentSpin, Vector3.forward);
            
            // 2. Scale down (rocket shrinks)
            float scale = Mathf.Lerp(1f, 0.2f, progress);
            rocket.localScale = Vector3.one * scale;
            
            // 3. Fade out
            if (rocket.GetComponent<UnityEngine.UI.Image>() != null)
            {
                var image = rocket.GetComponent<UnityEngine.UI.Image>();
                Color color = image.color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                image.color = color;
            }
            
            // 4. Small random shake
            Vector2 basePos = rocket.anchoredPosition;
            float shakeIntensity = 10f * (1f - progress); // Shake decreases over time
            Vector2 shake = new Vector2(
                UnityEngine.Random.Range(-shakeIntensity, shakeIntensity),
                UnityEngine.Random.Range(-shakeIntensity, shakeIntensity)
            );
            rocket.anchoredPosition = basePos + shake;
            
            // Update line to follow crashing rocket
            UpdateParabolicLine(1f);
        }
        else
        {
            // Crash finished - hide rocket and stop
            StopAnimation();
        }
    }
    
    void StopAnimation()
    {
        isAnimating = false;
        isCrashing = false;
        
        // Reset rocket properties
        if (rocket != null)
        {
            rocket.localScale = Vector3.one;
            rocket.rotation = Quaternion.identity;
            
            // Reset alpha if it has an image component
            if (rocket.GetComponent<UnityEngine.UI.Image>() != null)
            {
                var image = rocket.GetComponent<UnityEngine.UI.Image>();
                Color color = image.color;
                color.a = 1f;
                image.color = color;
            }
            
            rocket.gameObject.SetActive(false);
        }
        
        // Hide parabolic line
        if (parabolicLine != null)
            parabolicLine.positionCount = 0;
            
        // Reset timer
        if (timerLabel != null)
            timerLabel.text = "0,0s";
    }
    
    void UpdateTimer()
    {
        if (timerLabel == null || gameController == null) return;
        
        float currentTime = gameController.gameTime;
        
        // Format time as "15,2s" (using comma as decimal separator)
        string timeString = currentTime.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
        timeString = timeString.Replace('.', ','); // Replace dot with comma
        timerLabel.text = timeString + "s";
    }
    
    void UpdateAnimation()
    {
        animationTimer += Time.deltaTime;
        
        if (!hasReachedEnd)
        {
            // Flight phase: 0 to 3 seconds
            if (animationTimer <= flightDuration)
            {
                UpdateFlightPhase();
            }
            else
            {
                hasReachedEnd = true;
                animationTimer = 0f; // Reset timer for wobble phase
            }
        }
        else
        {
            // Wobble phase: after reaching destination
            UpdateWobblePhase();
        }
    }
    
    void UpdateFlightPhase()
    {
        if (rocket == null) return;
        
        // Calculate progress (0 to 1)
        float progress = animationTimer / flightDuration;
        
        // Store previous position for rotation calculation
        Vector2 previousPos = rocket.anchoredPosition;
        
        // Create parabolic curve (x²)
        Vector2 currentPos = CalculateParabolicTrajectory(progress);
        
        // Update rocket position
        rocket.anchoredPosition = currentPos;
        
        // Update rocket rotation to look at movement direction and store it
        UpdateRocketRotation(previousPos, currentPos);
        lastRotation = rocket.rotation.eulerAngles.z;
        
        // Update continuous parabolic line from start to current position
        UpdateParabolicLine(progress);
    }
    
    Vector2 CalculateParabolicTrajectory(float progress)
    {
        // X position: linear progression over time
        float x = Mathf.Lerp(startPos.x, endPos.x, progress);
        
        // Y position: parabolic curve (x²)
        // Use progress² for parabolic growth
        float parabolicProgress = progress * progress; // This creates x² curve
        
        float y = Mathf.Lerp(startPos.y, endPos.y, parabolicProgress);
        
        return new Vector2(x, y);
    }
    
    void UpdateParabolicLine(float progress)
    {
        if (parabolicLine == null || rocket == null) return;
        
        // Line should start from fixed point (-550, -600) and go to current rocket position
        Vector3 lineStart = new Vector3(-550f, -600f, 0f);
        Vector3 lineEnd = new Vector3(rocket.anchoredPosition.x, rocket.anchoredPosition.y, 0f);
        
        // Create parabolic curve between start and rocket position
        int lineSegments = 20;
        Vector3[] linePoints = new Vector3[lineSegments + 1];
        
        for (int i = 0; i <= lineSegments; i++)
        {
            float t = (float)i / lineSegments;
            
            // Linear interpolation for X
            float x = Mathf.Lerp(lineStart.x, lineEnd.x, t);
            
            // Parabolic interpolation for Y (creates curved path)
            float parabolicT = t * t; // x² curve
            float y = Mathf.Lerp(lineStart.y, lineEnd.y, parabolicT);
            
            linePoints[i] = new Vector3(x, y, 0);
        }
        
        parabolicLine.positionCount = linePoints.Length;
        parabolicLine.SetPositions(linePoints);
        
        // Debug line positions
        if (linePoints.Length > 0)
        {
            Debug.Log($"LineRenderer updated: {linePoints.Length} points, Start={linePoints[0]}, End={linePoints[linePoints.Length-1]}");
        }
    }
    
    void UpdateWobblePhase()
    {
        if (rocket == null) return;
        
        // Keep X position at end
        float x = endPos.x;
        
        // Smooth transition into wobble - start from finish position and gradually expand
        float wobbleTransitionTime = 3f; // 3 seconds to reach full wobble
        float transitionProgress = Mathf.Min(animationTimer / wobbleTransitionTime, 1f);
        float amplitudeMultiplier = Mathf.SmoothStep(0f, 1f, transitionProgress);
        
        // Start wobble from the actual finish position, not the center
        float wobbleCenter = Mathf.Lerp(endPos.y, (wobbleMinY + wobbleMaxY) / 2f, transitionProgress);
        float wobbleRange = (wobbleMaxY - wobbleMinY) / 2f * amplitudeMultiplier;
        
        // Combine multiple sine waves for smoother, more natural wobble
        // Start with very slow wobble and gradually increase frequency
        float speedMultiplier = Mathf.Lerp(0.2f, 1f, transitionProgress);
        float wobble1 = Mathf.Sin(animationTimer * wobbleSpeed * 0.8f * speedMultiplier) * 0.6f;
        float wobble2 = Mathf.Sin(animationTimer * wobbleSpeed * 1.2f * speedMultiplier) * 0.3f;
        float wobble3 = Mathf.Sin(animationTimer * wobbleSpeed * 0.6f * speedMultiplier) * 0.1f;
        float combinedWobble = wobble1 + wobble2 + wobble3;
        
        float y = wobbleCenter + combinedWobble * wobbleRange;
        
        Vector2 currentPos = new Vector2(x, y);
        rocket.anchoredPosition = currentPos;
        
        // Update parabolic line to follow rocket during wobble
        UpdateParabolicLine(1f); // Use progress = 1 since we're past flight phase
        
        // Maintain the last rotation from flight phase (don't change rotation to zero)
        rocket.rotation = Quaternion.AngleAxis(lastRotation, Vector3.forward);
    }
    
    void UpdateRocketRotation(Vector2 previousPos, Vector2 currentPos)
    {
        if (rocket == null) return;
        
        // Calculate movement direction
        Vector2 direction = (currentPos - previousPos).normalized;
        
        // Only rotate if there's significant movement
        if (direction.magnitude > 0.01f)
        {
            // Calculate angle in degrees
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Apply rotation (assuming rocket sprite points right by default)
            rocket.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    
    // Manual control methods
    public void StartManualAnimation()
    {
        StartAnimation();
    }
    
    public void StopManualAnimation()
    {
        StopAnimation();
    }
}