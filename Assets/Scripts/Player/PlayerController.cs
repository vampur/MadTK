using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public bool bBlockInput;

    // mvmt vars
    public float flAccelerationGround = 50f;
    public float flAccelerationGroundSprint = 120f;
    public float flAccelerationAir = 150f;
    public float flMaxSpeedGround = 7f;
    public float flMaxSpeedAir = 1f;
    public float flBaseFriction = 5f;
    public float flStopFriction = 15f;
    public float flJumpVel = 5f;
    public bool bAutoJump = true;
    public float flCurrentSpeed;
    bool bOnGround, bInJump;
    Vector3 vecMove = Vector3.zero;

    // view vars
    public float flMouseSensitivity = 1f;
    float flPitch, flYaw;

    // input vars
    float flForwardMove, flSideMove;
    float flMouseDeltaX, flMouseDeltaY;
    bool bJumpButton, bSprintButton;
    bool bTkGrabButton, bTkThrowButton, bTkDropButton, bTkFreezeButton;
    bool bFlashlight;

    // component vars
    CharacterController cc;
    Camera cam;
    Collider col;
    GameObject flashlight;

    // tk vars
    public float flTkThrowForce = 25f;
    public float flTkPullForce = 20f;
    public Vector3 vecTkHoldLocal = new Vector3(0f, -.5f, .75f);
    float flTkCurDist = 0f;
    List<Rigidbody> lstTkRbs;
    struct tkMoveData
    {
        public Rigidbody rbTk;
        public Vector3 vecTkForce;
        public bool bResetVel;
        public bool bDampVel;

        public tkMoveData(Rigidbody rbTk, Vector3 vecTkForce, bool bResetVel = false, bool bDampVel = false)
        {
            this.rbTk = rbTk;
            this.vecTkForce = vecTkForce;
            this.bResetVel = bResetVel;
            this.bDampVel = bDampVel;
        }
    }
    List<tkMoveData> lstTkMove;



    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        cam = Camera.main;
        col = GetComponent<Collider>();
        flashlight = GameObject.FindWithTag("Flashlight");

        lstTkRbs = new List<Rigidbody>();
        lstTkMove = new List<tkMoveData>();

        flYaw = cam.transform.rotation.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if(bBlockInput)
            return;
        
        GetInput();
        HandleLook();
        FindTkObjs();

        ApplyPlayerMove();
        ApplyTkMove();

        //ghetto fallen under the world check
        if (transform.position.y < -75f)
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void FixedUpdate()
    {
        UpdatePlayerMove();
        UpdateTkMove();
        //ApplyTkMove();
    }

    void GetInput()
    {
        flForwardMove = Input.GetAxisRaw("Vertical");
        flSideMove = Input.GetAxisRaw("Horizontal");

        flMouseDeltaX = Input.GetAxisRaw("Mouse X");
        flMouseDeltaY = Input.GetAxisRaw("Mouse Y");

        bJumpButton = Input.GetButton("Jump");
        bSprintButton = Input.GetButton("Sprint");

        bTkGrabButton = Input.GetButton("TkGrab");
        bTkThrowButton = Input.GetButton("TkThrow");
        bTkDropButton = Input.GetButton("TkDrop");
        bTkFreezeButton = Input.GetButton("TkFreeze");

        // ghetto flashlight button
        if (Input.GetButtonDown("Flashlight"))
            bFlashlight = !bFlashlight;
        flashlight.SetActive(bFlashlight);

        // ghetto restart level button
        if (Input.GetKey(KeyCode.F5))
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void HandleLook()
    {
        flPitch -= flMouseDeltaY * flMouseSensitivity;
        flPitch = Mathf.Clamp(flPitch, -89f, 89f);

        flYaw += flMouseDeltaX * flMouseSensitivity;

        cam.transform.rotation = Quaternion.Euler(flPitch, flYaw, 0f);
    }

    void FindTkObjs()
    {
        if (!(bTkGrabButton || bTkFreezeButton))
            return;

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, cam.farClipPlane, LayerMask.GetMask("TKable")))
        {
            if (!lstTkRbs.Contains(hit.rigidbody))
            {
                if (bTkFreezeButton)
                {
                    hit.rigidbody.isKinematic = true;
                    return;
                }

                hit.rigidbody.isKinematic = false;
                lstTkRbs.Add(hit.rigidbody);
            }
        }
    }

    void ApplyPlayerMove()
    {
        cc.Move(vecMove * Time.deltaTime);
    }

    void GroundCheck()
    {
        Vector3 vecFeetPos = transform.position;
        float flFeetOffset = 0.07f;
        vecFeetPos.y = vecFeetPos.y - cc.bounds.extents.y + cc.radius - cc.skinWidth - flFeetOffset;
        float flRadius = cc.radius - flFeetOffset / 2f;
        bOnGround = Physics.OverlapSphere(vecFeetPos, flRadius, ~LayerMask.GetMask("Player", "Trigger")).Length > 0;
    }

    // Source: https://adrianb.io/2015/02/14/bunnyhop.html
    // vecAccelDir: normalized direction that the player has requested to move (taking into account the movement keys and look direction)
    // vecPrevVel: The current velocity of the player, before any additional calculations
    // flAccelerate: The server-defined player acceleration value
    // flMaxSpd: The server-defined maximum player velocity (this is not strictly adhered to due to strafejumping)
    Vector3 Accelerate(Vector3 vecAccelDir, Vector3 vecPrevVel, float flAccelerate, float flMaxSpd)
    {
        float flProjSpd = Vector3.Dot(vecPrevVel, vecAccelDir); // Vector projection of Current velocity onto accelDir.
        float flAccelSpd = flAccelerate * Time.fixedDeltaTime; // Accelerated velocity in direction of movment

        // If necessary, truncate the accelerated velocity so the vector projection does not exceed flMaxSpd
        if (flProjSpd + flAccelSpd > flMaxSpd)
            flAccelSpd = flMaxSpd - flProjSpd;

        if (flAccelSpd < 0f)
            flAccelSpd = 0f;

        return vecPrevVel + vecAccelDir * flAccelSpd;
    }

    Vector3 MoveGround(Vector3 vecAccelDir, Vector3 vecPrevVel)
    {
        vecPrevVel.y = 0f;
        // Apply Friction
        float speed = vecPrevVel.magnitude;
        if (speed != 0) // To avoid divide by zero errors
        {
            float flFriction = (flForwardMove == flSideMove && flForwardMove == 0f ? flStopFriction : flBaseFriction);
            float drop = speed * flFriction * Time.fixedDeltaTime;
            vecPrevVel *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
        }

        float flAccel = flAccelerationGround;
        if (bSprintButton)
            flAccel = flAccelerationGroundSprint;

        // ground_accelerate and flMaxSpd_ground are server-defined movement variables
        return Accelerate(vecAccelDir, vecPrevVel, flAccel, flMaxSpeedGround);
    }

    Vector3 MoveAir(Vector3 vecAccelDir, Vector3 vecPrevVel)
    {
        vecPrevVel.y = 0f;
        // air_accelerate and flMaxSpd_air are server-defined movement variables
        return Accelerate(vecAccelDir, vecPrevVel, flAccelerationAir, flMaxSpeedAir);
    }

    Vector3 GetVerticalMove(Vector3 vecPrevVel)
    {
        Vector3 vecVertical = Vector3.zero;
        vecVertical.y = vecPrevVel.y;
        if (bOnGround)
        {
            if (bJumpButton && (bAutoJump || !bInJump))
            {
                vecVertical = new Vector3(0f, flJumpVel, 0f);
                bInJump = true;
                bOnGround = false;
            }
            else if (!bJumpButton)
            {
                bInJump = false;
            }
        }
        vecVertical += Physics.gravity * Time.fixedDeltaTime;

        return vecVertical;
    }

    Vector3 GetSlopeSlide(Vector3 vecPrevVel)
    {
        Vector3 vecSlopeSlide = Vector3.zero;
        Vector3 vecFallVel = Physics.gravity;
        vecFallVel.y += vecPrevVel.y;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, cc.radius, -transform.up, out hit, cc.bounds.extents.y + cc.skinWidth, ~LayerMask.GetMask("Player")))
        {
            if (Vector3.Angle(transform.up, hit.normal) >= 45f)
            {
                bOnGround = false;
                vecSlopeSlide = -(Vector3.Dot(vecFallVel, hit.normal) * hit.normal);
            }
        }

        //Debug.DrawLine(transform.position, transform.position + vecSlopeSlide, Color.magenta, .1f);

        return vecSlopeSlide * Time.fixedDeltaTime;
    }

    void UpdatePlayerMove()
    {
        transform.rotation = Quaternion.Euler(0f, flYaw, 0f);

        GroundCheck();

        Vector3 vecPrevVel = cc.velocity;

        Vector3 vecVertical = GetVerticalMove(vecPrevVel);
        Vector3 vecSlopeSlide = GetSlopeSlide(vecPrevVel);
        vecVertical.y += vecSlopeSlide.y;

        Vector3 vecAccelDir = ((transform.forward * flForwardMove) + (transform.right * flSideMove));
        vecAccelDir.y = 0f;
        vecAccelDir = vecAccelDir.normalized;

        Vector3 vecNewVel = bOnGround ? MoveGround(vecAccelDir, vecPrevVel) : MoveAir(vecAccelDir, vecPrevVel + vecSlopeSlide);
        flCurrentSpeed = vecNewVel.magnitude;
        vecNewVel += vecVertical;

        // try mitigate super bounce clipping issue
        if (vecNewVel.y > flJumpVel)
            vecNewVel.y = flJumpVel;

        vecMove = vecNewVel;
    }

    // DELETE ME DEBUG STUFF
    public Collider dbgCollider;

    void UpdateTkMove()
    {
        if (lstTkRbs.Count == 0)
            return;

        if (bTkDropButton)
        {
            lstTkRbs.Clear();
            lstTkMove.Clear();
            flTkCurDist = 0f;
            return;
        }

        if (bTkThrowButton ^ bTkGrabButton)
            flTkCurDist = 0f;

        Vector3 vecTarget = new Vector3();

        if (bTkThrowButton && !bTkGrabButton)
        {
            vecTarget = cam.transform.position + cam.transform.forward * cam.farClipPlane;
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, cam.farClipPlane, ~LayerMask.GetMask("Player", "UI", "Trigger")))
            {
                if (!lstTkRbs.Contains(hit.rigidbody) || Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, cam.farClipPlane, ~LayerMask.GetMask("Player", "Trigger", "UI", "TKable")))
                {
                    vecTarget = hit.point;
                    dbgCollider = hit.collider;
                }
            }
            foreach (var rbTk in lstTkRbs)
            {
                Vector3 vecTkForce = (vecTarget - rbTk.position).normalized * flTkThrowForce;
                tkMoveData tkMvData = new tkMoveData(rbTk, vecTkForce);

                if (Vector3.Dot(tkMvData.rbTk.velocity, (vecTarget - rbTk.position).normalized) < 0)
                    tkMvData.bDampVel = true;

                lstTkMove.Add(tkMvData);
            }
            return;
        }

        if (bTkGrabButton && !bTkThrowButton)
        {
            vecTarget = cam.transform.TransformPoint(vecTkHoldLocal);
            foreach (var rbTk in lstTkRbs)
            {
                Vector3 vecTkForce = (vecTarget - rbTk.position).normalized * flTkPullForce;
                tkMoveData tkMvData = new tkMoveData(rbTk, vecTkForce);
                
                if (Vector3.Dot(tkMvData.rbTk.velocity, (vecTarget - rbTk.position).normalized) < 0)
                    tkMvData.bDampVel = true;
                
                lstTkMove.Add(tkMvData);
            }
            return;
        }

        // if we got this far, we're either holding pull and push or nothing at all ...

        if (flTkCurDist == 0f)
        {// only get current tk distance once per hold session
            Vector3 vecTkAvgPos = Vector3.zero;
            foreach (var rbTk in lstTkRbs)
            {
                vecTkAvgPos += rbTk.transform.position;
            }
            vecTkAvgPos /= lstTkRbs.Count;
            flTkCurDist = (vecTkAvgPos - cam.transform.position).magnitude;
        }

        // set target vector to our eyepos + normalized forward vector * cur tk distance
        vecTarget = cam.transform.position + cam.transform.forward * flTkCurDist;

        foreach (var rbTk in lstTkRbs)
        {
            Vector3 vecTkForce = (vecTarget - rbTk.position).normalized * ((flTkPullForce + flTkThrowForce) / 2f);
            tkMoveData tkMvData = new tkMoveData(rbTk, vecTkForce);

            if (Vector3.Dot(tkMvData.rbTk.velocity, (vecTarget - rbTk.position).normalized) < 0)
                tkMvData.bDampVel = true;
            
            lstTkMove.Add(tkMvData);
        }

    }

    void ApplyTkMove()
    {
        if (lstTkMove.Count == 0)
            return;

        if (bTkFreezeButton)
        {
            foreach (var tkRb in lstTkRbs)
                tkRb.isKinematic = true;
            lstTkRbs.Clear();
            lstTkMove.Clear();
            return;
        }

        foreach (var tkMv in lstTkMove)
        {

            if (Vector3.Dot(tkMv.rbTk.velocity.normalized, tkMv.vecTkForce.normalized) < 1)
            {
                float flProjVec = Vector3.Dot(tkMv.rbTk.velocity, tkMv.vecTkForce.normalized);
                Vector3 vecWanted = flProjVec * tkMv.vecTkForce.normalized;
                Vector3 vecUnwanted = tkMv.rbTk.velocity - vecWanted;
                tkMv.rbTk.velocity -= vecUnwanted * (2f * Time.fixedDeltaTime);
            }

            if (tkMv.bResetVel)
                tkMv.rbTk.velocity = Vector3.zero;

            if (tkMv.bDampVel)
                tkMv.rbTk.velocity -= 0.027f * tkMv.rbTk.velocity;

            tkMv.rbTk.AddForce((tkMv.rbTk.mass * tkMv.vecTkForce) - (tkMv.rbTk.mass * Physics.gravity));
        }

        lstTkMove.Clear();
    }
}