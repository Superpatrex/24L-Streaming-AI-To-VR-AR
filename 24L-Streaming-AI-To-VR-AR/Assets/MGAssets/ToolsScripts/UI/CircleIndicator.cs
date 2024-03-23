using UnityEngine;

public class CircleIndicator : MonoBehaviour
{
    public bool isActive = true;

    [Space]
    public RectTransform ball;
    public Vector2 value;
    public float factor = 1;

    [Space]
    public bool clampCircle = true;
    public bool clampSquare = false;
    public float maxDistance = 80f;


    //Update to Inicial Value
    void OnEnable() { if (isActive && gameObject.activeInHierarchy) setValue(value); }
    //

    //External call to set value
    public void setValue(Vector2 newValue)
    {
        if (!isActive || ball == null) return;

        //Update Value
        value = factor * newValue;

        //Update GUI ball position
        if (clampCircle) ball.localPosition = Vector2.ClampMagnitude(value * maxDistance, maxDistance);
        else if (clampSquare) ball.localPosition = new Vector2(Mathf.Clamp( value.x * maxDistance, -maxDistance, maxDistance), Mathf.Clamp(value.y * maxDistance, -maxDistance, maxDistance)); 
        else ball.localPosition = value * maxDistance;
    }
    //



}
