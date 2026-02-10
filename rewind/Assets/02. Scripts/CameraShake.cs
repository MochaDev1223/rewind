using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    [Header("Cinemachine Camera")]
    public CinemachineCamera cinemachineCamera;
    
    private CinemachineBasicMultiChannelPerlin noise;
    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (cinemachineCamera != null)
        {
            // Cinemachine Noise 컴포넌트 가져오기
            noise = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
        else
        {
            Debug.LogError("CameraShake: Cinemachine Camera가 할당되지 않았습니다!");
        }
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            
            // 시간에 따라 강도 감소
            if (noise != null)
            {
                noise.AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));
            }

            if (shakeTimer <= 0f)
            {
                // 쉐이크 종료
                if (noise != null)
                {
                    noise.AmplitudeGain = 0f;
                }
            }
        }
    }

    /// <summary>
    /// 기본 카메라 쉐이크
    /// </summary>
    /// <param name="intensity">흔들림 강도 (0~10 권장)</param>
    /// <param name="time">지속 시간</param>
    public void Shake(float intensity = 2f, float time = 0.3f)
    {
        if (noise != null)
        {
            noise.AmplitudeGain = intensity;
            startingIntensity = intensity;
            shakeTimerTotal = time;
            shakeTimer = time;
        }
    }

    /// <summary>
    /// 보스 사망용 강한 쉐이크
    /// </summary>
    public void ShakeBossDeath()
    {
        StartCoroutine(BossDeathShakeCoroutine());
    }

    private IEnumerator BossDeathShakeCoroutine()
    {
        if (noise == null) yield break;

        // 1단계: 매우 강한 쉐이크
        float duration1 = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration1)
        {
            noise.AmplitudeGain = 8f;
            noise.FrequencyGain = 3f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2단계: 중간 강도 쉐이크
        float duration2 = 0.5f;
        elapsed = 0f;
        while (elapsed < duration2)
        {
            float progress = elapsed / duration2;
            noise.AmplitudeGain = Mathf.Lerp(5f, 2f, progress);
            noise.FrequencyGain = 2f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3단계: 약한 여진
        float duration3 = 0.7f;
        elapsed = 0f;
        while (elapsed < duration3)
        {
            float progress = elapsed / duration3;
            noise.AmplitudeGain = Mathf.Lerp(2f, 0f, progress);
            noise.FrequencyGain = 1f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 쉐이크 종료
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 1f;
    }

    /// <summary>
    /// 커스텀 쉐이크 (강도와 주파수 모두 조절)
    /// </summary>
    public void ShakeCustom(float amplitude, float frequency, float time)
    {
        StartCoroutine(ShakeCustomCoroutine(amplitude, frequency, time));
    }

    private IEnumerator ShakeCustomCoroutine(float amplitude, float frequency, float time)
    {
        if (noise == null) yield break;

        noise.AmplitudeGain = amplitude;
        noise.FrequencyGain = frequency;

        yield return new WaitForSeconds(time);

        // 부드럽게 감소
        float fadeTime = 0.2f;
        float elapsed = 0f;
        float startAmplitude = amplitude;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeTime;
            noise.AmplitudeGain = Mathf.Lerp(startAmplitude, 0f, progress);
            yield return null;
        }

        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 1f;
    }

    /// <summary>
    /// 펄스 쉐이크 (충격파 느낌)
    /// </summary>
    public void ShakePulse(int pulseCount = 3, float pulseIntensity = 5f, float pulseDuration = 0.1f)
    {
        StartCoroutine(ShakePulseCoroutine(pulseCount, pulseIntensity, pulseDuration));
    }

    private IEnumerator ShakePulseCoroutine(int pulseCount, float pulseIntensity, float pulseDuration)
    {
        if (noise == null) yield break;

        for (int i = 0; i < pulseCount; i++)
        {
            // 펄스 강도 (점점 약해짐)
            float currentIntensity = pulseIntensity * (1f - (float)i / pulseCount);
            
            noise.AmplitudeGain = currentIntensity;
            noise.FrequencyGain = 3f;

            yield return new WaitForSeconds(pulseDuration);

            // 짧은 휴식
            noise.AmplitudeGain = 0f;
            yield return new WaitForSeconds(0.05f);
        }

        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 1f;
    }

    /// <summary>
    /// 쉐이크 즉시 중지
    /// </summary>
    public void StopShake()
    {
        shakeTimer = 0f;
        if (noise != null)
        {
            noise.AmplitudeGain = 0f;
            noise.FrequencyGain = 1f;
        }
    }
}