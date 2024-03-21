using UnityEngine;
using UnityEngine.UI;

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////// Advanced Analogical Panel Script - Version 1.0.190821 - Unity 2018.3.4f1 - Maloke Games 2019
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class AnalogAdvancedScript : MonoBehaviour
{
    public static AnalogAdvancedScript current;


    //Config Variables
    [Header("Aircraft Analog")]
    public bool isActive = false;

    [Tooltip("Link your Aircraft Transform here!")] public Transform aircraft;
    [Tooltip("If your Aircraft have a RigidBody, link it here!")] public Rigidbody aircraftRB;

    [Space]
    public string activeMsg = "Instruments Activated";
    public DisplayMsg consoleMsg;
    public RectTransform analogPanel;    
    //

    [Space(5)]
    [Header("Roll")]
    public bool useRoll = true;
    public float rollAmplitude = 1, rollOffSet = 0;
    [Range(0, 1)] public float rollFilterFactor = 0.25f;
    public RectTransform horizonRoll;
    public Text horizonRollTxt;

    [Space(5)]
    [Header("Pitch")]
    public bool usePitch = true;
    public float pitchAmplitude = 1, pitchOffSet = 0, pitchXOffSet = 0, pitchYOffSet = 0;
    [Range(0, 1)] public float pitchFilterFactor = 0.125f;
    public RectTransform horizonPitch;
    public Text horizonPitchTxt;

    [Space(5)]
    [Header("Heading & TurnRate")]
    public bool useHeading = true;
    public float headingAmplitude = 1, headingOffSet = 0;
    [Range(0, 1)] public float headingFilterFactor = 0.1f;
    public RectTransform compassHSI;
    public Text headingTxt;
    public CompassBar compassBar;
    public RollDigitIndicator headingRollDigit;


    [Space]
    public bool useTurnRate = true;
    public float turnRateAmplitude = 1, turnRateOffSet = 0;
    [Range(0, 1)] public float turnRateFilterFactor = 0.1f;
    public Text turnRateTxt;
    public PointerIndicator turnRatePointer;


    [Space(5)]
    [Header("Altitude")]
    public bool useAltitude = true;
    public float altitudeAmplitude = 1, altitudeOffSet = 0;
    [Range(0, 1)] public float altitudeFilterFactor = 0.05f;
    public RollDigitIndicator altitudeRollDigit;
    public PointerIndicator altitudePointer;
    public Text altitudeTxt;

    [Space]
    public bool useRadarAltitude = false;
    public LayerMask radarLayer;// = LayerMask.NameToLayer("Default");
    public bool absoluteMode = true;
    public float maxRadarAltitude = 1000;

    [Space]
    public bool useSeparatedRadarAltitude = false;
    public RollDigitIndicator radarAltitudeRollDigit;
    public PointerIndicator radarAltitudePointer;
    public Text radarAltitudeTxt;


    [Space(5)]
    [Header("Speed")]
    public bool useSpeed = true;
    public float speedAmplitude = 1, speedOffSet = 0;
    [Range(0, 1)] public float speedFilterFactor = 0.25f;
    public NeedleIndicator speedNeedle;
    public RollDigitIndicator speedRollDigit;
    public PointerIndicator speedPointer;
    public Text speedTxt;

    [Space(5)]
    [Header("Vertical Velocity")]
    public bool useVV = true;
    public float vvAmplitude = 1, vvOffSet = 0;
    [Range(0, 1)] public float vvFilterFactor = 0.1f;
    public NeedleIndicator vvNeedle;
    public ArrowIndicator vvArrow;
    public RollDigitIndicator vvRollDigit;
    public bool roundVV = true, showDecimalVV = true;
    public float roundFactorVV = 0.1f;
    public Text verticalSpeedTxt;

    [Space(5)]
    [Header("Horizontal Velocity")]
    public bool useHV = true;
    public float hvAmplitude = 1, hvOffSet = 0;
    [Range(0, 1)] public float hvFilterFactor = 0.1f;
    public NeedleIndicator hvNeedle;
    public ArrowIndicator hvArrow;
    public bool roundHV = true, showDecimalHV = true;
    public float roundFactorHV = 0.1f;
    public Text horizontalSpeedTxt;


    [Space(5)]
    [Header("G-Force")]
    public bool useGForce = true;
    public float gForceAmplitude = 1, gForceOffSet = 0;
    [Range(0, 1)] public float gForceFilterFactor = 0.25f;
    public Text gForceTxt, maxGForceTxt, minGForceTxt;


    [Space(5)]
    [Header("AOA, AOS and GlidePath")]
    public bool useAlphaBeta = true;
    public float alphaAmplitude = 1, alphaOffSet = 0;
    [Range(0, 1)] public float alphaFilterFactor = 0.25f;
    public NeedleIndicator alphaNeedle;
    public ArrowIndicator alphaArrow;
    public Text alphaTxt;

    [Space]
    public float betaAmplitude = 1;
    public float betaOffSet = 0;
    [Range(0, 1)] public float betaFilterFactor = 0.25f;
    public NeedleIndicator betaNeedle;
    public ArrowIndicator betaArrow;
    public Text betaTxt;

    [Space]
    public bool useGlidePath = true;
    [Range(0, 1)] public float glidePathFilterFactor = 0.1f;
    public float glideXDeltaClamp = 600f, glideYDeltaClamp = 700f;
    public RectTransform glidePath;


    [Space(5)]
    [Header("Engine and Fuel")]
    public bool useEngine = true;
    public float engineAmplitude = 1;
    [Range(-1, 1)] public float engineOffSet = 0;
    [Range(0, 1)] public float engineFilterFactor = 0.0125f;
    public PointerIndicator enginePointer;
    public RollDigitIndicator engineRollDigit;
    public Text engineTxt;

    [Space]
    public bool useFuel = true;
    public float fuelAmplitude = 1;//, fuelOffSet = 0;
    [Range(0, 1)] public float fuelFilterFactor = 0.0125f;
    public PointerIndicator fuelPointer;
    public RollDigitIndicator fuelRollDigit;
    public Text fuelTxt, fuelFlowTxt;


    [Space]
    [Header("External Controlers")]
    [Range(0, 1)] public float engineTarget = 0.75f;
    [Tooltip("If non zero, Engine RPM will automaticaly follow the current Speed and will be 100% at this speed value.")] public float followMaxSpeed = 0;
    public AudioSource EngineAS;
    public float minPitch = 0.25f, maxPitch = 2.0f;

    [Space]
    [Range(0, 3)] public float fuelTarget = 0.8f;
    [Tooltip("Percentage of Consumed Fuel per Minute at Máx Engine RPM")]
    public float maxfuelFlow = 1.00f;
    [Tooltip("Min Percentage of consumed Fuel per minute at any Engine RPM")]
    public float idlefuelFlow = 0.25f;


    //////[Space]
    //////public bool speedOverride = false;
    //////public float speedTarget = 0f;



    //All Flight Variables
    [Space(10)] [Header("Flight Variables - ReadOnly!")] 
    public float speed;
    public float altitude, radarAltitude, pitch, roll, heading, turnRate, gForce, maxGForce, minGForce, alpha, beta, vv, hv, engine, fuel, fuelFlow;
    //


    //Internal Calculation Variables
    Vector3 currentPosition, lastPosition, relativeSpeed, absoluteSpeed, lastSpeed, relativeAccel;

    Vector3 angularSpeed;
    Quaternion currentRotation, lastRotation, deltaTemp;
    float angleTemp = 0.0f;
    Vector3 axisTemp = Vector3.zero;

    float engineReNormalized, fuelReNormalized;

    int waitInit = 6;
    //

    //Set Default Values via Editor -> This will be implemented in future updates
    //[ContextMenu("Default Simulation")] void setDefaultSimulation() { Debug.Log("Default Simulation!"); }
    //

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Inicialization
    void Awake()
    {
        if (aircraft == null && aircraftRB == null) aircraft = Camera.main.transform;   //If there is no reference set, then it gets the MainCamera
        if (aircraft == null && aircraftRB != null) aircraft = aircraftRB.transform;
    }
    void OnEnable()
    {
        if (aircraft == null && aircraftRB == null) aircraft = Camera.main.transform;
        ResetHud();
    }
    public void ResetHud()
    {
        current = this;
        if (aircraft == null && aircraftRB != null) aircraft = aircraftRB.transform;

        waitInit = 6;
        maxGForce = 1f; minGForce = 1f;
        if (useGForce) { if (maxGForceTxt != null) maxGForceTxt.text = "0.0"; if (minGForceTxt != null) minGForceTxt.text = "0.0"; }


        isActive = true;
        if (consoleMsg != null) DisplayMsg.current = consoleMsg;
        if (activeMsg != "") DisplayMsg.show(activeMsg, 5);

    }
    public void toogleHud()
    {
        SndPlayer.playClick();
        analogPanel.gameObject.SetActive(!analogPanel.gameObject.activeSelf);


        if (!analogPanel.gameObject.activeSelf)
        {
            isActive = false; current = null;
            DisplayMsg.show("Hud Disabled", 5);
        }
        else { if (!isActive) ResetHud(); }
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Inicialization



    /////////////////////////////////////////////////////// Updates and Calculations
    void FixedUpdate() //Update()
    {
        // Return if not active
        if (!isActive || !analogPanel.gameObject.activeSelf) return;
        if (aircraft == null) { isActive = false; return; }

        //////////////////////////////////////////// Frame Calculations
        lastPosition = currentPosition;
        lastSpeed = relativeSpeed;
        lastRotation = currentRotation;

        if (aircraft != null && aircraftRB == null) //Mode Transform
        {
            currentPosition = aircraft.transform.position;
            absoluteSpeed = (currentPosition - lastPosition) / Time.fixedDeltaTime;
            relativeSpeed = aircraft.transform.InverseTransformDirection((currentPosition - lastPosition) / Time.fixedDeltaTime);
            relativeAccel = (relativeSpeed - lastSpeed) / Time.fixedDeltaTime;
            currentRotation = aircraft.transform.rotation;

            //angular speed
            deltaTemp = currentRotation * Quaternion.Inverse(lastRotation);
            angleTemp = 0.0f;
            axisTemp = Vector3.zero;
            deltaTemp.ToAngleAxis(out angleTemp, out axisTemp);
            //
            angularSpeed = aircraft.InverseTransformDirection(angleTemp * axisTemp) * Mathf.Deg2Rad / Time.fixedDeltaTime;
            //
        }
        else if (aircraft != null && aircraftRB != null)  //Mode RB
        {
            currentPosition = aircraftRB.transform.position;
            absoluteSpeed = (currentPosition - lastPosition) / Time.fixedDeltaTime;
            relativeSpeed = aircraftRB.transform.InverseTransformDirection(aircraftRB.velocity);
            relativeAccel = (relativeSpeed - lastSpeed) / Time.fixedDeltaTime;
            currentRotation = aircraft.transform.rotation;

            angularSpeed = aircraftRB.angularVelocity;
        }
        else //Zero all values
        {
            currentPosition = Vector3.zero;
            relativeSpeed = Vector3.zero;
            relativeAccel = Vector3.zero;
            angularSpeed = Vector3.zero;

            lastPosition = currentPosition;
            lastSpeed = relativeSpeed;
            lastRotation = currentRotation;
        }
        //
        if (waitInit > 0) { waitInit--; return; } //Wait some frames for stablization before starting calculating
        //
        //////////////////////////////////////////// Frame Calculations


        //////////////////////////////////////////// Compass, Heading and/or HSI + Turn Rate
        if (useHeading)
        {
            heading = Mathf.LerpAngle(heading, headingAmplitude * aircraft.eulerAngles.y + headingOffSet, headingFilterFactor) % 360f;

            //Send values to Gui and Instruments
            if (compassHSI != null) compassHSI.localRotation = Quaternion.Euler(0, 0, heading);
            if (compassBar != null) compassBar.setValue(heading);
            if (headingRollDigit != null) headingRollDigit.setValue((heading < 0) ? (heading + 360f) : heading);
            if (headingTxt != null) { if (heading < 0) headingTxt.text = (heading + 360f).ToString("000"); else headingTxt.text = heading.ToString("000"); }

        }
        //
        if (useTurnRate)
        {
            ////// Mode: World Coorditate
            //turnRate = Mathf.LerpAngle(turnRate, turnRateOffSet + turnRateAmplitude * Mathf.DeltaAngle(lastRotation.eulerAngles.y, currentRotation.eulerAngles.y) / Time.fixedDeltaTime, turnRateFilterFactor) % 360f;
            //turnRate = Mathf.Round(100f * turnRate + 0.5f) / 100f;

            // Mode: Relative to Aircraft
            turnRate = Mathf.LerpAngle(turnRate, turnRateOffSet + turnRateAmplitude * (angularSpeed.y - 0.05f * angularSpeed.z) * Mathf.Rad2Deg, turnRateFilterFactor) % 360f;
            turnRate = Mathf.Round(100f * turnRate + 0.5f) / 100f;
            //////

            //Send values to Gui and Instruments
            if (turnRatePointer != null) turnRatePointer.setValue(turnRate);
            if (turnRateTxt != null) { turnRateTxt.text = turnRate.ToString("0"); }
        }
        //////////////////////////////////////////// Compass, Heading and/or HSI + Turn Rate


        //////////////////////////////////////////// Roll
        if (useRoll)
        {
            roll = Mathf.LerpAngle(roll, aircraft.rotation.eulerAngles.z + rollOffSet, rollFilterFactor) % 360;

            //Send values to Gui and Instruments
            if (horizonRoll != null) horizonRoll.localRotation = Quaternion.Euler(0, 0, rollAmplitude * roll);
            if (horizonRollTxt != null)
            {
                //horizonRollTxt.text = roll.ToString("##");
                if (roll > 180) horizonRollTxt.text = (roll - 360).ToString("00");
                else if (roll < -180) horizonRollTxt.text = (roll + 360).ToString("00");
                else horizonRollTxt.text = roll.ToString("00");
            }
            //
        }
        //////////////////////////////////////////// Roll


        //////////////////////////////////////////// Pitch
        if (usePitch)
        {
            pitch = Mathf.LerpAngle(pitch, -aircraft.eulerAngles.x + pitchOffSet, pitchFilterFactor);

            //Send values to Gui and Instruments
            if (horizonPitch != null) horizonPitch.localPosition = new Vector3(-pitchAmplitude * pitch * Mathf.Sin(horizonPitch.transform.localEulerAngles.z * Mathf.Deg2Rad) + pitchXOffSet, pitchAmplitude * pitch * Mathf.Cos(horizonPitch.transform.localEulerAngles.z * Mathf.Deg2Rad) + pitchYOffSet, 0);
            if (horizonPitchTxt != null) horizonPitchTxt.text = pitch.ToString("0");
        }
        //////////////////////////////////////////// Pitch


        //////////////////////////////////////////// Altitude + RadarAltitude
        if (useAltitude)
        {
            if (!useRadarAltitude || (useRadarAltitude && useSeparatedRadarAltitude)) altitude = Mathf.Lerp(altitude, altitudeOffSet + altitudeAmplitude * currentPosition.y, altitudeFilterFactor);
            else
            {
                RaycastHit hit;
                if (Physics.Linecast(aircraft.position, (absoluteMode) ? aircraft.position + Vector3.down * maxRadarAltitude : aircraft.position - aircraft.transform.up * maxRadarAltitude, out hit, radarLayer))
                {
                    altitude = Mathf.Lerp(altitude, altitudeOffSet + altitudeAmplitude * hit.distance, altitudeFilterFactor);
                }
                else altitude = Mathf.Lerp(altitude, altitudeOffSet + altitudeAmplitude * maxRadarAltitude, altitudeFilterFactor);

                radarAltitude = altitude;
            }

            //Send values to Gui and Instruments (Altitude)
            if (altitudeRollDigit != null) altitudeRollDigit.setValue(altitude);
            if (altitudePointer != null) altitudePointer.setValue(altitude);
            if (altitudeTxt != null) altitudeTxt.text = altitude.ToString("0").PadLeft(5);
        }

        //RadarAltitude as a Separeted Instrument
        if (useRadarAltitude && useSeparatedRadarAltitude)
        {
            RaycastHit hit;
            if (Physics.Linecast(aircraft.position, (absoluteMode) ? aircraft.position + Vector3.down * maxRadarAltitude : aircraft.position - aircraft.transform.up * maxRadarAltitude, out hit, radarLayer))
            {
                radarAltitude = Mathf.Lerp(radarAltitude, altitudeOffSet + altitudeAmplitude * hit.distance, altitudeFilterFactor);
            }
            else radarAltitude = Mathf.Lerp(radarAltitude, altitudeOffSet + altitudeAmplitude * maxRadarAltitude, altitudeFilterFactor);

            //Send values to Gui and Instruments (Radar-Altitude)
            if (radarAltitudeRollDigit != null) radarAltitudeRollDigit.setValue(radarAltitude);
            if (radarAltitudePointer != null) radarAltitudePointer.setValue(radarAltitude);
            if (radarAltitudeTxt != null) radarAltitudeTxt.text = radarAltitude.ToString("0").PadLeft(5);
        }
        //////////////////////////////////////////// Altitude + RadarAltitude


        //////////////////////////////////////////// Speed
        if (useSpeed)
        {
            speed = Mathf.Lerp(speed, speedOffSet + speedAmplitude * relativeSpeed.z, speedFilterFactor);

            //////if (speedOverride) speed = Mathf.Lerp(speed, speedOffSet + speedTarget, speedFilterFactor);
            //////else speed = Mathf.Lerp(speed, speedOffSet + speedAmplitude * relativeSpeed.z, speedFilterFactor);


            //Send values to Gui and Instruments
            if (speedNeedle != null) speedNeedle.setValue(speed);
            if (speedRollDigit != null) speedRollDigit.setValue(speed);            
            if (speedPointer != null) speedPointer.setValue(speed);
            if (speedTxt != null) speedTxt.text = speed.ToString("0").PadLeft(5);//.ToString("##0");
        }
        //////////////////////////////////////////// Speed


        //////////////////////////////////////////// Vertical Velocity - VV
        if (useVV)
        {
            vv = Mathf.Lerp(vv, vvOffSet + vvAmplitude * absoluteSpeed.y, vvFilterFactor);

            //Send values to Gui and Instruments
            if (vvNeedle != null) vvNeedle.setValue(vv);
            if (vvArrow != null) vvArrow.setValue(vv);
            if (vvRollDigit != null) vvRollDigit.setValue(vv);            
            if (verticalSpeedTxt != null)
            {
                if (roundVV)
                {
                    if (showDecimalVV) verticalSpeedTxt.text = (System.Math.Round(vv / roundFactorVV, System.MidpointRounding.AwayFromZero) * roundFactorVV).ToString("0.0").PadLeft(4);
                    else verticalSpeedTxt.text = (System.Math.Round(vv / roundFactorVV, System.MidpointRounding.AwayFromZero) * roundFactorVV).ToString("0").PadLeft(3);
                }
                else
                {
                    if (showDecimalVV) verticalSpeedTxt.text = (vv).ToString("0.0").PadLeft(4);
                    else verticalSpeedTxt.text = (vv).ToString("0").PadLeft(3);
                }

            }
        }
        //////////////////////////////////////////// Vertical Velocity - VV


        //////////////////////////////////////////// Horizontal Velocity - HV
        if (useHV)
        {
            hv = Mathf.Lerp(hv, hvOffSet + hvAmplitude * relativeSpeed.x, hvFilterFactor);

            //Send values to Gui and Instruments
            if (hvNeedle != null) hvNeedle.setValue(hv);
            if (hvArrow != null) hvArrow.setValue(hv);
            if (horizontalSpeedTxt != null)
            {
                if (roundHV)
                {
                    if (showDecimalHV) horizontalSpeedTxt.text = (System.Math.Round(hv / roundFactorHV, System.MidpointRounding.AwayFromZero) * roundFactorHV).ToString("0.0").PadLeft(4);
                    else horizontalSpeedTxt.text = (System.Math.Round(hv / roundFactorHV, System.MidpointRounding.AwayFromZero) * roundFactorHV).ToString("0").PadLeft(3);
                }
                else
                {
                    if (showDecimalHV) horizontalSpeedTxt.text = (hv).ToString("0.0").PadLeft(4);
                    else horizontalSpeedTxt.text = (hv).ToString("0").PadLeft(3);
                }
            }
        }
        //////////////////////////////////////////// Horizontal Velocity - HV


        //////////////////////////////////////////// Vertical G-Force 
        if (useGForce)
        {
            //G-FORCE -> Gravity + Vertical Acceleration + Centripetal Acceleration (v * w) radians
            float gTotal =
                ((-aircraft.transform.InverseTransformDirection(Physics.gravity).y +
                gForceAmplitude * (relativeAccel.y - angularSpeed.x * Mathf.Abs(relativeSpeed.z)
                )) / Physics.gravity.magnitude);

            gForce = Mathf.Lerp(gForce, gForceOffSet + gTotal, gForceFilterFactor);
            //

            //Send values to Gui and Instruments
            if (gForceTxt != null) gForceTxt.text = gForce.ToString("0.0").PadLeft(3);
            if (gForce > maxGForce)
            {
                maxGForce = gForce;
                if (maxGForceTxt != null) maxGForceTxt.text = maxGForce.ToString("0.0").PadLeft(3);
            }
            if (gForce < minGForce)
            {
                minGForce = gForce;
                if (minGForceTxt != null) minGForceTxt.text = minGForce.ToString("0.0").PadLeft(3);
            }
            //
        }
        ////////////////////////////////////////////  Vertical G-Force 


        //////////////////////////////////////////////// AOA (Alpha) + AOS (Beta) + GlidePath (Velocity Vector)
        if (useAlphaBeta || useGlidePath)
        {
            //Calculate both Angles
            alpha = Mathf.Lerp(alpha, alphaOffSet + alphaAmplitude * Vector2.SignedAngle(new Vector2(relativeSpeed.z, relativeSpeed.y), Vector2.right), alphaFilterFactor);
            beta  = Mathf.Lerp(beta, betaOffSet + betaAmplitude * Vector2.SignedAngle(new Vector2(relativeSpeed.x, relativeSpeed.z), Vector2.up), betaFilterFactor);

            ////Used in older Unity versions where Vector2.SignedAngle didnt exist
            ////int alphaSign = (int)Mathf.Sign(Vector3.Dot(Vector3.forward, Vector3.Cross(new Vector2(relativeSpeed.z, relativeSpeed.y), Vector2.right)));
            ////int betaSign = (int)Mathf.Sign(Vector3.Dot(Vector3.forward, Vector3.Cross(new Vector2(relativeSpeed.x, relativeSpeed.z), Vector2.up)));
            ////alpha = Mathf.Lerp(alpha, alphaOffSet + alphaAmplitude * alphaSign * Vector2.Angle(new Vector2(relativeSpeed.z, relativeSpeed.y), Vector2.right), alphaFilterFactor);
            ////beta = Mathf.Lerp(beta, betaOffSet + betaAmplitude * betaSign * Vector2.Angle(new Vector2(relativeSpeed.x, relativeSpeed.z), Vector2.up), betaFilterFactor);
            //

            //Apply angle values to the glidePath UI element
            if (useGlidePath && glidePath != null) glidePath.localPosition = Vector3.Lerp(glidePath.localPosition, new Vector3(Mathf.Clamp(-beta * pitchAmplitude, -glideXDeltaClamp, glideXDeltaClamp), Mathf.Clamp(alpha * pitchAmplitude, -glideYDeltaClamp, glideYDeltaClamp), 0), glidePathFilterFactor);


            //Send values to Instruments
            if (useAlphaBeta)
            {
                if (alphaNeedle != null) alphaNeedle.setValue(alpha);
                if ( alphaArrow != null) alphaArrow.setValue(alpha);
                if ( betaNeedle != null) betaNeedle.setValue(beta);
                if (  betaArrow != null) betaArrow.setValue(beta);


                ////Send lateral G-Force Instead (Under Test)
                //if (  betaArrow != null) betaArrow.setValue((gForceAmplitude * (relativeAccel.x - angularSpeed.y * Mathf.Abs(relativeSpeed.z))) / Physics.gravity.magnitude);
                ////
                

                //Send values to Gui Text
                if (alphaTxt != null) alphaTxt.text = alpha.ToString("0").PadLeft(3);
                if (betaTxt != null) betaTxt.text = beta.ToString("0").PadLeft(3);
            }
            //
        }
        //////////////////////////////////////////////// AOA (Alpha) + AOS (Beta)


        //////////////////////////////////////////// Engine & Fuel
        if (useEngine)
        {
            //Auto RPM control and Fuel Condition
            if (followMaxSpeed > 0) engineTarget = Mathf.Abs(speed / followMaxSpeed);
            if (useFuel && fuelReNormalized < 0.01f) engineTarget = 0;
            //

            //Updates current Engine RPM
            engineTarget = Mathf.Clamp01(Mathf.Abs(engineTarget));
            engine = Mathf.Lerp(engine, engineAmplitude * Mathf.Clamp01(engineTarget + engineOffSet), engineFilterFactor);

            if (engineTarget == 0 && engine < 0.01f) engine = 0;
            engineReNormalized = Mathf.Clamp01( (engine - engineOffSet) / engineAmplitude);
            //

            //Engine Sound and Pitch
            if (EngineAS != null && EngineAS.isActiveAndEnabled)
            {
                if (!EngineAS.isPlaying && engineTarget > 0) EngineAS.Play();

                if (engineReNormalized > 0.01f) EngineAS.pitch = Mathf.Lerp(minPitch, maxPitch, engineReNormalized);
                else { EngineAS.Stop(); EngineAS.pitch = 1; } //EngineAS.pitch = 0;
            }
            //

            //Send values to Gui and Instruments
            if (engineRollDigit != null) engineRollDigit.setValue(engine);
            if (enginePointer != null) enginePointer.setValue(engine);
            if (engineTxt != null) engineTxt.text = engine.ToString("##0");
        }
        //
        if (useFuel)
        {
            //Calculates Fuel Consumption
            if ( maxfuelFlow != 0 || idlefuelFlow != 0)
            {
                if (engine != 0)
                {
                    fuelFlow = Mathf.Clamp(engineReNormalized * maxfuelFlow, idlefuelFlow, maxfuelFlow) * Time.fixedDeltaTime / 0.60f;
                    fuelTarget -= fuelFlow / 100f;
                }
                else fuelFlow = 0;
            }
            else fuelFlow = 0;
            //

            //Updates current Fuel value
            if (fuelTarget < 0) fuelTarget = 0;//fuelTarget = Mathf.Clamp01(fuelTarget);
            fuel = Mathf.Lerp(fuel, /*fuelOffSet +*/ fuelAmplitude * fuelTarget, fuelFilterFactor);

            if (fuel < 0) fuel = 0;
            if (fuelTarget == 0 && fuel < 0.01f) fuel = 0;
            fuelReNormalized = fuel / fuelAmplitude; //Mathf.Clamp01(fuel /*- fuelOffSet*/) / fuelAmplitude;
            //

            //Send values to Gui and Instruments
            if (fuelRollDigit != null) fuelRollDigit.setValue(fuel);
            if (fuelPointer != null) fuelPointer.setValue(fuel);
            if (fuelTxt != null) fuelTxt.text = fuel.ToString("##0");
            if (fuelFlowTxt != null) fuelFlowTxt.text = (fuelAmplitude * fuelFlow).ToString("##0.0");//.ToString("0.0").PadLeft(4);  //.ToString("##0");     
        }
        //////////////////////////////////////////// Engine & Fuel


    }
    /////////////////////////////////////////////////////// Updates and Calculations
}





//Backup formulas
//if (hvIndicator != null) hvIndicator.localPosition = new Vector3( Mathf.Clamp(hv, -hvDeltaClamp, hvDeltaClamp), hvIndicator.localPosition.y , hvIndicator.localPosition.z);
//if (roundHV) horizontalSpeedTxt.text = (System.Math.Round(hv / roundFactorHV, System.MidpointRounding.AwayFromZero) * roundFactorHV).ToString((showDecimalHV) ? "0.0" : "0").PadLeft(4);
//else horizontalSpeedTxt.text = (hv).ToString((showDecimalHV) ? "0.0" : "0").PadLeft(4);
//if (roundVV) verticalSpeedTxt.text = (System.Math.Round(vv / roundFactorVV, System.MidpointRounding.AwayFromZero) * roundFactorVV).ToString((showDecimalVV) ? "0.0" : "0").PadLeft(4);
//else verticalSpeedTxt.text = (vv).ToString((showDecimalVV) ? "0.0" : "0").PadLeft(4);
