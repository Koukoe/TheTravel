using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the WaveDecode shader - a waveform decryption puzzle.
/// Player adjusts Amplitude and Frequency to match the answer wave.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class WaveDecodeController : MonoBehaviour
{
    [Header("Answer Wave (Target)")]
    [SerializeField] private float answerAmplitude = 1.2f;
    [SerializeField] private float answerFrequency = 4.5f;
    [SerializeField] private float answerPhase;
    [SerializeField] private float answerOffset;
    
    [Header("Player Wave (Adjustable)")]
    [Range(0.1f, 2.0f)]
    [SerializeField] private float amplitude = 1.0f;
    [Range(0.5f, 10.0f)]
    [SerializeField] private float frequency = 3.0f;
    
    [Header("Decode Settings")]
    [Range(0.01f, 0.5f)]
    [SerializeField] private float matchThreshold = 0.15f;
    [SerializeField] private float matchRequired = 0.9f; // % of wave points must match to decode
    
    [Header("Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent onDecodeSuccess;
    [SerializeField] private UnityEngine.Events.UnityEvent onDecodeFail;
    
    [Header("UI (Optional)")]
    [SerializeField] private Slider amplitudeSlider;
    [SerializeField] private Slider frequencySlider;
    [SerializeField] private Image progressBar;
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    
    private Material material;
    private bool isDecoded;
    private float matchProgress;
    private const int SAMPLE_POINTS = 128;
    
    private void Awake()
    {
        // Try to get material from renderer
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            material = rend.material;
            // Change to use WaveDecode shader if not already
            Shader waveShader = Shader.Find("Custom/WaveDecode");
            if (waveShader != null && material.shader != waveShader)
            {
                material.shader = waveShader;
            }
        }
        
        // Try to get material from Image (UI)
        if (material == null)
        {
            Image img = GetComponent<Image>();
            if (img != null && img.material != null)
            {
                material = img.material;
            }
        }
    }
    
    private void Start()
    {
        PushAnswerToShader();
        SyncMaterial();
        
        // Wire up UI sliders
        if (amplitudeSlider != null)
        {
            amplitudeSlider.minValue = 0.1f;
            amplitudeSlider.maxValue = 2.0f;
            amplitudeSlider.value = amplitude;
            amplitudeSlider.onValueChanged.AddListener(SetAmplitude);
        }
        
        if (frequencySlider != null)
        {
            frequencySlider.minValue = 0.5f;
            frequencySlider.maxValue = 10.0f;
            frequencySlider.value = frequency;
            frequencySlider.onValueChanged.AddListener(SetFrequency);
        }
    }
    
    private void Update()
    {
        if (material == null || isDecoded) return;
        
        EvaluateMatch();
    }
    
    /// <summary>Set player wave amplitude and update shader.</summary>
    public void SetAmplitude(float value)
    {
        amplitude = value;
        SyncMaterial();
    }
    
    /// <summary>Set player wave frequency and update shader.</summary>
    public void SetFrequency(float value)
    {
        frequency = value;
        SyncMaterial();
    }
    
    /// <summary>Randomize the answer wave.</summary>
    public void RandomizeAnswer()
    {
        answerAmplitude = Random.Range(0.3f, 1.8f);
        answerFrequency = Random.Range(1.0f, 9.0f);
        answerPhase = Random.Range(0f, Mathf.PI * 2f);
        answerOffset = Random.Range(-0.5f, 0.5f);
        
        PushAnswerToShader();
        isDecoded = false;
        matchProgress = 0f;
        
        if (statusText != null) statusText.text = "";
    }
    
    /// <summary>Generate a new random answer, keep player settings.</summary>
    public void NewPuzzle()
    {
        RandomizeAnswer();
        matchProgress = 0f;
        if (progressBar != null) progressBar.fillAmount = 0f;
    }
    
    /// <summary>Get current match progress (0-1).</summary>
    public float GetMatchProgress() => matchProgress;
    
    /// <summary>Check if fully decoded.</summary>
    public bool IsDecoded() => isDecoded;
    
    private void PushAnswerToShader()
    {
        if (material == null) return;
        
        material.SetFloat("_AnsAmplitude", answerAmplitude);
        material.SetFloat("_AnsFrequency", answerFrequency);
        material.SetFloat("_AnsPhase", answerPhase);
        material.SetFloat("_AnsOffset", answerOffset);
        material.SetFloat("_MatchThreshold", matchThreshold);
    }
    
    private void SyncMaterial()
    {
        if (material == null) return;
        
        material.SetFloat("_PlayerAmplitude", amplitude);
        material.SetFloat("_PlayerFrequency", frequency);
    }
    
    private void EvaluateMatch()
    {
        // Sample the wave across the X axis and check match percentage
        int matchCount = 0;
        
        for (int i = 0; i < SAMPLE_POINTS; i++)
        {
            float x = (float)i / SAMPLE_POINTS;
            
            float ansY = WaveY(x, answerAmplitude, answerFrequency, answerPhase, answerOffset);
            float playerY = WaveY(x, amplitude, frequency, 0f, 0f);
            
            if (Mathf.Abs(ansY - playerY) < matchThreshold)
            {
                matchCount++;
            }
        }
        
        matchProgress = (float)matchCount / SAMPLE_POINTS;
        
        // Update shader
        if (material != null)
        {
            material.SetFloat("_MatchProgress", matchProgress);
        }
        
        // Update UI
        if (progressBar != null)
        {
            progressBar.fillAmount = matchProgress;
        }
        
        // Check decode success
        if (matchProgress >= matchRequired && !isDecoded)
        {
            isDecoded = true;
            if (statusText != null) statusText.text = "解密成功！";
            onDecodeSuccess?.Invoke();
        }
        else if (matchProgress < matchRequired && isDecoded)
        {
            // Lost match - could happen if player adjusts after decoding
            // Uncomment if you want to allow "losing" the match
            // isDecoded = false;
            // onDecodeFail?.Invoke();
        }
    }
    
    private static float WaveY(float x, float amp, float freq, float phase, float offset)
    {
        return amp * Mathf.Sin(x * freq * (Mathf.PI * 2f) + phase) + offset;
    }
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && material != null)
        {
            PushAnswerToShader();
            SyncMaterial();
            material.SetFloat("_MatchThreshold", matchThreshold);
        }
        else if (!Application.isPlaying)
        {
            // Editor preview: update static batching won't work but helper for inspectors
        }
    }
    #endif
}
