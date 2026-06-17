using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    // --- UPGRADE AICI: Am adăugat controlul de volum ---
    [System.Serializable]
    public struct SoundClip
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] // Asta va crea un slider frumos în Unity Editor!
        public float volume;
    }

    public List<SoundClip> soundClips;

    // Acum dicționarul memorează tot pachetul (clip + volum)
    private Dictionary<string, SoundClip> soundDictionary;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;

        soundDictionary = new Dictionary<string, SoundClip>();
        foreach (SoundClip sound in soundClips)
        {
            // Predescoperire eroare: Dacă uiți să pui volumul, îl setăm automat la maxim (1) ca să se audă
            if (sound.volume == 0f)
            {
                var tempSound = sound;
                tempSound.volume = 1f;
                soundDictionary.Add(tempSound.name, tempSound);
            }
            else
            {
                soundDictionary.Add(sound.name, sound);
            }
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void PlaySound(string name)
    {
        if (soundDictionary.ContainsKey(name))
        {
            // PlayOneShot redă acum clipul la volumul setat de tine!
            sfxSource.PlayOneShot(soundDictionary[name].clip, soundDictionary[name].volume);
        }
        else
        {
            Debug.LogWarning("AudioManager: Efectul '" + name + "' nu a fost găsit!");
        }
    }

    public void PlayMusic(string name)
    {
        if (soundDictionary.ContainsKey(name))
        {
            if (musicSource.clip == soundDictionary[name].clip && musicSource.isPlaying)
            {
                return;
            }
            musicSource.clip = soundDictionary[name].clip;
            // Setăm și volumul muzicii când o pornim
            musicSource.volume = soundDictionary[name].volume;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioManager: Muzica '" + name + "' nu a fost găsită!");
        }
    }
}