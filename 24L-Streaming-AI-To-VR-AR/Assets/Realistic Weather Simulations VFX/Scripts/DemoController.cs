using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class DemoController : MonoBehaviour {
    //variables 
    #region
    //Rain
    public GameObject rain_Drizzle;
    public GameObject rain_Light;
    public GameObject rain_Steady;
    public GameObject rain_Downpour;
    //Snow 
    public GameObject snow_Light;
    public GameObject snow_Steady;
    public GameObject snow_Heavy;
    public GameObject snow_Blizzard;
    //Clouds
    public GameObject clouds1;
    public GameObject clouds2;
    public GameObject clouds3;
    public GameObject clouds4;
    public GameObject clouds5;
    public GameObject clouds6;
    //Fog
    public GameObject fog1;
    public GameObject fog2;
    //Wind
    public GameObject windLight;
    public GameObject windStrong;
    #endregion 

    // Use this for initialization
    void Start ()
    {
        rain_Drizzle.SetActive(true);
        clouds3.SetActive(true);
	}
    //Set Active Rain
    #region
    public void RainDrizzle()
    {
        rain_Drizzle.SetActive(true);
        rain_Light.SetActive(false);
        rain_Steady.SetActive(false);
        rain_Downpour.SetActive(false);

        snow_Light.SetActive(false);
        snow_Steady.SetActive(false);
        snow_Heavy.SetActive(false);
        snow_Blizzard.SetActive(false);
    }

    public void RainLight()
    {
        rain_Drizzle.SetActive(false);
        rain_Light.SetActive(true);
        rain_Steady.SetActive(false);
        rain_Downpour.SetActive(false);

        snow_Light.SetActive(false);
        snow_Steady.SetActive(false);
        snow_Heavy.SetActive(false);
        snow_Blizzard.SetActive(false);
    }

    public void RainSteady()
    {
        rain_Drizzle.SetActive(false);
        rain_Light.SetActive(false);
        rain_Steady.SetActive(true);
        rain_Downpour.SetActive(false);

        snow_Light.SetActive(false);
        snow_Steady.SetActive(false);
        snow_Heavy.SetActive(false);
        snow_Blizzard.SetActive(false);
    }

    public void RainDownpour()
    {
        rain_Drizzle.SetActive(false);
        rain_Light.SetActive(false);
        rain_Steady.SetActive(false);
        rain_Downpour.SetActive(true);

        snow_Light.SetActive(false);
        snow_Steady.SetActive(false);
        snow_Heavy.SetActive(false);
        snow_Blizzard.SetActive(false);
    }

    public void RainOff()
    {
        rain_Drizzle.SetActive(false);
        rain_Light.SetActive(false);
        rain_Steady.SetActive(false);
        rain_Downpour.SetActive(false);

        snow_Light.SetActive(false);
        snow_Steady.SetActive(false);
        snow_Heavy.SetActive(false);
        snow_Blizzard.SetActive(false);
    }
    #endregion
    //Set Active Snow
    #region
    public void SnowLight()
    {
        rain_Drizzle.SetActive(false);
        rain_Light.SetActive(false);
        rain_Steady.SetActive(false);
        rain_Drizzle.SetActive(false);

        snow_Light.SetActive(true);
        snow_Steady.SetActive(false);
        snow_Heavy.SetActive(false);
        snow_Blizzard.SetActive(false);
    }

    public void SnowSteady()
    {
        rain_Drizzle.SetActive(false);
        rain_Light.SetActive(false);
        rain_Steady.SetActive(false);
        rain_Drizzle.SetActive(false);

        snow_Light.SetActive(false);
        snow_Steady.SetActive(true);
        snow_Heavy.SetActive(false);
        snow_Blizzard.SetActive(false);
    }

    public void SnowHeavy()
    {
        rain_Drizzle.SetActive(false);
        rain_Light.SetActive(false);
        rain_Steady.SetActive(false);
        rain_Drizzle.SetActive(false);

        snow_Light.SetActive(false);
        snow_Steady.SetActive(false);
        snow_Heavy.SetActive(true);
        snow_Blizzard.SetActive(false);
    }

    public void SnowBlizzard()
    {
        rain_Drizzle.SetActive(false);
        rain_Light.SetActive(false);
        rain_Steady.SetActive(false);
        rain_Drizzle.SetActive(false);

        snow_Light.SetActive(false);
        snow_Steady.SetActive(false);
        snow_Heavy.SetActive(false);
        snow_Blizzard.SetActive(true);
    }

    public void SnowOff()
    {
        rain_Drizzle.SetActive(false);
        rain_Light.SetActive(false);
        rain_Steady.SetActive(false);
        rain_Downpour.SetActive(false);

        snow_Light.SetActive(false);
        snow_Steady.SetActive(false);
        snow_Heavy.SetActive(false);
        snow_Blizzard.SetActive(false);
    }
    #endregion
    //Set Active Clouds
    #region
    public void CloudsOne()
    {
        clouds1.SetActive(true);
        clouds2.SetActive(false);
        clouds3.SetActive(false);
        clouds4.SetActive(false);
        clouds5.SetActive(false);
        clouds6.SetActive(false);
    }

    public void CloudsTwo()
    {
        clouds1.SetActive(false);
        clouds2.SetActive(true);
        clouds3.SetActive(false);
        clouds4.SetActive(false);
        clouds5.SetActive(false);
        clouds6.SetActive(false);
    }

    public void CloudsThree()
    {
        clouds1.SetActive(false);
        clouds2.SetActive(false);
        clouds3.SetActive(true);
        clouds4.SetActive(false);
        clouds5.SetActive(false);
        clouds6.SetActive(false);
    }

    public void CloudsFour()
    {
        clouds1.SetActive(false);
        clouds2.SetActive(false);
        clouds3.SetActive(false);
        clouds4.SetActive(true);
        clouds5.SetActive(false);
        clouds6.SetActive(false);
    }

    public void CloudsFive()
    {
        clouds1.SetActive(false);
        clouds2.SetActive(false);
        clouds3.SetActive(false);
        clouds4.SetActive(false);
        clouds5.SetActive(true);
        clouds6.SetActive(false);
    }

    public void CloudsSix()
    {
        clouds1.SetActive(false);
        clouds2.SetActive(false);
        clouds3.SetActive(false);
        clouds4.SetActive(false);
        clouds5.SetActive(false);
        clouds6.SetActive(true);
    }
    #endregion
    //Set Active Fog
    #region
    public void FogOne()
    {
        fog1.SetActive(true);
        fog2.SetActive(false);
    }

    public void FogTwo()
    {
        fog1.SetActive(false);
        fog2.SetActive(true);
    }

    public void FogOff()
    {
        fog1.SetActive(false);
        fog2.SetActive(false);
    }
    #endregion
    //Set Active Wind
    #region
    public void LightWind()
    {
        windLight.SetActive(true);
        windStrong.SetActive(false);
    }

    public void StrongWind()
    {
        windLight.SetActive(false);
        windStrong.SetActive(true);
    }

    public void NoWind()
    {
        windLight.SetActive(false);
        windStrong.SetActive(false);
    }
}
#endregion