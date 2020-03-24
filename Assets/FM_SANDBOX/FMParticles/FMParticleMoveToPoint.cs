using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BezierCurve
{
    Quadratic,
    Cubic
}

/// <summary>
/// it will spawn particles(UISprite) form point A to point B onclick, using
/// one of the bezier curves available (Quadratic for straighter transitions, Cubic for more erratic transitions)
/// </summary>
public class FMParticleMoveToPoint: MonoBehaviour
{
    [SerializeField]
    public Transform Destination;

    Vector3 clickPosition;
    Vector3 p1;
    Vector3 p2;
    float t = -1;
    Vector3 closeToClickDistance;
    BezierCurve curveType = BezierCurve.Quadratic;
    Vector3 offset = new Vector3(999f, 999f, 999f);
    ParticleSystem particleSystem;

    private void Start()
    {
        particleSystem = gameObject.GetComponent<ParticleSystem>();
        transform.position = offset;
        particleSystem.Pause();
    }

    void Update()
    {
        if (t >= 0f)
        {
            MoveParticle();
        }
    }

    public void InitPosition(Vector3 startPosition)
    {
        clickPosition = startPosition;
        transform.position = startPosition;

        //closeToStartDistance is a position very close to the click related to the destination
        closeToClickDistance = Destination.position - clickPosition; 
        Vector3.Normalize(closeToClickDistance);

        //get a perpendicular point 
        closeToClickDistance.x = closeToClickDistance.y * RandomExcept(-1, 1, 0);
        closeToClickDistance.y = closeToClickDistance.x * RandomExcept(-1, 1, 0);

        //get p1 and p2 values by getting new random positions from the perpendicular point
        p1 = closeToClickDistance + new Vector3(Random.Range(-1f, 1f),
            Random.Range(-1f, 1f), 0);
        if (!curveType.Equals(BezierCurve.Quadratic))
        {
            p2 = closeToClickDistance + new Vector3(Random.Range(-0.1f, 1f), 0, 0);
        }
        t = 0;
        particleSystem.Play();
    }

    void MoveParticle()
    {
        if (t >= 1f)
        {
            t = -1f;
            transform.position = offset;
            return;
        }
        //lerping from click to bezier point
        Vector3 bezier = Vector3.zero;
        switch (curveType)
        {
            case BezierCurve.Quadratic:
                bezier = GetBezierQuadraticPosition(t, clickPosition, p1, Destination.position); 
                break;
            case BezierCurve.Cubic:
                bezier = GetBezierCubicPosition(t, clickPosition, p1, p2, Destination.position);
                break;
        }
        transform.position = Vector3.Lerp(clickPosition, bezier, t);
        t = Mathf.LerpUnclamped(0, 1, t + Time.deltaTime);
    }

    /// <summary>
    /// quadratic bezier curve
    /// </summary>
    /// <param name="t">time (from 0 to 1)</param>
    /// <param name="p0">the starting position(click)</param>
    /// <param name="p1">calculated on click (var P1)</param>
    /// <param name="p2">destination position</param>
    /// <returns></returns>
    Vector3 GetBezierQuadraticPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2              
        return Mathf.Pow(1 - t, 2) * p0 + 2 * (1 - t) * t * p1 + Mathf.Pow(t, 2) * p2;
    }

    /// <summary>
    /// cubic bezier curve
    /// </summary>
    /// <param name="t">time (from 0 to 1)</param>
    /// <param name="p0">the starting position(click)</param>
    /// <param name="p1">calculated on click (var P1)</param>
    /// <param name="p2">calculated on click (var P2)</param>
    /// <param name="p3">destination position</param>
    /// <returns></returns>
    Vector3 GetBezierCubicPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // (1 - t)^3 * p0 + 3(1 - t)^2 * p1 + 3(1 - t)* t^2 *p^2 + t^3* p^3        
        return Mathf.Pow(1f - t, 3f) * p0 + 3f * Mathf.Pow(1f - t, 2f) * t * p1 + 3f * (1f - t)
            * Mathf.Pow(t, 2f) * p2 + Mathf.Pow(t, 3f) * p3;
    }

    public void SetBezierCurve(BezierCurve bezierCurve)
    {
        curveType = bezierCurve;
    }

    /// <summary>
    /// random that ignores 1 number
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="except"></param>
    /// <returns></returns>
    int RandomExcept(int min, int max, int except)
    {
        int random = Random.Range(min, max);
        if (random >= except) random = (random + 1) % max;
        return random;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(p1, 0.1f);
    //    Gizmos.DrawSphere(p2, 0.1f);
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(closeToClickDistance, 0.1f);

    //    Gizmos.color = Color.gray;
    //    Gizmos.DrawLine(clickPosition, destination.position);
    //}
}
