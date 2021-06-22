using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float m_MovementSpeed = 5.0f;

    [SerializeField]
    private LayerMask m_InputCollisionLayer;

    private Animator m_AnimatorController;

    private bool m_HasKey = false;

    private Rigidbody m_Rigidbody;

    private int m_VelocityHash = Animator.StringToHash("Velocity");

    private Camera m_MainCamera;
    
    [SerializeField]
    private GameObject m_level;
    
    private RaycastHit m_PhysHitInfo;
    private NavMeshHit m_NavHitInfo;

    private MeshCollider m_MeshCollider;
   
    const float k_MinMovementDistance = 1.2f;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_AnimatorController = GetComponent<Animator>();
        m_MainCamera = Camera.main;
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

        //TODO: Put this outside of the PlayerController
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

                other.gameObject.GetComponent<Door>().Open();

                // TODO: Change this number to a member variable
                StartCoroutine(LevelCompleted());
            }
        }
    }

    private IEnumerator LevelCompleted()
    {
        yield return new WaitForSeconds(2.15f);
        m_Rigidbody.transform.LookAt(m_MainCamera.transform.position, Vector3.up);
        m_Rigidbody.velocity = (m_MainCamera.transform.position - m_Rigidbody.transform.position).normalized  * m_MovementSpeed;
        m_AnimatorController.Play("Dino_Victory");
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
        
        // apply animation

        if(m_AnimatorController != null)
            m_AnimatorController.SetFloat(m_VelocityHash, m_Rigidbody.velocity.magnitude);

        var cameraLookAtPos = m_Rigidbody.transform.position + Vector3.up * 3.0F;
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
            if (Vector3.Distance(m_Rigidbody.position, m_PhysHitInfo.point) > k_MinMovementDistance)
            {
                Debug.DrawLine(m_Rigidbody.position, m_PhysHitInfo.point, Color.red, 5);
                // calculate move direction vector
                Vector3 movementDirection = m_PhysHitInfo.point - m_Rigidbody.position;    
                movementDirection.Normalize();
                var velocity = movementDirection * m_MovementSpeed;
                
                if (!NavMesh.Raycast(m_Rigidbody.position, m_PhysHitInfo.point, out m_NavHitInfo, NavMesh.AllAreas)) {
                    // rotation
                    m_Rigidbody.transform.LookAt(m_PhysHitInfo.point, Vector3.up);
                    // lock rotation to y 
                    Vector3 eulerAngle = m_Rigidbody.transform.eulerAngles;
                    m_Rigidbody.transform.eulerAngles = new Vector3(0, eulerAngle.y, 0);

                    

                    // apply calculated velocity
                    m_Rigidbody.velocity = movementDirection * m_MovementSpeed;
                }
            }
        }
    }
}
