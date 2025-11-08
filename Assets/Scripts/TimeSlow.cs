using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TimeSlow : MonoBehaviour
{
    [Header("SUPERHOT Time Settings")]
    [Range(0.01f, 1f)] public float minTimeScale = 0.05f;
    [Range(0.1f, 2f)] public float maxTimeScale = 1.0f;
    [Tooltip("Higher = faster reaction to movement changes")]
    public float responsiveness = 15f;
    public bool includeMouseMovement = true;

    [Header("Manual Slow Motion Trigger (optional)")]
    public float slowdownFactor = 0.05f;
    public float slowdownLength = 2f;

    private CharacterController controller;
    private bool temporarilyResumed = false;
    public Volume globalVolume;
    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;

    void Start()
    {
        controller = FindFirstObjectByType<CharacterController>();

        if (globalVolume != null)
        {
            if (!globalVolume.profile.TryGet(out lensDistortion))
            {
                lensDistortion = globalVolume.profile.Add<LensDistortion>(true);
                lensDistortion.active = true;
            }

            if (!globalVolume.profile.TryGet(out chromaticAberration))
            {
                chromaticAberration = globalVolume.profile.Add<ChromaticAberration>(true);
                chromaticAberration.active = true;
            }
        }
        else
        {
            Debug.LogError("Global Volume not assigned in the inspector!");
        }
    }


    void Update()
    {
        if (temporarilyResumed)
            return; // skip automatic slow-mo while in resume burst
        
        if (controller != null)
        {
            // --- Dynamic slow motion like SUPERHOT ---
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 desiredMove = new Vector3(horizontal, 0, vertical);

            float movementMagnitude = desiredMove.magnitude;
            float mouseInfluence = 0f;

            if (includeMouseMovement)
            {
                mouseInfluence = Mathf.Abs(Input.GetAxis("Mouse X")) + Mathf.Abs(Input.GetAxis("Mouse Y"));
                mouseInfluence = Mathf.Clamp01(mouseInfluence * 0.5f);
            }

            float targetTimeScale = Mathf.Clamp(movementMagnitude + mouseInfluence, minTimeScale, maxTimeScale);
            float lerpSpeed = responsiveness * Time.unscaledDeltaTime;
            float slowStrength = 1f - Mathf.InverseLerp(minTimeScale, maxTimeScale, Time.timeScale);
            Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, lerpSpeed);
            float targetAberration = Mathf.Lerp(0f, 1f, slowStrength);
            chromaticAberration.intensity.value = Mathf.Lerp(
                chromaticAberration.intensity.value,
                targetAberration,
                lerpSpeed * 0.5f
            );
            float targetDistortion = Mathf.Lerp(0f, -0.15f, slowStrength);
            lensDistortion.intensity.value = Mathf.Lerp(
                lensDistortion.intensity.value,
                targetDistortion,
                lerpSpeed
            );
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else
        {
            // fallback to normal behavior
            Time.timeScale += (1f / slowdownLength) * Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
        }
    }

    public void DoSlowMotion()
    {
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    // --- NEW FEATURE: Resume time temporarily ---
    public void ResumeTimeTemporarily(float duration = 0.1f)
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(ResumeRoutine(duration));
    }

    private IEnumerator ResumeRoutine(float duration)
    {
        temporarilyResumed = true;
        Time.timeScale = maxTimeScale;
        Time.fixedDeltaTime = 0.02f;
        yield return new WaitForSecondsRealtime(duration); // unaffected by timescale
        temporarilyResumed = false;
    }
}
