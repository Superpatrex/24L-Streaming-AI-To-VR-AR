using UnityEngine;

// Copyright (c) 2013-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Based on the versions from RacR4 and AG SSC Racer.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Utilities/Music Controller")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [RequireComponent(typeof(AudioSource))]
    public class MusicController : MonoBehaviour
    {
        #region Public Static Variables
        public static MusicController musicController;
        #endregion

        #region Public Variables
        [Range(0f, 1f)] public float initialMusicVolume = 0.2f;
        [Range(0f, 1f)] public float initialGameVolume = 1f;

        public AudioClip[] musicTracks;
        public float[] trackVolumes;
        public float musicToFXFactor = 0.5f;

        /// <summary>
        /// At runtime call PauseMusic() or ResumeMusic()
        /// </summary>
        public bool isPaused = false;
        public int firstTrack = 1000;
        #endregion

        #region Private variables
        private int nextTrack = 0;
        private float trackLength = 0f;
        private float trackLengthTimer = 0f;
        private int previousTrack = 0;

        private float musicVolume = 0f;
        private float gameVolume = 0f;
        private float maxMusicVolume = 0f;

        private AudioSource audioSource;
        private GameObject optionsPanel;

        private float realDeltaTime;
        private bool isChangingTracks = false;
        private float newVolume = 0f;

        private bool isInitialised = false;
        private int numMusicTracks = 0;
        private int numTrackVolumes = 0;
        #endregion

        #region Initialise Methods
        // Use this for initialization
        public void Initialise()
        {
            // Make sure there is only one instance of this object in the game at one time
            if (musicController == null)
            {
                musicController = this;
                isInitialised = false;

                numMusicTracks = musicTracks == null ? 0 : musicTracks.Length;
                numTrackVolumes = trackVolumes == null ? 0 : trackVolumes.Length;

                if (numMusicTracks < 1)
                {
                    #if UNITY_EDITOR
                    Debug.Log("INFO: MusicController Initialise - no music tracks supplied");
                    #endif
                }
                else if (numMusicTracks != numTrackVolumes)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: MusicController Initialise - number of music tracks does not match number of track volumes");
                    #endif
                }
                else
                {
                    nextTrack = Random.Range(0, musicTracks.Length);
                    if (firstTrack != 1000)
                    {
                        nextTrack = firstTrack;
                    }
                    audioSource = gameObject.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.ignoreListenerVolume = true;

                        // Make 2D sound so it can be heard equally everywhere in the scene.
                        audioSource.spatialBlend = 0;

                        musicVolume = initialMusicVolume;
                        gameVolume = initialGameVolume;

                        audioSource.volume = musicVolume;
                        AudioListener.volume = gameVolume;
                        //Debug.Log ("music vol: " + audioSource.volume.ToString() + " al vol: " + AudioListener.volume);
                        isInitialised = true;
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        Debug.LogWarning("ERROR: MusicController Initialise could not find AudioSource");
                    }
                    #endif
                }
            }
            else if (musicController != this)
            {
                Destroy(musicController);
            }
        }

        #endregion

        #region Update Methods
        // Update is called once per frame
        void Update()
        {
            if (isChangingTracks || !isInitialised || isPaused)  { return; }

            if (nextTrack == 0)
            {
                previousTrack = trackVolumes.Length - 1;
            }
            else
            {
                previousTrack = nextTrack - 1;
            }

            maxMusicVolume = trackVolumes[previousTrack] * musicVolume;

            // The music is louder than the (ship??) audio (SoundFX), so reduce the music accordingly
            maxMusicVolume *= musicToFXFactor;

            if (Time.timeScale > 0f && audioSource != null)
            {
                realDeltaTime = Time.deltaTime / Time.timeScale;
                trackLengthTimer += realDeltaTime;
                if (trackLengthTimer > trackLength)
                {
                    PlayNextTrack();
                }
                else if (trackLengthTimer > trackLength - 3 && audioSource.volume > Time.deltaTime)
                {
                    newVolume = audioSource.volume - (realDeltaTime / 3f * maxMusicVolume);
                    audioSource.volume = Mathf.Clamp(newVolume, 0f, 1f);
                }
                else if (audioSource.volume < maxMusicVolume)
                {
                    newVolume = audioSource.volume + (realDeltaTime / 3f * maxMusicVolume);
                    audioSource.volume = Mathf.Clamp(newVolume, 0f, 1f);
                }
                else if (audioSource.clip != null && !audioSource.isPlaying)
                {
                    PlayNextTrack();
                }
            }
        }
        #endregion

        #region Public API Methods


        public void PauseMusic()
        {
            if (isInitialised && musicTracks != null && audioSource != null && audioSource.clip != null && audioSource.isPlaying)
            {
                audioSource.Pause();
                isPaused = true;
            }
        }

        public void ResumeMusic()
        {
            if (isInitialised && musicTracks != null && audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
                isPaused = false;
            }
        }

        public void SetMusicVolume(float newValue)
        {
            if (newValue < 0f) { musicVolume = 0f; }
            else if (newValue > 1f) { musicVolume = 1f; }
            else { musicVolume = newValue; }
        }

        public void SetGameVolume(float newValue)
        {
            if (newValue < 0f) { gameVolume = 0f; }
            else if (newValue > 1f) { gameVolume = 1f; }
            else { gameVolume = newValue; }
        }

        public void PlayNextTrack()
        {
            if (!isChangingTracks && isInitialised && musicTracks != null && audioSource != null && trackVolumes != null)
            {
                isChangingTracks = true;

                audioSource.Stop();

                if (musicTracks[nextTrack] != null)
                {
                    audioSource.clip = musicTracks[nextTrack];
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: MusicController PlayNextTrack could not get audio clip: " + nextTrack.ToString());
                }
                #endif

                audioSource.volume = 0f;

                if (audioSource.clip != null)
                {
                    trackLength = audioSource.clip.length;
                    trackLengthTimer = 0;
                    audioSource.Play();
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: MusicController PlayNextTrack audio clip is not defined for track: " + nextTrack.ToString());
                }
                #endif

                // Setup the next track for when this one has finished playing
                nextTrack += 1;
                if (nextTrack > musicTracks.Length - 1)
                {
                    nextTrack = 0;
                }

                isChangingTracks = false;
            }
        }
        #endregion
    }
}