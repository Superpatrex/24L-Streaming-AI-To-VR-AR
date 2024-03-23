using UnityEngine;


namespace Core3lb
{
    public class URLOpener : MonoBehaviour
    {
        public string theURL;
        public void _OpenURLName(string chg)
        {
            Application.OpenURL(chg);
        }

        public void _OpenURL()
        {
            Application.OpenURL(theURL);
        }
    }
}
