using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hzRDWController : MonoBehaviour
{
    private RedirectionManager redirectionManager;

    // Start is called before the first frame update
    void Start()
    {
        redirectionManager = transform.GetComponentInChildren<RedirectionManager>();
    }

    // Update is called once per frame
    void Update()
    {
        redirectionManager.MakeOneStepRedirection();
    }
}
