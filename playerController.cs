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
    public float gravityForce;
    [SerializeField]float gravity = -5f;
    [SerializeField]float jumpForce;

    bool frontCollision, backCollision, rightCollision, leftCollision;

    Vector3 velocity;
    byte equippedBlock = 1;

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
        checkCollisions();
        jump();
        Movement();    
        GravityCalc();
        groundCheck();
    }

    private void FixedUpdate() {
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
        if(!groundCheck())
        {
            if(headCollision()) {
                gravityForce = 0f;
            }
            if(gravityForce <= gravity) {
                gravityForce = gravity;
                return;
            }
            gravityForce += gravity * Time.deltaTime;
        }
        else {
            gravityForce = 0f;
        }
    }

    bool groundCheck()
    {
        if(gravityForce > 0f)
        {
            return false;
        }

        else
        {
            if (
                world.CheckVoxel(new Vector3(transform.position.x - 0.10f, transform.position.y - 2f, transform.position.z - 0.10f)) ||
                world.CheckVoxel(new Vector3(transform.position.x + 0.10f, transform.position.y - 2f, transform.position.z - 0.10f)) ||
                world.CheckVoxel(new Vector3(transform.position.x + 0.10f, transform.position.y - 2f, transform.position.z + 0.10f)) ||
                world.CheckVoxel(new Vector3(transform.position.x - 0.10f, transform.position.y - 2f, transform.position.z + 0.10f))
            ) 
            {              
                float iY = Mathf.FloorToInt(transform.position.y + 1);
                if(transform.position.y > Mathf.FloorToInt(transform.position.y)  && isJumping == true) {
                    transform.position = new Vector3(transform.position.x, iY, transform.position.z);
                }
                isJumping = false;
                return true;
            }

            else {
                return false;
            }
        }

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
        if(world.CheckVoxel(new Vector3(transform.position.x, transform.position.y - 1.5f, transform.position.z + 0.15f)) || world.CheckVoxel(new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z + 0.15f))) {
            frontCollision = true;
        } else {frontCollision = false;}
        
        //BACK CHECK
        if(world.CheckVoxel(new Vector3(transform.position.x, transform.position.y - 1.5f, transform.position.z - 0.15f)) || world.CheckVoxel(new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z - 0.15f))) {
            backCollision = true;
        } else {backCollision = false;}

        //RIGHT CHECK
        if(world.CheckVoxel(new Vector3(transform.position.x + 0.15f, transform.position.y - 1.5f, transform.position.z)) || world.CheckVoxel(new Vector3(transform.position.x + 0.15f, transform.position.y - 0.5f, transform.position.z))) {
            rightCollision = true;
        } else {rightCollision = false;}

        //LEFT CHECK
        if(world.CheckVoxel(new Vector3(transform.position.x - 0.15f, transform.position.y - 1.5f, transform.position.z)) || world.CheckVoxel(new Vector3(transform.position.x - 0.15f, transform.position.y - 0.5f, transform.position.z))) {
            leftCollision = true;
        } else {leftCollision = false;}
    }
    
    void Inputs() {
        if(groundCheck() && isJumping == false && Input.GetButtonDown("Jump")) {
            //Debug.Log("eua");
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
            Vector3 pPos = new Vector3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
            if(pPos == placePos || pPos - new Vector3(0f, 1f, 0f) == placePos)
            {
                return;
            }
            world.GetChunk(placePos).ChangeChunkMap(placePos, equippedBlock);
        }

        if(Input.inputString != "0") {
            try {
                equippedBlock = byte.Parse(Input.inputString);
            }
            catch { return; }
        }

        //equippedBlock = byte.Parse(Input.inputString);
        Debug.Log(equippedBlock);
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
