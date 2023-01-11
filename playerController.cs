using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    [SerializeField] private float mouseSens;
    float rotation;
    [SerializeField] private int speed;

    public new Transform camera;

    float raycastMax = 6f;
    float raycastChecks;
    float raycastInc = 0.1f;

    Vector3 destroyPos = new Vector3();
    Vector3 placePos = new Vector3();
    worldScript world;

    public bool Jump = false;
    public bool isGrounded = true;
    public bool isJumping = false;
    float gravityForce;
    [SerializeField]float gravity = -5f;
    [SerializeField]float jumpForce;

    bool frontCollision, backCollision, rightCollision, leftCollision;

    Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        world = GameObject.FindGameObjectWithTag("World").GetComponent<worldScript>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    // Update is called once per frame
    void Update()
    {
        CameraLook();        
        Inputs();
        cursorRaycast();
        groundCheck();
        checkCollisions();
        jump();
        Movement();    
    }

    private void FixedUpdate() {
        GravityCalc();
    }

    void CameraLook() {
        float mouseX = Input.GetAxis("Mouse X") * Time.fixedDeltaTime * mouseSens;
        float mouseY = Input.GetAxis("Mouse Y") * Time.fixedDeltaTime * mouseSens;

        rotation -= mouseY;
        rotation = Mathf.Clamp(rotation, -90f, 90f);
        camera.localRotation = Quaternion.Euler(rotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void jump() {
        if(Jump) {
            gravityForce = jumpForce;
            isGrounded = false;
            Jump = false;
        }
    }

    void Movement() {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        velocity = (transform.right * inputX) + (transform.up * gravityForce) + (transform.forward * inputZ);

        if((frontCollision && velocity.z > 0f) || (backCollision && velocity.z < 0f)) {
            velocity.z = 0f;
        }

        if((rightCollision && velocity.x > 0f) || (leftCollision && velocity.x < 0f)) {
            velocity.x = 0f;
        }


        transform.position += velocity * speed * Time.deltaTime;
    }

    void GravityCalc() {
        if(!isGrounded)
        {
            if(headCollision()) {
                gravityForce = 0f;
            }
            if(gravityForce <= gravity) {
                gravityForce = gravity;
                return;
            }
            gravityForce -= 0.1f;
        }
    }

    void groundCheck()
    {
        //Vector3 onGroundcheck = new Vector3(transform.position.x + 0.15, transform.position.y - 2f, transform.localScale.z / 2 + transform.position.z);
        if (
            world.CheckVoxel(new Vector3(transform.position.x - 0.10f, transform.position.y - 2f, transform.position.z - 0.10f)) ||
            world.CheckVoxel(new Vector3(transform.position.x + 0.10f, transform.position.y - 2f, transform.position.z - 0.10f)) ||
            world.CheckVoxel(new Vector3(transform.position.x + 0.10f, transform.position.y - 2f, transform.position.z + 0.10f)) ||
            world.CheckVoxel(new Vector3(transform.position.x - 0.10f, transform.position.y - 2f, transform.position.z + 0.10f))
           ) 
           {              
            gravityForce = 0f;
            isGrounded = true;
            isJumping = false;
        }

        else {
            isGrounded = false;
        }
        
        /*if(!world.CheckVoxel(onGroundcheck))
        { 
            GravityCalc(); 
            isGrounded = false;
        }*/
    }

    bool headCollision() {
        if(world.CheckVoxel(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z))) {
            return true;
        }
        else {
            return false;
        }
    }

    void checkCollisions() {
        //FRONT CHECK
        if(world.CheckVoxel(new Vector3(transform.position.x, transform.position.y - 1.9f, transform.position.z + 0.15f))) {
            frontCollision = true;
        } else {frontCollision = false;}
        
        if(world.CheckVoxel(new Vector3(transform.position.x, transform.position.y - 1.9f, transform.position.z - 0.15f))) {
            backCollision = true;
        } else {backCollision = false;}

        if(world.CheckVoxel(new Vector3(transform.position.x + 0.15f, transform.position.y - 1.9f, transform.position.z))) {
            rightCollision = true;
        } else {rightCollision = false;}

        if(world.CheckVoxel(new Vector3(transform.position.x - 0.15f, transform.position.y - 1.9f, transform.position.z))) {
            leftCollision = true;
        } else {leftCollision = false;}
    }
    
    void Inputs() {
        if( isGrounded && isJumping == false && Input.GetButtonDown("Jump")) {
            Debug.Log("eua");
            Jump = true;
            isJumping = true;
        }

        if(Cursor.lockState == CursorLockMode.Locked && Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.None;
        }
        if(Cursor.lockState != CursorLockMode.Locked && Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            world.GetChunk(destroyPos).ChangeChunkMap(destroyPos, 0);
        }

        if(Input.GetKeyDown(KeyCode.Mouse1)){
            world.GetChunk(placePos).ChangeChunkMap(placePos, 2);
        }

    }

    void cursorRaycast(){
        float raycastChecks = raycastInc;
        while (raycastChecks < raycastMax) {
            Vector3 pos = camera.position + (raycastChecks * camera.forward);
            if(world.CheckVoxel(pos))
            {
                destroyPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                Vector3 p = pos - (raycastInc * camera.forward);
                placePos = new Vector3(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y), Mathf.FloorToInt(p.z));
                return;
            }
            raycastChecks += raycastInc;
        }
    }
}
