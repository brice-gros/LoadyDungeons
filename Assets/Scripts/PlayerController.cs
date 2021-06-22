using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float m_MovementSpeed = 5.0f;

    [SerializeField]
    private LayerMask m_InputCollisionLayer;

    [SerializeField]
    private string m_NextLevel;

    private Animator m_AnimatorController;

    private bool m_HasKey = false;
    [SerializeField]
    private Image m_KeyUI;

    private Rigidbody m_Rigidbody;

    [SerializeField]
    private TMPro.TextMeshPro m_DebugText;

    private int m_VelocityHash = Animator.StringToHash("Velocity");

    private Camera m_MainCamera;
    
    [SerializeField]
    private GameObject m_level;
    public float dotProd;
    public float movMagnitude;
    private Vector3 m_Velocity;
    private RaycastHit m_PhysHitInfo;
    private NavMeshHit m_NavHitInfo;
    private MeshCollider m_MeshCollider;
   
    const float k_MinMovementDistance = 1.2f;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_AnimatorController = GetComponent<Animator>();
        m_MainCamera = Camera.main;
        
        // for screen to world raycast, sadly no NavMesh.Raycast3d()...
        var navTriangles = NavMesh.CalculateTriangulation();
        var mesh = new Mesh();
        mesh.SetVertices(navTriangles.vertices);
        mesh.SetIndices(navTriangles.indices, MeshTopology.Triangles, 0);
        m_MeshCollider = m_level.GetComponent<MeshCollider>();
        m_MeshCollider.sharedMesh = mesh;
    }

    private void KeyCollected()
    {
        m_HasKey = true;
        m_KeyUI.color = Color.white;
    }

    private void OnTriggerEnter(Collider other)
    {
        //TODO: Cache the string value
        if (other.CompareTag("Chest"))
        {
            // TODO: Maybe cache the getcomponent read, although it is only read once
            other.gameObject.GetComponent<Chest>().Open();

            KeyCollected();
        }

        if (other.CompareTag("Door"))
        {
            Debug.Log("Triggered by a door");

            if(m_HasKey)
            {
                Debug.Log("Opened the door");
                m_HasKey = false;
                m_KeyUI.color = Color.gray;

                other.gameObject.GetComponent<Door>().Open();

                // TODO: Change this number to a member variable
                StartCoroutine(LevelCompleted());
            }
        }
    }

    private IEnumerator LevelCompleted()
    {
        yield return new WaitForSeconds(2.15f);
        transform.LookAt(m_MainCamera.transform.position, Vector3.up);
        m_Velocity = (m_MainCamera.transform.position - transform.position).normalized  * m_MovementSpeed;
        m_AnimatorController.Play("Dino_Victory");
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene(m_NextLevel);
    }

    void Update()
    {
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
            var maxMove = m_NavHitInfo.position - transform.position;
            var currentMove = m_Velocity * Time.deltaTime;
            if (maxMove.sqrMagnitude - currentMove.sqrMagnitude < 0.01f) {
                transform.position = m_NavHitInfo.position;
                m_Velocity = Vector3.zero;
            } else {
                transform.position += currentMove;
                m_Velocity *= 0.5f;
                if (m_Velocity.sqrMagnitude < 0.005f) {
                    m_Velocity = Vector3.zero;
                }
            }
        }
        
        // apply animation
        if(m_AnimatorController != null)
            m_AnimatorController.SetFloat(m_VelocityHash, (m_Rigidbody.velocity + m_Velocity).magnitude);

        var cameraLookAtPos = transform.position + Vector3.up * 3.0F;
        var vecDiff = (cameraLookAtPos - m_MainCamera.transform.position);
        m_MainCamera.transform.LookAt(cameraLookAtPos);
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
        if (Physics.Raycast(m_MainCamera.ScreenPointToRay(screenPosition), out m_PhysHitInfo, Mathf.Infinity, m_InputCollisionLayer))
        {
            // don't move if touching close to character
            if (Vector3.Distance(transform.position, m_PhysHitInfo.point) > k_MinMovementDistance)
            {               
                NavMesh.Raycast(transform.position, m_PhysHitInfo.point, out m_NavHitInfo, NavMesh.AllAreas);

                // calculate move direction vector
                Vector3 movementDirection = m_PhysHitInfo.point - transform.position;    
                movementDirection.y = 0.0f;
                movementDirection.Normalize();
                var velocity = movementDirection * m_MovementSpeed;
                
                // sadly, no visual debug replay tool, NavigationLab I miss you ❤
                var debugDelay = 5;
                Debug.DrawLine(transform.position, m_PhysHitInfo.point, Color.red, debugDelay);
                Debug.DrawRay(m_NavHitInfo.position, Vector3.up, Color.magenta, debugDelay);
                Debug.DrawRay(m_NavHitInfo.position, m_NavHitInfo.normal, Color.yellow, debugDelay);
                if (m_DebugText) {
                    // sadly no Debug.Text3d()
                    m_DebugText.text = $"{m_NavHitInfo.distance}";
                }
                // rotation
                var lookAt = m_PhysHitInfo.point;
                lookAt.y = transform.position.y;
                transform.LookAt(lookAt, Vector3.up);
                // apply calculated velocity
                m_Velocity = velocity;
            }
        }
    }
}
