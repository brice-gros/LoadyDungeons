using System;
using System.Collections;
using UnityEngine;

[RequireComponent (typeof(Animator))]
public class Chest : MonoBehaviour
{
    private Animator m_Animator;
    private AudioSource m_AudioSource;
    internal bool m_Opened = false;
    

    [SerializeField]
    private ParticleSystem m_ParticleSystem;

    [SerializeField]
    private float m_ParticlePlayDelayTime = 0.75f;

    [SerializeField]
    private GameObject m_Key;
    
    public string m_HatName;

    [SerializeField]
    private float m_KeyDestroyDelayTime = 0.75f;

    // Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_AudioSource = GetComponent<AudioSource>();
    }

    public bool HasKey() {
        return m_Key != null;
    }

    public void Open()
    {
        if (m_Opened == false) {
            m_Opened = true;
            StartCoroutine(PlayOpen());
        }
    }

    private IEnumerator PlayOpen()
    {
        if (m_Key) m_Key.SetActive(true);
        if (m_AudioSource) m_AudioSource.Play();
        m_Animator.SetTrigger("Open");
        yield return new WaitForSeconds(m_ParticlePlayDelayTime);
        m_ParticleSystem.Play();

        yield return new WaitForSeconds(m_KeyDestroyDelayTime);
        if (m_Key) m_Key.SetActive(false);
    }

}
