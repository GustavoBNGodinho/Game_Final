using UnityEngine;

public class BoxBulletControler : MonoBehaviour
{
    public float distanceToDetect = 1;
    public float valeu = 6;
    public GameObject txtGetBullet;
    private Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(transform.position, player.position) <= distanceToDetect)
        {
            txtGetBullet.SetActive(true);
            if (Input.GetKey(KeyCode.E))
            {
                player.GetComponent<PlayerController>().AddBullet(valeu);
                txtGetBullet.SetActive(false);
                Destroy(gameObject);
            }
        }
        else
        {
            txtGetBullet.SetActive(false);
        }
    }
}
