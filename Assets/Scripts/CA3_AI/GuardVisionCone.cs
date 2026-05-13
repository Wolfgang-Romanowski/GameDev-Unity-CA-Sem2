using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GuardVisionCone : MonoBehaviour
{
    [SerializeField] private float viewDistance = 15f;
    [SerializeField] private float viewAngle = 55f;
    
    [Range(10, 80)]
    [Tooltip("Higher = smoother cone but more raycasts. 80 rays × 3 heights = 240 casts/frame. Acceptable for single guard; would need LOD culling at scale.")]
    [SerializeField] private int rayCount = 80;

    [Tooltip("Small lift above the detected floor so the cone doesn't z-fight with the ground. Floor lookup costs 1 raycast/frame in addition to the cone raycasts.")]
    [SerializeField] private float coneFloorOffset = 0.05f;
    [Tooltip("Layers treated as floor when locating the guard's feet via downward raycast.")]
    [SerializeField] private LayerMask floorMask = ~0;

    [Header("Hearing Visualisation")]
    [Tooltip("Drawn as a translucent disc around the guard so the player can read the hearing range. NOTE: must match GuardSensor.hearingRange for the visualisation to be honest.")]
    [SerializeField] private float hearingRange = 5f;
    [Tooltip("Set to false to hide the hearing ring entirely.")]
    [SerializeField] private bool showHearingRing = true;
    [SerializeField] private Color hearingRingColor = new Color(1f, 1f, 0f, 0.08f);

    [SerializeField] private Color patrolColor = new Color(0f, 1f, 0f, 0.2f);
    [SerializeField] private Color investigateColor = new Color(1f, 1f, 0f, 0.25f);
    [SerializeField] private Color chaseColor = new Color(1f, 0f, 0f, 0.35f);
    [SerializeField] private Color searchColor = new Color(1f, 0.5f, 0f, 0.25f);
    [SerializeField] private Color detectColor = new Color(1f, 0f, 0f, 0.5f);

    private const int HeightSteps = 3;
    private const int HearingRingSegments = 48;

    private Mesh mesh;
    private Material coneMaterial;
    private GuardAI guardAI;

    private GameObject hearingRingObj;
    private Mesh hearingRingMesh;
    private Material hearingRingMaterial;

    //cached cone mesh buffers — avoid per-frame GC churn (profiler showed ~1KB/frame)
    private Vector3[] coneVertices;
    private int[] coneTriangles;
    private int lastRayCount = -1;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        var meshRenderer = GetComponent<MeshRenderer>();
        coneMaterial = new Material(Shader.Find("Sprites/Default"));
        coneMaterial.color = patrolColor;
        meshRenderer.material = coneMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        guardAI = GetComponentInParent<GuardAI>();

        if (showHearingRing)
            CreateHearingRing();
    }

    void CreateHearingRing()
    {
        hearingRingObj = new GameObject("HearingRing");
        hearingRingObj.transform.SetParent(transform.parent, false);
        hearingRingObj.transform.localPosition = Vector3.up * 0.05f;

        var mf = hearingRingObj.AddComponent<MeshFilter>();
        var mr = hearingRingObj.AddComponent<MeshRenderer>();

        hearingRingMesh = new Mesh();
        mf.mesh = hearingRingMesh;

        hearingRingMaterial = new Material(Shader.Find("Sprites/Default"));
        hearingRingMaterial.color = hearingRingColor;
        mr.material = hearingRingMaterial;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        BuildHearingRingMesh();
    }

    void BuildHearingRingMesh()
    {
        Vector3[] vertices = new Vector3[HearingRingSegments + 1];
        int[] triangles = new int[HearingRingSegments * 3];
        vertices[0] = Vector3.zero;

        float angleStep = 360f / HearingRingSegments;
        for (int i = 0; i < HearingRingSegments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * hearingRange;
        }

        for (int i = 0; i < HearingRingSegments; i++)
        {
            triangles[i * 3]     = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % HearingRingSegments + 1;
        }

        hearingRingMesh.Clear();
        hearingRingMesh.vertices = vertices;
        hearingRingMesh.triangles = triangles;
        hearingRingMesh.RecalculateNormals();
    }

    void LateUpdate()
    {
        DrawVisionCone();
        UpdateColor();
    }

        void DrawVisionCone()
        {
            float angleStep = (viewAngle * 2f) / rayCount;

            if (coneVertices == null || lastRayCount != rayCount)
            {
                coneVertices = new Vector3[rayCount + 2];
                coneTriangles = new int[rayCount * 3];
                lastRayCount = rayCount;
            }

            coneVertices[0] = Vector3.zero;

            Transform guard = transform.parent;
            float guardYAngle = guard.eulerAngles.y;

            for (int i = 0; i <= rayCount; i++)
            {
                float currentAngle = -viewAngle + (angleStep * i);
                float worldAngle = guardYAngle + currentAngle;
                float rad = worldAngle * Mathf.Deg2Rad;
                Vector3 worldDir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));

                float hitDistance = viewDistance;

                //cast at multiple heights so cone clips against walls properly
                for (int h = 0; h < HeightSteps; h++)
                {
                    float height = 0.3f + (h * 0.5f);
                    Vector3 origin = guard.position + Vector3.up * height;

                    if (Physics.Raycast(origin, worldDir, out RaycastHit hit, viewDistance))
                    {
                        if (!hit.transform.CompareTag("Player") && hit.distance < hitDistance)
                            hitDistance = hit.distance;
                    }
                }

                float localRad = currentAngle * Mathf.Deg2Rad;
                Vector3 localDir = new Vector3(Mathf.Sin(localRad), 0f, Mathf.Cos(localRad));
                coneVertices[i + 1] = localDir * hitDistance;
            }

            for (int i = 0; i < rayCount; i++)
            {
                coneTriangles[i * 3] = 0;
                coneTriangles[i * 3 + 1] = i + 1;
                coneTriangles[i * 3 + 2] = i + 2;
            }

            mesh.Clear();
            mesh.vertices = coneVertices;
            mesh.triangles = coneTriangles;
            mesh.RecalculateNormals();

            float floorY = ResolveFloorY(guard.position);
            transform.position = new Vector3(guard.position.x, floorY + coneFloorOffset, guard.position.z);

            if (hearingRingObj != null)
                hearingRingObj.transform.position = new Vector3(guard.position.x, floorY + coneFloorOffset, guard.position.z);
        }

    float ResolveFloorY(Vector3 guardPos)
    {
        Vector3 origin = guardPos + Vector3.up * 2f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 10f, floorMask, QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return guardPos.y;
    }

        void UpdateColor()
    {
        if (coneMaterial == null || guardAI == null) return;

        Color targetColor = guardAI.CurrentState switch
        {
            GuardState.Patrol      => patrolColor,
            GuardState.Investigate => investigateColor,
            GuardState.Chase       => chaseColor,
            GuardState.Search      => searchColor,
            _                      => patrolColor
        };

        //flash detect colour when in Chase
        if (guardAI.CurrentState == GuardState.Chase)
            targetColor = Color.Lerp(targetColor, detectColor, Time.deltaTime * 10f);

        coneMaterial.color = Color.Lerp(coneMaterial.color, targetColor, Time.deltaTime * 5f);
    }
}