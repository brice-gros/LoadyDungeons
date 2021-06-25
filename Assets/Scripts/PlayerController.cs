using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.AI.Behaviours;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float m_MovementSpeed = 5.0f;

    [SerializeField]
    private LayerMask m_InputCollisionLayer;

    [SerializeField]
    private string m_NextLevel;

    private BTAgent m_BTAgent;
    private Animator m_AnimatorController;
    private PlayerConfigurator m_PlayerConfigurator;

    public bool m_HasKey = false;
    [SerializeField]
    private Image m_KeyUI;

    [SerializeField]
    private TMPro.TextMeshPro m_DebugText;

    private int m_VelocityHash = Animator.StringToHash("Velocity");

    private Camera m_MainCamera;
    
    [SerializeField]
    private GameObject m_level;
    public float dotProd;
    public float movMagnitude;
    private Vector3 m_Velocity;
    private Vector3 m_CameraLookAtPos;
    private NavMeshHit m_NavHitInfo;
    private Vector3 m_startPoint;
    private float m_InitRatio;
   
    const float k_MinMovementDistance = 1.2f;

    static Vector2 To2D(Vector3 v) { return new Vector2(v.x, v.z); }
    static Vector3 To3D(Vector2 v) { return new Vector3(v.x, 0f, v.y); }
    static float Cross(Vector2 a, Vector2 b) { return a.x * b.y - a.y * b.x; }

    void Start()
    {
        m_startPoint = transform.position;
        m_InitRatio = 0.0f;
        transform.position += Vector3.up * 5.0f;
        m_AnimatorController = GetComponent<Animator>();
        m_PlayerConfigurator = GetComponent<PlayerConfigurator>();
        m_BTAgent = GetComponent<BTAgent>();
        m_MainCamera = Camera.main;
    }

    private IEnumerator KeyCollected()
    {
        yield return new WaitForSeconds(2.1f);
        m_HasKey = true;
        m_KeyUI.color = Color.white;
    }

    private void OnTriggerEnter(Collider other)
    {
        //TODO: Cache the string value
        if (other.CompareTag("Chest"))
        {
            // TODO: Maybe cache the getcomponent read, although it is only read once
            var chest = other.gameObject.GetComponent<Chest>();
            chest.Open();
            if (chest.HasKey()) {
                StartCoroutine(KeyCollected());
            } else {
                m_PlayerConfigurator.SetHat(chest.m_HatName);
            }
        }

        if (other.CompareTag("Door"))
        {
            Debug.Log("Triggered by a door");

            if(m_HasKey)
            {
                Debug.Log("Opened the door");

                other.gameObject.GetComponent<Door>().Open();

                // TODO: Change this number to a member variable
                StartCoroutine(LevelCompleted());
            }
        }
    }

    private IEnumerator LevelCompleted()
    {
        yield return new WaitForSeconds(2.15f);
        m_HasKey = false;
        m_KeyUI.color = Color.gray;
        transform.LookAt(m_MainCamera.transform.position, Vector3.up);
        m_AnimatorController.Play("Dino_Victory");
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene(m_NextLevel);
    }

    void Update()
    {
        if (m_InitRatio < 1f) {
            m_InitRatio += Time.deltaTime / 0.7f;
            transform.position = Vector3.Lerp(m_startPoint+Vector3.up*5f, m_startPoint, m_InitRatio);
        } else {
            if (Input.GetMouseButton(0))
            {
                MoveToPosition(Input.mousePosition);
            }
            else if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                MoveToPosition(touch.position);
            }
            if (m_Velocity != Vector3.zero) {
                // sadly no MoveOnNavMeshQuery, to smooth movement along the edges, GwNavigation where are you ❤
                var maxMove2D = (To2D(m_NavHitInfo.position) - To2D(transform.position)) * 0.95f;
                var currentMove = m_Velocity * Time.deltaTime;
                if ((maxMove2D - To2D(currentMove)).sqrMagnitude < 0.1f) {
                    var dir = To2D(transform.forward); //To2D(transform.position - m_NavHitInfo.position).normalized;
                    var cross = Cross(dir, To2D(m_NavHitInfo.normal));
                    bool clockwise = cross >= 0f;
                    var move2d = new Vector2((clockwise ? 1f : -1f) * m_NavHitInfo.normal.z, (clockwise ? -1f : 1f) * m_NavHitInfo.normal.x)* Time.deltaTime;
                    m_Velocity = (To3D(move2d) + m_NavHitInfo.normal) * m_Velocity.magnitude;
                    currentMove = m_Velocity * Time.deltaTime;
                }
                var prevPos = transform.position;
                var newPos = transform.position + currentMove;
                NavMeshHit navHitInfo;
                NavMesh.Raycast(prevPos, newPos, out navHitInfo, NavMesh.AllAreas);
                transform.position = navHitInfo.position;

                m_Velocity *= 0.5f;
                if (m_Velocity.sqrMagnitude < 0.005f) {
                    m_Velocity = Vector3.zero;
                }
            }
        }
        
        // apply animation
        if(m_AnimatorController != null)
            m_AnimatorController.SetFloat(m_VelocityHash, m_Velocity.magnitude);

        m_CameraLookAtPos = transform.position + Vector3.up * 3.0F;
        var vecDiff = (m_CameraLookAtPos - m_MainCamera.transform.position);
        m_MainCamera.transform.LookAt(m_CameraLookAtPos);
        if (vecDiff.sqrMagnitude < 100.0F) {
            m_MainCamera.GetComponent<Rigidbody>().velocity = -vecDiff.normalized * m_MovementSpeed;
        } else if (vecDiff.sqrMagnitude > 400.0F) {
            m_MainCamera.GetComponent<Rigidbody>().velocity = vecDiff.normalized * m_MovementSpeed;
        } else {
            m_MainCamera.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    void MoveToPosition(Vector2 screenPosition)
    {
        Vector2 refPoint = m_MainCamera.WorldToScreenPoint(transform.position);       
        Vector2 diff = screenPosition - refPoint;
        Vector3 worldDir = new Vector3(diff.x, 0.0f, diff.y);
        worldDir.Normalize();
        var movementDirection = worldDir; //transform.InverseTransformVector(worldDir);
        var velocity = movementDirection * m_MovementSpeed;
        var userHintDest = transform.position+velocity; // Sadly, no RayCanGo vs RayCast, have to provide a destination...
        
        userHintDest.y = transform.position.y;
        NavMesh.Raycast(transform.position, userHintDest, out m_NavHitInfo, NavMesh.AllAreas);

        // sadly, no visual debug replay tool, NavigationLab I miss you ❤
        var debugDelay = 5;
        Debug.DrawLine(transform.position, userHintDest, Color.red, debugDelay);
        Debug.DrawRay(m_NavHitInfo.position, Vector3.up, Color.magenta, debugDelay);
        Debug.DrawRay(m_NavHitInfo.position, m_NavHitInfo.normal, Color.yellow, debugDelay);
        if (m_DebugText) {
            // sadly no Debug.Text3d()
            m_DebugText.text = $"{m_NavHitInfo.distance}";
        }
        // rotation
        var lookAt = userHintDest;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt, Vector3.up);
        // apply calculated velocity
        m_Velocity = velocity;
    }
}
