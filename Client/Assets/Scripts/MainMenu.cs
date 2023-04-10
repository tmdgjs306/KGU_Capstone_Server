using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    public GameObject audioSlider; // ����� ��ư ������ �� �����̴��� ���̰� �ϱ����� ����

    public void Play()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void Quit()
    {
        Debug.Log("Quit");
    }

    public void Start()
    {
        audioSlider.SetActive(false);
    }

    public void ShowAudio()
    {
        audioSlider.SetActive(!audioSlider.activeSelf);
    }
}
