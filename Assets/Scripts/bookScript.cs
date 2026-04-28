using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class bookScript : MonoBehaviour
{
    public float distanceToDetect = 2;
    public GameObject cartaText;
    public GameObject interectPrompt;
    private Transform player;
    private bool isCartaOnScream = false;
    public bool isRead;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        interectPrompt.SetActive(false);
        cartaText.SetActive(false);
        isRead = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(transform.position, player.position) <= distanceToDetect)
        {
            interectPrompt.SetActive(true);
            if(Input.GetKeyDown(KeyCode.E))
            {
                isCartaOnScream = !isCartaOnScream;
                cartaText.SetActive(isCartaOnScream);
                isRead = (true);
            }
        } else
        {
            interectPrompt.SetActive(false);
        }
    }

    public bool GetIsRead()
    {
        return isRead;
    }
}
