using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float runSpeed = 5;
    public float acceleration = 20;
    public float jumpSpeed = 7;
    public float gravity = 15;
    public Vector2 influence = new Vector2(10, 4);
    public float maxAirJumps = 1;
    public GameObject cursor;
    public GameObject knife;
    public float knifeVelocity = 9;
    public float knifeAcceleration = 15;
    public LayerMask groundLayer;
    public Vector3 knifeOffset = new Vector2(0,0);


    private float nextKnifeVelocity = 0;

    Vector3 defaultScale;
    float groundY;
    public bool grounded = false;
    float stateStartTime;
    RaycastHit2D hit;
    LaunchArcRenderer larc;

    float timeInState
    {
        get { return Time.time - stateStartTime; }
    }

    enum State
    {
        Idle,
        RunningRight,
        RunningLeft,
        JumpingUp,
        JumpingDown,
        Landing
    }
    State state;
    Vector2 velocity;
    float horzInput;
    bool jumpJustPressed;
    bool jumpHeld;
    int airJumpsDone = 0;

	// Use this for initialization
	void Start () {
        defaultScale = transform.localScale;
        groundY = transform.position.y; //Replace with raycasting
        larc = GetComponent<LaunchArcRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        //Inputs
        horzInput = Input.GetAxisRaw("Horizontal");
        jumpJustPressed = Input.GetButtonDown("Jump");
        jumpHeld = Input.GetButton("Jump");

        ContinueState();
        UpdateTransform();

        ApplyMouseInput();
	}

    void SetOrKeepState(State state)
    {
        if (this.state == state) return;
        EnterState(state);
    }

    void ExitState()
    {

    }

    void EnterState(State state)
    {
        ExitState();
        switch (state)
        {
            case State.Idle:
                //do idle things
                break;
            case State.RunningLeft:
                Face(-1);
                break;
            case State.RunningRight:
                Face(1);
                break;
            case State.JumpingUp:
                velocity.y = jumpSpeed;
                break;
            case State.Landing:
                airJumpsDone = 0;
                break;
        }
        this.state = state;
        stateStartTime = Time.time;
    }

    void ContinueState()
    {
        switch (state)
        {
            case State.Idle:
                RunOrJump();
                break;

            case State.RunningLeft:
            case State.RunningRight:
                if (!RunOrJump()) EnterState(State.Idle);
                break;
            case State.JumpingUp:
                if (velocity.y < 0) EnterState(State.JumpingDown);
                if (jumpJustPressed && airJumpsDone < maxAirJumps)
                {
                    EnterState(State.JumpingUp);
                    airJumpsDone++;
                }
                break;

            case State.JumpingDown:
                if (grounded) EnterState(State.Landing);
                if (jumpJustPressed && airJumpsDone < maxAirJumps)
                {
                    EnterState(State.JumpingUp);
                    airJumpsDone++;
                }
                break;
            case State.Landing:
                if (timeInState > 0.2f) EnterState(State.Idle);
                else if (timeInState > 0.01f) RunOrJump();
                break;
        }
    }
    
    bool RunOrJump()
    {
        if (jumpJustPressed && grounded) SetOrKeepState(State.JumpingUp);
        else if (horzInput < 0) SetOrKeepState(State.RunningLeft);
        else if (horzInput > 0) SetOrKeepState(State.RunningRight);
        else return false;
        return true;
    }

    void Face(int direction)
    {
        transform.localScale = new Vector3(defaultScale.x * direction, defaultScale.y, defaultScale.z);
    }

    void UpdateTransform()
    {
        if (grounded)
        {
            float targetSpeed = 0;
            switch (state)
            {
                case State.RunningLeft:
                    targetSpeed = -runSpeed;
                    break;
                case State.RunningRight:
                    targetSpeed = runSpeed;
                    break;
            }
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, acceleration * Time.deltaTime);
            
        }
        else
        {
            if (jumpHeld) velocity.y += influence.y * Time.deltaTime;

            float targetSpeed = horzInput * runSpeed;
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, influence.x * Time.deltaTime);
            
        }
        //velocity.y -= gravity * Time.deltaTime;

        Vector3 newPos = transform.position + (Vector3)(velocity * Time.deltaTime);
        if (IsGrounded(newPos) && velocity.y<=0)
        {
            newPos.y = hit.point.y + (GetComponent<CapsuleCollider2D>().size.y/2) ;
            velocity.y = 0;
            grounded = true;
        }
        else
        {
            grounded = false;
            velocity.y -= gravity * Time.deltaTime;
        }
        transform.position = newPos;
        
       
    }

    bool IsGrounded(Vector2 pos)
    {

        Vector2 direction = Vector2.down;
        CapsuleCollider2D cc2d = GetComponent<CapsuleCollider2D>();
        float distance = 0.03f;
        Debug.DrawRay(pos, direction * distance, Color.green);
        hit = Physics2D.Raycast(new Vector2(pos.x,cc2d.bounds.min.y), direction * distance, distance, groundLayer);
        if (hit.collider != null)
            return true;
        Vector2 shiftedPos = new Vector2(cc2d.bounds.min.x, cc2d.bounds.min.y);
        Debug.DrawRay(shiftedPos, direction * distance, Color.green);
        hit = Physics2D.Raycast(shiftedPos, direction * distance, distance, groundLayer);
        if (hit.collider != null)
            return true;
        shiftedPos = new Vector2(cc2d.bounds.max.x, cc2d.bounds.min.y);
        Debug.DrawRay(shiftedPos, direction * distance, Color.green);
        hit = Physics2D.Raycast(shiftedPos, direction * distance, distance, groundLayer);
        if (hit.collider != null)
            return true;

        return false;
    }

    void ApplyMouseInput()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = 10;
        cursor.transform.position = Camera.main.ScreenToWorldPoint(pos);
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Ready...");
            nextKnifeVelocity = 8.0f;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Debug.Log(nextKnifeVelocity.ToString());
            GameObject newKnife = Instantiate(knife, transform.position + knifeOffset, Quaternion.identity);
            ProjectileMover knifeScript = newKnife.GetComponent<ProjectileMover>();
            Vector2 knifeVector = new Vector2((cursor.transform.position.x - this.transform.position.x), (cursor.transform.position.y - this.transform.position.y));
            knifeVector = knifeVector.normalized * nextKnifeVelocity;
            knifeScript.velocity = knifeVector;
        }
        else if (Input.GetMouseButton(0))
        {
            nextKnifeVelocity = Mathf.MoveTowards(nextKnifeVelocity, knifeVelocity, knifeAcceleration * Time.unscaledDeltaTime);
            Vector2 knifeVector = new Vector2((cursor.transform.position.x - this.transform.position.x), (cursor.transform.position.y - this.transform.position.y));
            knifeVector = knifeVector.normalized * nextKnifeVelocity;
            larc.RenderArc(knifeVector);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Time.timeScale = .1f;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            Time.timeScale = 1.0f;
        }
    }

}
