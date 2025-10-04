using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource uiSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip cardFlipSound;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip mismatchSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip comboSound;

    [Header("Background Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip victoryMusic;

    [Header("Settings")]
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float uiVolume = 0.8f;

    private static AudioManager instance;
    public static AudioManager Instance => instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetSFXVolume(sfxVolume);
        SetMusicVolume(musicVolume);
        SetUIVolume(uiVolume);
        SubscribeToEvents();
        PlayMenuMusic();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeAudioSources()
    {
        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFX Source");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
        }

        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("Music Source");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        if (uiSource == null)
        {
            GameObject uiGO = new GameObject("UI Source");
            uiGO.transform.SetParent(transform);
            uiSource = uiGO.AddComponent<AudioSource>();
        }
    }

    private void SubscribeToEvents()
    {
        GameEvents.OnCardFlipped += OnCardFlipped;
        GameEvents.OnCardsMatched += OnCardsMatched;
        GameEvents.OnCardsMismatched += OnCardsMismatched;
        GameEvents.OnGameCompleted += OnGameCompleted;
        GameEvents.OnComboChanged += OnComboChanged;
        GameEvents.OnGameStateChanged += OnGameStateChanged;
    }

    private void UnsubscribeFromEvents()
    {
        GameEvents.OnCardFlipped -= OnCardFlipped;
        GameEvents.OnCardsMatched -= OnCardsMatched;
        GameEvents.OnCardsMismatched -= OnCardsMismatched;
        GameEvents.OnGameCompleted -= OnGameCompleted;
        GameEvents.OnComboChanged -= OnComboChanged;
        GameEvents.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnCardFlipped(Card card)
    {
        PlayCardFlipSound();
    }

    private void OnCardsMatched(Card card1, Card card2)
    {
        PlayMatchSound();
    }

    private void OnCardsMismatched(Card card1, Card card2)
    {
        PlayMismatchSound();
    }

    private void OnGameCompleted()
    {
        PlayGameOverSound();
        PlayVictoryMusic();
    }

    private void OnComboChanged(int comboLevel)
    {
        if (comboLevel > 2)
            PlayComboSound();
    }

    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Ready:
                // Play menu music when in Ready state (main menu)
                PlayMenuMusic();
                break;
            case GameState.Playing:
                PlayGameplayMusic();
                break;
            case GameState.Victory:
                PlayVictoryMusic();
                break;
        }
    }

    public void PlayCardFlipSound()
    {
        PlaySFX(cardFlipSound);
    }

    public void PlayMatchSound()
    {
        PlaySFX(matchSound);
    }

    public void PlayMismatchSound()
    {
        PlaySFX(mismatchSound);
    }

    public void PlayWrongMatchSound()
    {
        if (sfxSource != null && mismatchSound != null)
        {
            sfxSource.PlayOneShot(mismatchSound, sfxVolume);
        }
    }

    public void PlayGameOverSound()
    {
        PlaySFX(gameOverSound);
    }

    public void PlayButtonClickSound()
    {
        PlayUI(buttonClickSound);
    }

    public void PlayComboSound()
    {
        PlaySFX(comboSound);
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }

    public void PlayVictoryMusic()
    {
        PlayMusic(victoryMusic);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    private void PlayUI(AudioClip clip)
    {
        if (clip != null && uiSource != null)
            uiSource.PlayOneShot(clip);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip != null && musicSource != null)
        {
            // Only change music if it's different from what's currently playing
            if (musicSource.clip != clip)
            {
                musicSource.clip = clip;
                musicSource.Play();
            }
            // If same clip but not playing, restart it
            else if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        if (uiSource != null)
            uiSource.volume = uiVolume;
    }

    public void ToggleSFX()
    {
        SetSFXVolume(sfxSource.volume > 0 ? 0 : 1);
    }

    public void ToggleMusic()
    {
        SetMusicVolume(musicSource.volume > 0 ? 0 : 0.5f);
    }
}
