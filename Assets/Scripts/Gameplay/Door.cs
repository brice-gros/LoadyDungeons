using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator m_Animator;
    private AudioSource m_AudioSource;

    // Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_AudioSource = GetComponent<AudioSource>();
    }

    public void Open()
    {
        m_AudioSource.Play();
        m_Animator.SetTrigger("Open");
    }
}
