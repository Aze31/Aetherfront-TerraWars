using UnityEngine; 
using UnityEngine.InputSystem; 

public class PlayerMove : MonoBehaviour 
{ 
    public float moveSpeed = 5f; 
    private Vector2 moveInput = Vector2.zero; 
    private Rigidbody2D rb; 
    private PlayerInput playerInput; 
    private InputAction moveAction; 
    public Animator anim;

    void Awake() 
    { 
        rb = GetComponent<Rigidbody2D>(); 
        playerInput = GetComponent<PlayerInput>(); 
        anim = GetComponent<Animator>();

        if (playerInput == null) 
        { 
            Debug.LogError("PlayerInput component missing from Wizard!"); 
            return; 
        } 
        moveAction = playerInput.actions["moveAction"]; 
        if (moveAction == null) 
        { 
            Debug.LogError("Move action not found! Check your Input Actions asset."); 
        } 
        playerInput.actions.FindActionMap("moveMap").Enable();

    } 
    void Update() 
    { 
        if (moveAction != null) 
        { 
            moveInput = moveAction.ReadValue<Vector2>().normalized; 
        } 
        else 
        { 
            Debug.LogWarning("Move action is null in Update()"); 
        }
        if (Keyboard.current.wKey.isPressed)
        {
            anim.SetBool("WUp", true);
        }        else
        {
            anim.SetBool("WUp", false);
        }
            if (Keyboard.current.aKey.isPressed)
        {
            anim.SetBool("WLeft", true);
        }        else
        {
            anim.SetBool("WLeft", false);
        }
            if (Keyboard.current.sKey.isPressed)
        {
            anim.SetBool("WDown",true);
        }        else
        {
            anim.SetBool("WDown", false);
        }
            if (Keyboard.current.dKey.isPressed)
        {
            anim.SetBool("WRight", true);
        }         else
        {
            anim.SetBool("WRight", false);
        }
    } 
    void FixedUpdate() //wonder why this has to be fixedUpdate
    { 
        if (rb == null) return;
        Vector2 newPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime; 
        rb.MovePosition(newPosition);
    } 
}