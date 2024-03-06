
using UnityEngine;
using Cursor = UnityEngine.Cursor;

public class DebugMenu : MonoBehaviour
{
    TPSMovement move;
    TPSCamera cam;
    
    [SerializeField] GameObject debugMenu;
    [SerializeField] GameObject player;
    [SerializeField] KeyCode debugOpenKey;

    void Start()
    {
        cam = player.GetComponent<TPSCamera>();
        move = player.GetComponent<TPSMovement>();
    }

    void Update()
    {
        if (Input.GetKeyDown(debugOpenKey))
        {
            bool active = !debugMenu.activeInHierarchy;
            debugMenu.SetActive(active);
            Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = active;
            cam.canMove = move.canMove = !active;
        }
            
    }
}
