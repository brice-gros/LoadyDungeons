using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuLogic : MonoBehaviour
{
//Make sure to attach these Buttons in the Inspector
    public Button m_startButton, m_storeButton;

    void Start()
    {
        //Calls the TaskOnClick/TaskWithParameters/ButtonClicked method when you click the Button
        m_startButton.onClick.AddListener(StartClicked);
        m_storeButton.onClick.AddListener(() => StoreClicked(true));
    }

    void StartClicked()
    {
        SceneManager.LoadScene("Level_00");
    }

    void StoreClicked(bool auto)
    {
        SceneMode.auto = auto;
        SceneManager.LoadScene("Level_00");;
    }

}
