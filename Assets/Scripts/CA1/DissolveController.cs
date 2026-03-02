using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DissolveMoltenController : MonoBehaviour
{
    [System.Serializable]
    public class TargetGroup
    {
        public string name = "Group";
        public Renderer[] targets;

        [Header("Base values")]
        public Texture diffuse;
        public float edgeWidth = 0.01f;
        public Color edgeColor = new(1f, 0.55f, 0.1f, 1f);
        public float edgeIntensityBase = 6.8f;
        public float emberIntensityBase = 1f;

        public float noiseScale = 10f;
        public float noisePower = 1f;

        public Color charColor = Color.black;
        [Range(0f, 1f)] public float charStrengthTarget = 0.9f;
        public float charWidthMult = 2f;

        public float flickerSpeed = 2f;
        [Range(0f, 1f)] public float flickerAmount = 0.2f;

        public Color emberColor = Color.black;

        [Header("Flow targets")]
        public float flowSpeedTarget = 1.5f;
        public float flowScaleTarget = 100f;
    }

    [Header("Groups")]
    [SerializeField] private TargetGroup wallGroup = new() { name = "Wall", edgeIntensityBase = 6.8f };
    [SerializeField] private TargetGroup glassGroup = new() { name = "Glass", edgeIntensityBase = 5.5f };

    [Header("Shader Graph property refs")]
    [SerializeField] private string diffuseRef = "_Diffuse";
    [SerializeField] private string dissolveAmountRef = "_DissolveAmount";
    [SerializeField] private string edgeWidthRef = "_EdgeWidth";
    [SerializeField] private string edgeColorRef = "_EdgeColor";
    [SerializeField] private string edgeIntensityRef = "_EdgeIntensity";
    [SerializeField] private string noiseScaleRef = "_NoiseScale";
    [SerializeField] private string noisePowerRef = "_NoisePower";
    [SerializeField] private string charColorRef = "_CharColor";
    [SerializeField] private string charStrengthRef = "_CharStrength";
    [SerializeField] private string charWidthMultRef = "_CharWidthMult";
    [SerializeField] private string flickerSpeedRef = "_FlickerSpeed";
    [SerializeField] private string flickerAmountRef = "_FlickerAmount";
    [SerializeField] private string flowSpeedRef = "_FlowSpeed";
    [SerializeField] private string flowScaleRef = "_FlowScale";
    [SerializeField] private string emberColorRef = "_EmberColor";
    [SerializeField] private string emberIntensityRef = "_EmberIntensity";

    [Header("Timing")]
    [SerializeField] private float dissolveDuration = 1.5f;
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Cold start")]
    [SerializeField] private float flowSpeedStart = 0.28f;
    [SerializeField] private float flowScaleStart = 8.9f;
    [SerializeField] private float charStrengthStart = 0f;

    [Header("Char ramp")]
    [Range(0f, 1f)] [SerializeField] private float charFullAtDissolve = 0.7f;
    [SerializeField] private float charSpeedMultiplier = 2f;
    [Range(0f, 1f)] [SerializeField] private float flickerAmountEnd = 0.02f;

    [Header("Flow ramp")]
    [SerializeField] private AnimationCurve flowRamp = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Emissive shaping")]
    [SerializeField] private float intensityBoostNearMid = 2f;
    [SerializeField] private bool emissiveOffWhenIntact = true;

    [SerializeField] private bool clampEdgeIntensity = true;
    [SerializeField] private float edgeIntensityClamp = 18f;

    [SerializeField] private bool clampEmberIntensity = true;
    [SerializeField] private float emberIntensityClamp = 12f;

    [Header("Edge look")]
    [SerializeField] private bool enableEdgeColorRamp = true;
    [SerializeField] private Color edgeHot = new(1f, 0.98f, 0.90f, 1f);
    [SerializeField] private Color edgeWarm = new(1f, 0.65f, 0.20f, 1f);
    [SerializeField] private Color edgeCool = new(0.60f, 0.10f, 0.05f, 1f);

    [SerializeField] private bool dynamicEdgeWidth = true;
    [SerializeField] private float edgeWidthMultiplierAtEnds = 1.2f;
    [SerializeField] private float edgeWidthMultiplierAtPeak = 0.7f;

    [SerializeField] private bool enableDynamicNoisePower = true;
    [SerializeField] private float noisePowerStart = 0.8f;
    [SerializeField] private float noisePowerEnd = 2f;

    [SerializeField] private bool enablePeakFlickerBoost = true;
    [SerializeField] private float peakFlickerBoost = 1.35f;

    [Header("Hot core punch")]
    [SerializeField] private float hotCorePunch = 1.15f;
    [SerializeField] private float hotCorePunchAt = 0.35f;
    [SerializeField] private float hotCorePunchWidth = 0.25f;

    [Header("Input")]
    [SerializeField] private Key playKey = Key.Space;
    [SerializeField] private Key resetKey = Key.R;

    private MaterialPropertyBlock mpb;
    private Coroutine routine;
    private bool isDissolved;

    private int pidDiffuse, pidDissolve, pidEdgeWidth, pidEdgeColor, pidEdgeIntensity,
                pidNoiseScale, pidNoisePower, pidCharColor, pidCharStrength, pidCharWidthMult,
                pidFlickerSpeed, pidFlickerAmount, pidFlowSpeed, pidFlowScale, pidEmberColor, pidEmberIntensity;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
        CacheIDs();

        ApplyGroupStaticParams(wallGroup);
        ApplyGroupStaticParams(glassGroup);

        ApplyColdStartState();
        ApplyToGroup(wallGroup, pidDissolve, 0f);
        ApplyToGroup(glassGroup, pidDissolve, 0f);

        if (emissiveOffWhenIntact)
        {
            ApplyToGroup(wallGroup, pidEdgeIntensity, 0f);
            ApplyToGroup(glassGroup, pidEdgeIntensity, 0f);
            ApplyToGroup(wallGroup, pidEmberIntensity, 0f);
            ApplyToGroup(glassGroup, pidEmberIntensity, 0f);
        }
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb[playKey].wasPressedThisFrame) PlayDissolve();
        if (kb[resetKey].wasPressedThisFrame) ResetDissolve();
    }

    [ContextMenu("Play Dissolve")]
    public void PlayDissolve() => StartDissolve(0f, 1f, true);

    [ContextMenu("Reset Dissolve")]
    public void ResetDissolve() => StartDissolve(1f, 0f, false);

    private void StartDissolve(float start, float end, bool endStateDissolved)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(DissolveRoutine(start, end, endStateDissolved));
    }

    private IEnumerator DissolveRoutine(float start, float end, bool endStateDissolved)
    {
        ApplyGroupStaticParams(wallGroup);
        ApplyGroupStaticParams(glassGroup);
        ApplyColdStartState();

        float elapsed = 0f;
        float charDenom = Mathf.Max(0.0001f, charFullAtDissolve);
        float charSpeed = Mathf.Max(0.01f, charSpeedMultiplier);

        while (elapsed < dissolveDuration)
        {
            float t01 = Mathf.Clamp01(elapsed / dissolveDuration);
            float shaped = dissolveCurve.Evaluate(t01);

            float dissolve = Mathf.Lerp(start, end, shaped);
            float dissolve01 = Mathf.InverseLerp(Mathf.Min(start, end), Mathf.Max(start, end), dissolve);

            float flowT = Mathf.Clamp01(flowRamp.Evaluate(shaped));
            float flowSpeed = Mathf.Lerp(flowSpeedStart, Mathf.Max(flowSpeedStart, wallGroup.flowSpeedTarget), flowT);
            float flowScale = Mathf.Lerp(flowScaleStart, Mathf.Max(flowScaleStart, wallGroup.flowScaleTarget), flowT);

            float charT = Mathf.Clamp01((dissolve01 / charDenom) * charSpeed);
            float wallChar = Mathf.Lerp(charStrengthStart, wallGroup.charStrengthTarget, charT);
            float glassChar = Mathf.Lerp(charStrengthStart, glassGroup.charStrengthTarget, charT);

            float wallFlicker = Mathf.Lerp(wallGroup.flickerAmount, flickerAmountEnd, charT);
            float glassFlicker = Mathf.Lerp(glassGroup.flickerAmount, flickerAmountEnd, charT);

            float peak = Mathf.Clamp01(1f - Mathf.Abs(dissolve01 - 0.5f) * 2f);
            float boost = 1f + intensityBoostNearMid * peak;

            float punchWindow = 1f - Mathf.Clamp01(Mathf.Abs(dissolve01 - hotCorePunchAt) / Mathf.Max(0.0001f, hotCorePunchWidth));
            punchWindow = Mathf.SmoothStep(0f, 1f, punchWindow);
            float hotCoreMult = Mathf.Lerp(1f, Mathf.Max(1f, hotCorePunch), punchWindow);

            float wallEdgeInt = wallGroup.edgeIntensityBase * boost * hotCoreMult;
            float glassEdgeInt = glassGroup.edgeIntensityBase * boost * hotCoreMult;

            float wallEmberInt = wallGroup.emberIntensityBase * boost;
            float glassEmberInt = glassGroup.emberIntensityBase * boost;

            if (clampEdgeIntensity)
            {
                wallEdgeInt = Mathf.Min(wallEdgeInt, edgeIntensityClamp);
                glassEdgeInt = Mathf.Min(glassEdgeInt, edgeIntensityClamp);
            }
            if (clampEmberIntensity)
            {
                wallEmberInt = Mathf.Min(wallEmberInt, emberIntensityClamp);
                glassEmberInt = Mathf.Min(glassEmberInt, emberIntensityClamp);
            }

            if (dynamicEdgeWidth)
            {
                float widthMult = Mathf.Lerp(edgeWidthMultiplierAtEnds, edgeWidthMultiplierAtPeak, peak);
                ApplyToGroup(wallGroup, pidEdgeWidth, wallGroup.edgeWidth * widthMult);
                ApplyToGroup(glassGroup, pidEdgeWidth, glassGroup.edgeWidth * widthMult);
            }

            if (enableEdgeColorRamp)
            {
                float warmT = Mathf.Clamp01(dissolve01 * 1.2f);
                Color edgeCol = Color.Lerp(edgeHot, edgeWarm, warmT);
                edgeCol = Color.Lerp(edgeCol, edgeCool, charT * 0.8f);
                ApplyToGroup(wallGroup, pidEdgeColor, edgeCol);
                ApplyToGroup(glassGroup, pidEdgeColor, edgeCol);
            }
            else
            {
                ApplyToGroup(wallGroup, pidEdgeColor, wallGroup.edgeColor);
                ApplyToGroup(glassGroup, pidEdgeColor, glassGroup.edgeColor);
            }

            if (enableDynamicNoisePower)
            {
                float np = Mathf.Lerp(noisePowerStart, noisePowerEnd, dissolve01);
                ApplyToGroup(wallGroup, pidNoisePower, np);
                ApplyToGroup(glassGroup, pidNoisePower, np);
            }
            else
            {
                ApplyToGroup(wallGroup, pidNoisePower, wallGroup.noisePower);
                ApplyToGroup(glassGroup, pidNoisePower, glassGroup.noisePower);
            }

            if (enablePeakFlickerBoost)
            {
                float fBoost = Mathf.Lerp(1f, Mathf.Max(1f, peakFlickerBoost), peak);
                wallFlicker *= fBoost;
                glassFlicker *= fBoost;
            }

            ApplyToGroup(wallGroup, pidDissolve, dissolve);
            ApplyToGroup(glassGroup, pidDissolve, dissolve);

            ApplyToGroup(wallGroup, pidFlowSpeed, flowSpeed);
            ApplyToGroup(glassGroup, pidFlowSpeed, flowSpeed);

            ApplyToGroup(wallGroup, pidFlowScale, flowScale);
            ApplyToGroup(glassGroup, pidFlowScale, flowScale);

            ApplyToGroup(wallGroup, pidCharStrength, wallChar);
            ApplyToGroup(glassGroup, pidCharStrength, glassChar);

            ApplyToGroup(wallGroup, pidFlickerAmount, Mathf.Clamp01(wallFlicker));
            ApplyToGroup(glassGroup, pidFlickerAmount, Mathf.Clamp01(glassFlicker));

            ApplyToGroup(wallGroup, pidEdgeIntensity, wallEdgeInt);
            ApplyToGroup(glassGroup, pidEdgeIntensity, glassEdgeInt);

            ApplyToGroup(wallGroup, pidEmberIntensity, wallEmberInt);
            ApplyToGroup(glassGroup, pidEmberIntensity, glassEmberInt);

            elapsed += Time.deltaTime;
            yield return null;
        }

        ApplyToGroup(wallGroup, pidDissolve, end);
        ApplyToGroup(glassGroup, pidDissolve, end);

        isDissolved = endStateDissolved;

        if (!isDissolved)
        {
            ApplyColdStartState();

            if (emissiveOffWhenIntact)
            {
                ApplyToGroup(wallGroup, pidEdgeIntensity, 0f);
                ApplyToGroup(glassGroup, pidEdgeIntensity, 0f);
                ApplyToGroup(wallGroup, pidEmberIntensity, 0f);
                ApplyToGroup(glassGroup, pidEmberIntensity, 0f);
            }
        }

        routine = null;
    }

    private void ApplyColdStartState()
    {
        ApplyToGroup(wallGroup, pidCharStrength, charStrengthStart);
        ApplyToGroup(glassGroup, pidCharStrength, charStrengthStart);

        ApplyToGroup(wallGroup, pidFlowSpeed, flowSpeedStart);
        ApplyToGroup(glassGroup, pidFlowSpeed, flowSpeedStart);

        ApplyToGroup(wallGroup, pidFlowScale, flowScaleStart);
        ApplyToGroup(glassGroup, pidFlowScale, flowScaleStart);

        ApplyToGroup(wallGroup, pidFlickerAmount, Mathf.Clamp01(wallGroup.flickerAmount));
        ApplyToGroup(glassGroup, pidFlickerAmount, Mathf.Clamp01(glassGroup.flickerAmount));
    }

    private void ApplyGroupStaticParams(TargetGroup g)
    {
        if (g == null) return;

        if (g.diffuse != null) ApplyToGroup(g, pidDiffuse, g.diffuse);

        ApplyToGroup(g, pidEdgeWidth, g.edgeWidth);
        ApplyToGroup(g, pidNoiseScale, g.noiseScale);
        ApplyToGroup(g, pidCharColor, g.charColor);
        ApplyToGroup(g, pidCharWidthMult, g.charWidthMult);
        ApplyToGroup(g, pidFlickerSpeed, Mathf.Max(0f, g.flickerSpeed));
        ApplyToGroup(g, pidEmberColor, g.emberColor);

        ApplyToGroup(g, pidEdgeColor, g.edgeColor);
        ApplyToGroup(g, pidNoisePower, g.noisePower);
    }

    private void CacheIDs()
    {
        pidDiffuse = Shader.PropertyToID(diffuseRef);
        pidDissolve = Shader.PropertyToID(dissolveAmountRef);
        pidEdgeWidth = Shader.PropertyToID(edgeWidthRef);
        pidEdgeColor = Shader.PropertyToID(edgeColorRef);
        pidEdgeIntensity = Shader.PropertyToID(edgeIntensityRef);
        pidNoiseScale = Shader.PropertyToID(noiseScaleRef);
        pidNoisePower = Shader.PropertyToID(noisePowerRef);
        pidCharColor = Shader.PropertyToID(charColorRef);
        pidCharStrength = Shader.PropertyToID(charStrengthRef);
        pidCharWidthMult = Shader.PropertyToID(charWidthMultRef);
        pidFlickerSpeed = Shader.PropertyToID(flickerSpeedRef);
        pidFlickerAmount = Shader.PropertyToID(flickerAmountRef);
        pidFlowSpeed = Shader.PropertyToID(flowSpeedRef);
        pidFlowScale = Shader.PropertyToID(flowScaleRef);
        pidEmberColor = Shader.PropertyToID(emberColorRef);
        pidEmberIntensity = Shader.PropertyToID(emberIntensityRef);
    }

    private void ApplyToGroup(TargetGroup g, int propertyId, float value)
    {
        if (g?.targets == null || g.targets.Length == 0) return;

        for (int i = 0; i < g.targets.Length; i++)
        {
            var r = g.targets[i];
            if (!r) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetFloat(propertyId, value);
            r.SetPropertyBlock(mpb);
        }
    }

    private void ApplyToGroup(TargetGroup g, int propertyId, Color value)
    {
        if (g?.targets == null || g.targets.Length == 0) return;

        for (int i = 0; i < g.targets.Length; i++)
        {
            var r = g.targets[i];
            if (!r) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor(propertyId, value);
            r.SetPropertyBlock(mpb);
        }
    }

    private void ApplyToGroup(TargetGroup g, int propertyId, Texture value)
    {
        if (g?.targets == null || g.targets.Length == 0) return;

        for (int i = 0; i < g.targets.Length; i++)
        {
            var r = g.targets[i];
            if (!r) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetTexture(propertyId, value);
            r.SetPropertyBlock(mpb);
        }
    }
}
