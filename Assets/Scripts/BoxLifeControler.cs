using UnityEngine;

public class BoxLifeControler : MonoBehaviour
{
    public float distanceToDetect = 2;
    public float value = 10;
    public GameObject txtGetLife;
    private Transform player;
    private GameObject goatHeal;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        goatHeal = GameObject.FindGameObjectWithTag("buchada");
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) <= distanceToDetect)
        {
            txtGetLife.SetActive(true);
            if (Input.GetKey(KeyCode.E))
            {
                player.GetComponent<PlayerHealth>().AddLife(value);
                goatHeal.GetComponent<GoatHealth>().Heal(value);
                txtGetLife.SetActive(false);
                Destroy(gameObject);
            }
        }
        else
        {
            txtGetLife.SetActive(false);
        }
    }
}
