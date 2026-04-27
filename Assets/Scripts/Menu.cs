using UnityEngine;
using UnityEngine.Video;

public class Menu : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject menuOpcoes, rawImage;
    void Start()
    {
        rawImage.SetActive(false);
    }

    
    void Update()
    {
        if (!videoPlayer.isPlaying && Input.anyKeyDown)
        {
            videoPlayer.Play();
            rawImage.SetActive(true);
            menuOpcoes.SetActive(true);
        }
    }
}
