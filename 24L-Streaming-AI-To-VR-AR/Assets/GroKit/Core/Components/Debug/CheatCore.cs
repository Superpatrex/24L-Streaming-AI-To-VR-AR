using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Core3lb
{
    [System.Serializable]
    public class KeyWithEvent
    {
        public KeyCode myCode;
        public UnityEvent theEvent;
    }

    public class CheatCore : MonoBehaviour
    {
        public bool cheatsEnabled = true;
        public bool persistant = false;
        public KeyWithEvent[] myCheats;
        public bool numbersLoadScene = true;
        public bool runEventHolder = false;


        private void Start()
        {
            if (persistant)
            {
                DontDestroyOnLoad(this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!cheatsEnabled)
            {
                return;
            }
            if (Application.isEditor || Input.GetKey(KeyCode.LeftShift))
            {
                //if (Input.GetKeyDown(KeyCode.W))
                //{
                //    //PUT CHEAT HERE! EXAMPLE
                //}
                for (int i = 0; i < myCheats.Length; i++)
                {
                    if (Input.GetKeyDown(myCheats[i].myCode))
                    {
                        myCheats[i].theEvent.Invoke();
                    }
                }
            }
            if (numbersLoadScene)
            {
                LevelKeyCheats();
            }
        }
        public void LevelKeyCheats()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SceneManager.LoadScene(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SceneManager.LoadScene(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SceneManager.LoadScene(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SceneManager.LoadScene(4);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SceneManager.LoadScene(5);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SceneManager.LoadScene(6);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                SceneManager.LoadScene(7);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                SceneManager.LoadScene(8);
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                SceneManager.LoadScene(9);
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}
