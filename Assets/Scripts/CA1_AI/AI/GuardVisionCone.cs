using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GuardVisionCone : MonoBehaviour
{
    [SerializeField] private float viewDistance = 15f;
    [SerializeField] private float viewAngle = 55f;
    [SerializeField] private int rayCount = 80;

    [SerializeField] private Color patrolColor = new Color(0f, 1f, 0f, 0.2f);
    [SerializeField] private Color investigateColor = new Color(1f, 1f, 0f, 0.25f);
    [SerializeField] private Color chaseColor = new Color(1f, 0f, 0f, 0.35f);
    [SerializeField] private Color searchColor = new Color(1f, 0.5f, 0f, 0.25f);
    [SerializeField] private Color detectColor = new Color(1f, 0f, 0f, 0.5f);

    private Mesh mesh;
    private Material coneMaterial;
    private GuardAI guardAI;

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
    }

    void LateUpdate()
    {
        DrawVisionCone();
        UpdateColor();
    }

        void DrawVisionCone()
        {
            float angleStep = (viewAngle * 2f) / rayCount;
            Vector3[] vertices = new Vector3[rayCount + 2];
            int[] triangles = new int[rayCount * 3];
            vertices[0] = Vector3.zero;

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
                for (int h = 0; h < 3; h++)
                {
                    float height = 0.3f + (h * 0.5f);
                    Vector3 origin = guard.position + Vector3.up * height;

                    if (Physics.Raycast(origin, worldDir, out RaycastHit hit, viewDistance))
                    {
                        if (!hit.transform.CompareTag("Player") && hit.distance < hitDistance)
                            hitDistance = hit.distance;
                    }
                }

                //convert back to local space for the vertex
                float localRad = currentAngle * Mathf.Deg2Rad;
                Vector3 localDir = new Vector3(Mathf.Sin(localRad), 0f, Mathf.Cos(localRad));
                vertices[i + 1] = localDir * hitDistance;
            }

            for (int i = 0; i < rayCount; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
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