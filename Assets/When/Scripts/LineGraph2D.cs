using System;
using System.Linq.Expressions;
using Leap;
using UnityEngine;
using UnityEngine.Rendering;

public class LineGraph2D : MonoBehaviour {

    #region Parameters
    [SerializeField]
    IHandModel handModel;
    [SerializeField]
    int resolution = 10;
    [SerializeField]
    float pointScale = .01f;
    [SerializeField]
    Color color = Color.white;
    [SerializeField]
    string fname;
    #endregion

    #region Private
    int currentResolution;
    Delegate f;
    float lastValue;
    LineRenderer lineRenderer;
    Vector3[] m_vertices = new Vector3[0];
    #endregion

    Transform centerEyeAnchor; // TEST

    void OnValidation() {
        resolution = Mathf.Max(resolution, 10);
    }

    void Awake() {
        centerEyeAnchor = GameObject.FindGameObjectWithTag("MainCamera").transform; // TEST

        GameObject go = new GameObject();
        go.transform.SetParent(gameObject.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        lineRenderer = go.AddComponent<LineRenderer>();
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.useLightProbes = false;
        lineRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        lineRenderer.useWorldSpace = false;
        lineRenderer.SetColors(color, color);
        lineRenderer.SetWidth(pointScale, pointScale);
        go.hideFlags = HideFlags.HideInHierarchy;
    }

    void Start() {
        f = Delegate.CreateDelegate(Expression.GetFuncType(typeof(Hand), typeof(float)), this, fname);
    }

    void CreatePoints() {
        currentResolution = resolution;

        m_vertices = new Vector3[resolution];
        lineRenderer.SetVertexCount(resolution);
        float increment = 1f / (resolution - 1);
        for (int i = 0; i < resolution; i++) {
            float x = i * increment;
            var p = new Vector3(x - .5f, 0f, 0f);
            lineRenderer.SetPosition(i, p);
            m_vertices[i] = p;
        }
    }

    void Update() {
        if (currentResolution != resolution || m_vertices.Length == 0) {
            CreatePoints();
        }

        Next(handModel.GetLeapHand());
    }

    void Next(Hand hand) {
        for (int i = 0; i < resolution - 1; i++) {
            m_vertices[i] = new Vector3(m_vertices[i].x, m_vertices[i + 1].y, m_vertices[i].z);
            lineRenderer.SetPosition(i, m_vertices[i]);
        }

        lastValue = (hand == null || !handModel.IsTracked) ? -.5f : (float)f.DynamicInvoke(hand) * .5f;
        m_vertices[resolution - 1] = new Vector3(m_vertices[resolution - 1].x, lastValue, 0);
        lineRenderer.SetPosition(resolution - 1, m_vertices[resolution - 1]);
    }

    #region ValueFunctions
    float PinchDistance(Hand hand) {
        return hand.PinchDistance * .01f;
    }
    float GrabStrength(Hand hand) {
        return hand.GrabStrength;
    }
    float PinchStrength(Hand hand) {
        return hand.PinchStrength;
    }
    float PalmDotCenterEye(Hand hand) { // TEST
        return Vector3.Dot(hand.PalmNormal.ToVector3(), centerEyeAnchor.forward);
    }
    float ThumbVelocityX(Hand hand) {
        return hand.Fingers[0].TipVelocity.x;
    }
    float ThumbVelocityY(Hand hand) {
        return hand.Fingers[0].TipVelocity.y;
    }
    float ThumbVelocityZ(Hand hand) {
        return hand.Fingers[0].TipVelocity.z;
    }
    float PalmVelocityX(Hand hand) {
        return hand.PalmVelocity.x;
    }
    float PalmVelocityY(Hand hand) {
        return hand.PalmVelocity.y;
    }
    float PalmVelocityZ(Hand hand) {
        return hand.PalmVelocity.z;
    }
    #endregion
}