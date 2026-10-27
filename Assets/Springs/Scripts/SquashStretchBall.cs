using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

public class SquashStretchBall : MonoBehaviour
{
    public FloatSpring positionXSpring = new() { frequencyHz = 5f, dampingRatio = 1.6f };
    public FloatSpring positionYSpring = new() { frequencyHz = 5f, dampingRatio = 1.6f };
    public FloatSpring scaleSpring = new() { frequencyHz = 5f, dampingRatio = 0.6f };

    Vector3 startPosition;
    Vector3 startScale;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.localPosition;
        startScale = transform.localScale;
    }


    void Update()
    {
        //If the input button is clicked set the equilibrium of position string to it
        //advance the spring
        if (Input.GetMouseButton(0))
        {
            Vector3 newPos = getBallPosition();
            positionXSpring.equilibrium = Mathf.Clamp(newPos.x, -10, 10);
            positionYSpring.equilibrium = Mathf.Clamp(newPos.y, -10, 10);
            transform.right = new Vector3(positionXSpring.equilibrium-transform.localPosition.x, positionYSpring.equilibrium-transform.localPosition.y, 0);
            scaleSpring.equilibrium = 10;
        }
        else
        {
            scaleSpring.equilibrium = 1;
        }

        positionXSpring.Advance(Time.deltaTime);
        positionYSpring.Advance(Time.deltaTime);
        scaleSpring.Advance(Time.deltaTime);

        UpdatePosition();
        UpdateScale();
    }

    void UpdatePosition()
    {
        Vector3 advancedSliderPos = startPosition;
        advancedSliderPos.x += positionXSpring.Value;
        advancedSliderPos.y += positionYSpring.Value;
        transform.localPosition = advancedSliderPos;
    }

    void UpdateScale()
    {
        //In the direction of movement set the scale to the value
        //In the opposite direction set the scale to 1/value

        //This stretches it
        Vector3 advancedSliderPos = Vector3.one;
        advancedSliderPos.x = scaleSpring.Value;
        advancedSliderPos.y = 1/scaleSpring.Value;
        transform.localScale = advancedSliderPos;

        // Vector3 advancedSliderPos = startScale;
        // advancedSliderPos.x = scaleSpring.Value+Math.Abs(positionXSpring.Value/10);
        // advancedSliderPos.y = 1/scaleSpring.Value+Math.Abs(positionYSpring.Value/10);
        // transform.localScale = advancedSliderPos;
    }

    Vector3 getBallPosition()
    {
        // Debug.Log(transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
        // return transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit))
            return startPosition;

        return hit.point;
    }
}
