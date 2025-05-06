using UnityEngine;
using System.Collections;
using Cinemachine;

public class ShiftManager : MonoBehaviour
{
    public Transform worldRoot;                // Rotating parent
    public GameObject whitePlatforms;          // White platform group
    public GameObject blackPlatforms;          // Black platform group
    public GameObject playerPrefab;            // Player prefab

    private GameObject currentPlayer;
    private bool isWhiteWorld = true;
    private bool isShifting = false;
    private float rotationSpeed = 180f;        // degrees per second
    public Vector3 savedPosition;
    public CinemachineVirtualCamera virtualCamera; // Reference to the virtual camera
    private void Start()
    {
        whitePlatforms.SetActive(true);
        blackPlatforms.SetActive(false);
        currentPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        virtualCamera.m_Follow = currentPlayer.transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isShifting)
        {
            StartCoroutine(ShiftWorld());
        }
    }
    private IEnumerator ShiftWorld()
    {
        isShifting = true;

        // Detach platforms so they don't rotate with worldRoot
        whitePlatforms.transform.SetParent(null);
        blackPlatforms.transform.SetParent(null);

        whitePlatforms.SetActive(!isWhiteWorld);
        blackPlatforms.SetActive(isWhiteWorld);

        savedPosition = currentPlayer.transform.position;
        Destroy(currentPlayer);

        // Move worldRoot to player position before rotation
        worldRoot.position = savedPosition;
        
        // Determine rotation direction
        Quaternion startRotation = worldRoot.rotation;
        Quaternion endRotation = isWhiteWorld
            ? startRotation * Quaternion.Euler(0, 0, 180)
            : startRotation * Quaternion.Euler(0, 0, -180);

        whitePlatforms.transform.SetParent(worldRoot);
        blackPlatforms.transform.SetParent(worldRoot);
        // Smooth rotate worldRoot
        while (Quaternion.Angle(worldRoot.rotation, endRotation) > 0.1f)
        {
            worldRoot.rotation = Quaternion.RotateTowards(worldRoot.rotation, endRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        worldRoot.rotation = endRotation;
       
        // Re-instantiate player at correct location
        if (isWhiteWorld)
        {
            currentPlayer = Instantiate(playerPrefab, new Vector3(savedPosition.x,savedPosition.y + 1f,savedPosition.z) , Quaternion.identity);
            //currentPlayer.GetComponent<PlayerController>().jumpForce = Mathf.Abs(currentPlayer.GetComponent<PlayerController>().jumpForce);
        }
        else
        {
            currentPlayer = Instantiate(playerPrefab, new Vector3(savedPosition.x, savedPosition.y + 1f, savedPosition.z), Quaternion.identity);
            //currentPlayer.GetComponent<PlayerController>().jumpForce = Mathf.Abs(currentPlayer.GetComponent<PlayerController>().jumpForce);
        }
        virtualCamera.m_Follow = currentPlayer.transform;
        isWhiteWorld = !isWhiteWorld;
        isShifting = false;
    }
}
