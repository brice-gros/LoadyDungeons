using UnityEngine;

public class Door : MonoBehaviour
{
    internal bool m_Opened = false;
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
        if (m_Opened == false) {
            m_Opened = true;
            m_AudioSource.Play();
            m_Animator.SetTrigger("Open");
        }
    }
}
