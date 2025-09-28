using UnityEngine;

public class Rajas_Nanda : MonoBehaviour
{
    public GameObject s;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(s, new Vector3(0, 0, 0), Quaternion.identity);
        } 
    }
}
