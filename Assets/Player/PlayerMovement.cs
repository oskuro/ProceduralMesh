using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #pragma warning disable 0649 
    [SerializeField] float playerSpeed = 10f;
    [SerializeField] Camera cam;
    [SerializeField] float cameraHorizontalSpeed = 5f;
    [SerializeField] float cameraVerticalSpeed = 3f;
    [SerializeField] LayerMask groundLayer;
    #pragma warning restore 0649 
    float movement = 0;
    //float gravity = 9.81f;
    bool jumping = false;
    float jumpHeight = 5f;
    float jumpSpeed = 5f;
    float  jumpTarget = 0f;

    void Start()
    {
        if(cam == null)
        {
            Debug.Log("We got no first person camera. exiting");
            Destroy(this);
        }
    }


    void Update()
    {
        ApplyGravity();
        Move();
        MouseLook();
        Jump();
    }

    private void Move()
    {
        movement = Input.GetAxis("Vertical");

        transform.Translate(Vector3.forward * (movement * playerSpeed) * Time.deltaTime);
    }

    private void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y") * -1;

        transform.Rotate(0, mouseX * cameraHorizontalSpeed, 0);
        cam.transform.Rotate(mouseY * cameraVerticalSpeed, 0, 0);
    }

    private void ApplyGravity()
    {
        //if (!IsGrounded())
        //{
        //    transform.Translate(Vector3.down * gravity * Time.deltaTime);
        //}
    }

    private bool IsGrounded()
    {
        bool isGrounded = false;

        RaycastHit hitInfo = new RaycastHit();

        Vector3 spherePosition = transform.position;
        spherePosition.y -= 0.5f;
        #pragma warning disable 0642
        if (Physics.SphereCast(spherePosition, 0.6f, Vector3.zero, out hitInfo, groundLayer));
        {
            isGrounded = true;
        }
        #pragma warning restore 0642
        return isGrounded;
    }

    private void Jump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && IsGrounded() && !jumping)
        {
            jumping = true;
            jumpTarget = transform.position.y + jumpHeight;
        }

        if (jumping)
        {
            if (Mathf.Abs(transform.position.y) - Mathf.Abs(jumpHeight) < 0.3) 
            {
                Vector3 jumpPosition = new Vector3(0, jumpHeight, 0);
                transform.Translate(jumpPosition * jumpSpeed * Time.deltaTime);
            } else
            {
                jumping = false;
            }
        }
    }

    public void OnDrawGizmos()
    {
        Vector3 spherePosition = transform.position;
        spherePosition.y -= 0.5f;
        Gizmos.DrawWireSphere(spherePosition, 0.6f);
    }
}
