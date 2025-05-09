using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Door : InteractiveObject
{
    [SerializeField] public bool defaultOpenState = false;
    [SerializeField] public float openRotation = 90f; // 90 degree opening angle
    [SerializeField] public float timeToOpen = 2f; // 2 seconds for door to open
    [SerializeField] public GameObject doorPivot;
    [SerializeField] public float cooldownTime = 2f; // 2 second cooldown after using

    private bool isOpen = false;
    private bool isOnCooldown = false;

    private void Awake()
    {
        isOpen = defaultOpenState;
    } 

    // Interact method specific to doors
    public override void Interact()
    {
        if(doorPivot == null) // error
        {
            Debug.LogWarning($"No door pivot");
            return;
        }
        
        if(isOnCooldown)
        {
            Debug.Log("Door is on cooldown.");
            return;
        }

        isOpen = !isOpen; // flip open state
        
        if(isOpen)
        {
            Debug.Log($"Opening Door");
            StartCoroutine(ChangeDoorRotation(openRotation, timeToOpen));
        }
        else
        {
            Debug.Log("Closing Door");
            StartCoroutine(ChangeDoorRotation(0f, timeToOpen));
        }

        StartCoroutine(Cooldown());
    }

    private IEnumerator ChangeDoorRotation(float targetAngle, float duration)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z);

        float elapsedTime = 0f;

        while(elapsedTime < duration)
        {
            doorPivot.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        doorPivot.transform.rotation = targetRotation;
    }

    private IEnumerator Cooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }
}
