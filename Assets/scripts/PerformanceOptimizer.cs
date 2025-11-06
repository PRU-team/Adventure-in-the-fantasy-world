using UnityEngine;

/// <summary>
/// Script tối ưu hóa hiệu suất game
/// Đảm bảo FPS ổn định và tắt các tính năng không cần thiết
/// </summary>
public class PerformanceOptimizer : MonoBehaviour
{
    [Header("FPS Settings")]
    [Tooltip("Target frame rate (-1 = không giới hạn, 60 = 60fps)")]
    [SerializeField] private int targetFrameRate = 60;
    
    [Header("Optimization Settings")]
    [Tooltip("Tắt VSync để tăng FPS")]
    [SerializeField] private bool disableVSync = true;
    
    [Tooltip("Giảm Fixed Timestep để cải thiện physics performance")]
    [SerializeField] private bool optimizeFixedTimestep = true;
    
    [SerializeField] private float fixedTimestep = 0.02f; // 50 physics updates/second
    
    void Awake()
    {
        // Tắt VSync nếu được bật
        if (disableVSync)
        {
            QualitySettings.vSyncCount = 0;
        }
        
        // Đặt target frame rate
        Application.targetFrameRate = targetFrameRate;
        
        // Tối ưu Fixed Timestep cho physics
        if (optimizeFixedTimestep)
        {
            Time.fixedDeltaTime = fixedTimestep;
        }
        
        // Tắt một số tính năng không cần thiết
        Application.runInBackground = true;
        
        Debug.Log($"[Performance Optimizer] Initialized - Target FPS: {targetFrameRate}, VSync: {!disableVSync}");
    }
    
    // Hiển thị FPS counter (optional - có thể tắt trong build)
    private void OnGUI()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        int fps = (int)(1f / Time.unscaledDeltaTime);
        GUI.Label(new Rect(10, 10, 100, 20), $"FPS: {fps}");
        #endif
    }
}
