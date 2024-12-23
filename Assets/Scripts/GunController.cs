using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField]
    private Gun currentWeapon;
    bool auto;
    GameManager gameManager;
    [SerializeField] Animator animator;
    

    void Start()
    {
        UpdateStats();
        gameManager = GameManager.Instance;
    }

    void Update()
    {
        
        if (gameManager.MatchOver)
            return;
        if (auto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0))
        {
            currentWeapon.Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(currentWeapon.Reload());
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!currentWeapon.gunLocked)
            {
                currentWeapon.SwapGun();
                UpdateStats();
            }
        }
    }

    void UpdateStats()
    {
        auto = currentWeapon.IsAuto();
    }
}
