using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    public GameObject audioSlider; // 오디오 버튼 눌렀을 때 슬라이더가 보이게 하기위한 변수

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
