using UnityEngine;

public class BulletController : MonoBehaviour
{

    [SerializeField]
    private GameObject bulletDecal;

    private float speed = 50f;
    private float timeToDestroy = 3f;

    public Vector3 target
    {get; set; }
    public bool hit
    {get; set; }

    private void OnEnable()
    {
        Destroy(gameObject, timeToDestroy);



    }
    private void OnDisable()
        {
       
        }
  



    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (!hit && Vector3.Distance(transform.position, target) < .01f)
        {
           Destroy(gameObject);
        }
    }
    private void OnCollisionEnter(Collision other)
    {
        //ContactPoint contact = other.GetContact(0);
        //GameObject.Instatiate(bulletDecal, contact.point, Quaternian.LookRotation(contact.normal));
       // Destoy(gameObject);
    }

}
