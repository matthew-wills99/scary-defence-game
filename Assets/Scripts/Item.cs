using UnityEngine;

public class Item : InteractiveObject
{
    public override void Interact()
    {
        Debug.Log("Picked up item");
    }
}
