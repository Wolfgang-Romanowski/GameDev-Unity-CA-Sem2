using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.AI;

public class SlidingDoor : MonoBehaviour
{
    [SerializeField] private NavMeshObstacle obstacle;
    [SerializeField] private float slideDistance = 3f;
    [SerializeField] private float slideSpeed = 3f;
    [SerializeField] private float stayOpenTime = 3f;
    [SerializeField] private float interactDistance = 6f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private bool isAnimating = false;
    private bool hasBeenUnlocked = false;
    private Transform playerTransform;
    private Transform guardTransform;
    private GuardMovement guardMovement;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * slideDistance;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        GameObject guard = GameObject.Find("Guard");
        if (guard != null)
        {
            guardTransform = guard.transform;
            guardMovement = guard.GetComponent<GuardMovement>();
        }

        if (obstacle != null) obstacle.enabled = true;
    }

    void Update()
    {
        if (isAnimating) return;

        //use closed position so distance stays consistent while door slides up
        float playerDist = playerTransform != null ?
            Vector3.Distance(closedPosition, playerTransform.position) : float.MaxValue;
        float guardDist = guardTransform != null ?
            Vector3.Distance(closedPosition, guardTransform.position) : float.MaxValue;

        bool playerNear = playerDist < interactDistance;
        bool guardNear = guardDist < interactDistance;
        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        // player must unlock the door first, then either player or guard can open it
        if (playerNear && ePressed && (!hasBeenUnlocked || !isOpen))
        {
            hasBeenUnlocked = true;
            StartCoroutine(OpenDoor());
            return;
        }

        if (guardNear && hasBeenUnlocked && !isOpen)
        {
            StartCoroutine(OpenDoor());
            return;
        }

        if (isOpen && !playerNear && !guardNear)
            StartCoroutine(CloseDoor());
    }

    IEnumerator OpenDoor()
    {
        isAnimating = true;

        //disable obstacle so navmesh carving clears the blockage
        if (obstacle != null) obstacle.enabled = false;

        //one frame delay lets the navmesh update before we move
        yield return null;

        while (Vector3.Distance(transform.position, openPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, openPosition, slideSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = openPosition;

        isOpen = true;
        isAnimating = false;

        if (guardMovement != null)
            guardMovement.ForceRepath();
    }

    IEnumerator CloseDoor()
    {
        isAnimating = true;

        yield return new WaitForSeconds(stayOpenTime);

        while (Vector3.Distance(transform.position, closedPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, closedPosition, slideSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = closedPosition;

        //once unlocked the obstacle stays off so the guard can always reopen
        //the guard triggers the door open by proximity before reaching it
        if (!hasBeenUnlocked && obstacle != null)
            obstacle.enabled = true;

        isOpen = false;
        isAnimating = false;
    }
}