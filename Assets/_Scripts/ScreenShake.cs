using UnityEngine;
using Cinemachine; // IMPORTANT: Am adãugat linia asta ca sã vorbim cu Cinemachine
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake instance;
    private CinemachineVirtualCamera vcam;
    private CinemachineBasicMultiChannelPerlin noise;

    private void Awake()
    {
        if (instance == null) instance = this;

        // Preluãm referin?ele de la Cinemachine
        vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    public void Shake(float durata, float putere)
    {
        if (noise != null)
        {
            StartCoroutine(ExecutaShake(durata, putere));
        }
    }

    private IEnumerator ExecutaShake(float durata, float putere)
    {
        // Pornim cutremurul dând putere ?i vitezã
        noise.m_AmplitudeGain = putere;
        noise.m_FrequencyGain = 20f; // <--- Fã-l 20f sau 25f aici pentru un tremurat mai rapid

        yield return new WaitForSeconds(durata);

        // Oprim cutremurul
        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;
    }
}