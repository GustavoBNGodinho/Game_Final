using UnityEngine;
using UnityEngine.SceneManagement;

public class carControlller : MonoBehaviour
{
    private float distanceToDetect = 3;
    public GameObject itcCar;
    public GameObject book;
    private Transform player;
    bool isRead = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        itcCar.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Vector3.Distance(transform.position, player.position));
        if(Vector3.Distance(transform.position, player.position) <= distanceToDetect)
        {
            Debug.Log("uau");
            itcCar.SetActive(true);
            if(book.GetComponent<bookScript>().GetIsRead()) 
            {
                if(Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("aiiiiiiii aiiiiinn");
                    // SceneManager.LoadScene()
                }
            }
        }
    }
}
