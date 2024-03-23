using UnityEngine;

public class SndPlayer : MonoBehaviour
{
    public static SndPlayer current;
    public AudioSource som;

    public bool isMaster = true, playOnEnable = false;
    public int enableIndex = 0;

    public bool mute = false;

    public float volume = 1;
    [Tooltip("0 - Hover / 1 - Click / 2 - Effect")] public AudioClip[] sounds;


    ///////////// Inicialization
    void Awake() { if (som == null) som = GetComponent<AudioSource>(); }
    void OnEnable()
    {
        if (gameObject.activeInHierarchy && (current == null || isMaster) ) current = this;
        if(playOnEnable) indexSnd(enableIndex);
    }
    /////////////

    /////////////// Check if sound is mute
    bool checkIsMute()
    {
        //mute = SettingsControl.muteSound; //// Used in other Assets
        return mute;
    }
    ///////////////


    ////////////////////////////////////////////////////////////////////////
    /////////// Play Sounds
    ////////////////////////////////////////////////////////////////////////
    public void playClip(AudioClip clip, float volume = 1f) { if (!checkIsMute()) som.PlayOneShot(clip, volume); }
    public void indexSnd(int index) { if (!checkIsMute()) SndPlayer.current.som.PlayOneShot(SndPlayer.current.sounds[index], SndPlayer.current.volume); }
    public void enterGuiSnd() { if (!checkIsMute()) som.PlayOneShot(sounds[0], volume); }           //0 - HOVER
    public void clickGuiSnd() { if (!checkIsMute()) som.PlayOneShot(sounds[1], volume); }           //1 - Click
    public void effectSnd() { if (!checkIsMute()) som.PlayOneShot(sounds[2], volume); }             //2 - GuiEffect
    ////////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////////
    /////////  External Static Calls
    ////////////////////////////////////////////////////////////////////////
    public static void play(AudioClip clip, float volume = 1f) { if (current != null) SndPlayer.current.playClip(clip, volume); }
    public static void play(int index) { if (current != null) SndPlayer.current.indexSnd(index); }//{ SndPlayer.current.som.PlayOneShot(SndPlayer.current.sounds[index], SndPlayer.current.volume); }
    public static void playHover() { if (current != null) SndPlayer.current.enterGuiSnd(); }
    public static void playClick() { if (current != null) SndPlayer.current.clickGuiSnd(); }
    public static void playEffect() { if (current != null) SndPlayer.current.effectSnd(); }
    ////////////////////////////////////////////////////////////////////////
}





////// Used in other Assets
////public Toggle muteToggle;
////public GameObject musicObj;
////public void toogleMusic()
////{
////    if (musicObj != null)
////    {
////        musicObj.SetActive(!musicObj.activeSelf);
////        if (musicObj.activeSelf) DisplayMsg.show("Engine Sound Enabled", 5); else DisplayMsg.show("Engine Sound Disabled", 5);
////    }
////}
//////
