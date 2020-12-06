using UnityEngine;

public class Parenting : MonoBehaviour {
    public GameObject child;

    public Transform parent;

    //Invoked when a button is clicked.
    public void Example(Transform newParent) {
        child.transform.SetParent(newParent);
    }
}
