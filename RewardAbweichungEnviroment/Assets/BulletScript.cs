using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public RollerAgentB rollerAgentB;
    public RollerAgentA rollerAgentA;
    public Collider targetCollider;
    public Collider BigTriggerSphere;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Assign references after the bullet is instantiated
        rollerAgentB = Object.FindAnyObjectByType<RollerAgentB>(); // Finds the first instance of RollerAgentB in the scene
        rollerAgentA = Object.FindAnyObjectByType<RollerAgentA>(); // Finds the first instance of RollerAgentB in the scene

        // If you need to assign a specific target collider, find it like this:
        targetCollider = rollerAgentA.GetComponent<Collider>();
        BigTriggerSphere = rollerAgentA.transform.Find("BigCollider").GetComponent<Collider>();
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null & collision.collider == targetCollider) 
        {
            Debug.Log("reward was Granted for shooting Green");
            rollerAgentB.GrantReward(1);
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other == BigTriggerSphere)
        {
            Debug.Log("Small reward was granted for shooting close to green.");
            rollerAgentB.GrantReward(0.2f);
        }
    }
}
