using UnityEngine; 

using UnityEngine.InputSystem; 

public class PlayerMove : MonoBehaviour 
{ 
    public float moveSpeed = 5f; 
    private Vector2 moveInput = Vector2.zero; 

    private Rigidbody2D rb; 

    private PlayerInput playerInput; 

    private InputAction moveAction; 

    void Awake() 
    { 
        rb = GetComponent<Rigidbody2D>(); 
        playerInput = GetComponent<PlayerInput>(); 

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
        if (Keyboard.current.wKey.isPressed) Debug.Log("W is pressed"); 
    } 
    void FixedUpdate() 
    { 
        if (rb == null) return;
        Vector2 newPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime; 
        rb.MovePosition(newPosition);
    } 
}