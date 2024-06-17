using UnityEngine;

public class EventManager : MonoBehaviour
{
    static EventManager _instance;
    
    public GameObject player;
    public Gun playerGun;
    public TPSMovement playerMove;
    public TPSCamera playerCam;
    public TextWindow canvasTextWindow;
    
    public bool greaterGunLock;
    public bool infiniteAmmo;

    public static EventManager Instance
    {
        get
        {
            if(_instance == null)
                Debug.LogError("Event manager é nulo");
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
        playerCam = FindObjectOfType<TPSCamera>();
        canvasTextWindow = FindObjectOfType<TextWindow>();
        if (playerCam)
        {
            player = playerCam.gameObject;
            playerMove = player.GetComponent<TPSMovement>();
            playerGun = player.GetComponentInChildren<Gun>();
        }
    }

    public void ToggleGreaterLock(bool enable)
    {
        greaterGunLock = enable;
        playerGun.gunLocked = enable;
    }

    public void InfiniteAmmo(bool enable) { infiniteAmmo = enable; }
    
    public void ToggleFocus(bool enable) { playerCam.canFocus = enable; }
    
    public void ToggleCamera(bool enable) { playerCam.canMove = enable; }

    public void GiveNewGun(GunData gun)
    {
        if (playerGun.guns[1] == null)
        {
            playerGun.guns[1] = gun;
            playerGun.AcquireWeapon(1);
        } else
        {
            playerGun.guns[playerGun.gunID] = gun;
            playerGun.AcquireWeapon(playerGun.gunID);
        }
    }

    public void ForceReleaseMouse()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    
}