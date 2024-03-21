using UnityEngine;
using UnityEngine.SceneManagement;

namespace MGAssets
{

    public class DebugTools : MonoBehaviour
    {
        public static DebugTools current;

        public bool isActive = true, singleton = false, useTabCursor = false, useF7Restart = true, useF1Quit = true;
        public int awakeFrameRate = 60;
        public int superSize = 2;

        //public SndPlayer sndPlayer;
        //public DisplayMsg displayMsg;

        //
        void Awake()
        {
            if (isActive)
            {
                // Singleton - Single Instance
                if (singleton)
                {
                    if (current != null) { DestroyImmediate(gameObject); return; }
                    DontDestroyOnLoad(gameObject);
                    current = this;
                }
                //

                Application.targetFrameRate = awakeFrameRate;
            }
        }
        void Update()
        {
            //Disable component if is not active
            if (!isActive) { this.enabled = false; return; }
            //

            //Quit Playmode by pressing F1
            if (useF1Quit && Input.GetKeyDown(KeyCode.F1))
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
            //

            //Restart Game
            if (useF7Restart && Input.GetKeyDown(KeyCode.F7)) SceneManager.LoadScene(0);
            //

            //Cursor lock-unlock with Tab key
            if (useTabCursor && Input.GetKeyDown(KeyCode.Tab))
            {
                if (Cursor.lockState != CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
                else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

                print("Cursor = " + Cursor.lockState);
            }
            //


            //Changes FrameRate
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.Delete))
            {
                if (Application.targetFrameRate == 60) { Application.targetFrameRate = 120; }
                else if (Application.targetFrameRate == 120) { Application.targetFrameRate = -1; }
                else if (Application.targetFrameRate == -1) { Application.targetFrameRate = 15; }
                else if (Application.targetFrameRate == 15) { Application.targetFrameRate = 30; }
                else if (Application.targetFrameRate == 30) { Application.targetFrameRate = 60; }

                //SndPlayer.playClick(); //if (sndPlayer != null) sndPlayer.clickGuiSnd();
                DisplayMsg.showAll("FPS = " + Application.targetFrameRate, 5); //if (displayMsg != null) displayMsg.displayQuickMsg("FPS = " + Application.targetFrameRate);

                print("FPS = " + Application.targetFrameRate);
            }
            //


            //Show current FrameRate on console
            if (Input.GetKeyDown(KeyCode.N) && Input.GetKey(KeyCode.Delete))
            {
                DisplayMsg.showAll("Current FPS = " + (1 / Time.deltaTime)); //if (displayMsg != null) displayMsg.displayQuickMsg("Current FPS = " + (1 / Time.deltaTime));
                print("Current FPS = " + 1 / Time.deltaTime);
            }
            //


            /////////// Screenshots
            if (Input.GetKeyDown(KeyCode.Return) && Input.GetKey(KeyCode.Delete))
            {
                //ScreenCapture.CaptureScreenshot("Screenshot_" + contagem + ".png", superSize);
                //contagem++;

                //YYYYmmddHHMMSSfff -> Mode with Date+Hour+Minutes+Seconds+Miliseconds
                ScreenCapture.CaptureScreenshot("Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd-hhmmss-fff") + ".png", superSize);
                print("Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd-hhmmss-MS-fff") + ".png");

                //SndPlayer.playClick();
            }
            ///////////


            if (Input.GetKey(KeyCode.Delete) && Input.GetKeyDown(KeyCode.Insert)) ClearConsole();

        }
        //



        //
        static void ClearConsole()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
        //

    }
}