using UnityEngine;
using System.Collections;

public class hypercubeFpsControl : MonoBehaviour {

    public bool invertMouse = false;
    public float moveSpeed = 2f;
    public float scaleSpeed = 1f;
    public float rollSpeed = 20f;

    public float keyboardLookSpeed = 30f;

    public float sensitivityX = 30f;
    public float sensitivityY = 30f;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationX = 0f;
    float rotationY = 0f;
    float rotationZ = 0f;
    Quaternion originalRotation;

    Quaternion originRotation; //these are not used for navigation, and only for resetting the values
    Vector3 originPosition; 
    Vector3 originScale;

    float scaleMod = 1f;
    Vector3 baseScale = new Vector3(1f, 1f, 1f);

    public Transform moveNode;

    public KeyCode pauseKey;
    public bool paused = false;

   // Vector3 originalPosition;
  //  Vector3 originalScale;

    public void pauseInput()
    {
        paused = true;
    }
    public void resumeInput()
    {
        paused = false;
    }

    void Start()
    {
        if (!moveNode)
            moveNode = transform;

        reset();

        originRotation = moveNode.rotation;
        originPosition = moveNode.position;
        originScale = moveNode.localScale;
    }

    void Update()
    {
        if (paused)
            return;

        if (pauseKey != KeyCode.None && Input.GetKey(pauseKey))
            return;

        //mouse look
        float xLook = 0;
        float yLook = 0;

        if (Input.GetKey(KeyCode.Mouse2) || Input.GetKey(KeyCode.Mouse1))
        {
            xLook = Input.GetAxis("Mouse X");
            yLook = Input.GetAxis("Mouse Y");
        }

        if (Input.GetKey(KeyCode.RightArrow))
            xLook = keyboardLookSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.LeftArrow))
            xLook = -keyboardLookSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.UpArrow))
            yLook = keyboardLookSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.DownArrow))
            yLook = -keyboardLookSpeed * Time.deltaTime;


        if (invertMouse)
            yLook = -yLook;

        float roll = 0f;
        if (Input.GetKey(KeyCode.Tab))
            roll += rollSpeed;
        if (Input.GetKey(KeyCode.R))
            roll -= rollSpeed;

        if (xLook != 0 || yLook != 0 || roll != 0)
        {
            rotationX += xLook * sensitivityX * Time.deltaTime;
            rotationY += yLook * sensitivityY * Time.deltaTime;
            rotationX = ClampAngle(rotationX, minimumX, maximumX);
            rotationY = ClampAngle(rotationY, minimumY, maximumY);

            rotationZ += roll * Time.deltaTime;

            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
            Quaternion zQuaternion = Quaternion.AngleAxis(rotationZ, Vector3.forward);
            moveNode.localRotation = originalRotation * xQuaternion * yQuaternion * zQuaternion;
            //moveNode.Rotate(yLook * sensitivityX * Time.deltaTime, xLook * sensitivityX * Time.deltaTime, roll * Time.deltaTime);
        }



        //movement
        if (Input.GetKey(KeyCode.W))
            moveNode.localPosition += moveNode.forward * moveSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.S))
            moveNode.localPosition -= moveNode.forward * moveSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
            moveNode.localPosition -= moveNode.right * moveSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.D))
            moveNode.localPosition += moveNode.right * moveSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftShift))
            moveNode.localPosition += moveNode.up * moveSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.LeftControl))
            moveNode.localPosition -= moveNode.up * moveSpeed * Time.deltaTime;


        //scaling
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)) //reset it on keydowns for scale so that our modification will take into account any changes to scale applied in the editor
        {
            baseScale = moveNode.localScale;
            scaleMod = 1f;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            scaleMod += scaleSpeed * Time.deltaTime;
            moveNode.localScale = baseScale * scaleMod;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            scaleMod -= scaleSpeed * Time.deltaTime;
            moveNode.localScale = baseScale * scaleMod;
        }


        //other
        if (Input.GetKeyDown(KeyCode.I))
            invertMouse = !invertMouse;

    }

    public void reset()
    {
        originalRotation = moveNode.localRotation;
        rotationX = 0f;
        rotationY = 0f;
        rotationZ = 0f;
    }

    public void resetToOriginalLocation()
    {
        moveNode.rotation = originRotation;
        moveNode.position = originPosition;
        moveNode.localScale = originScale;
        reset();
    }

    public static float ClampAngle(float angle, float min, float max)
 {
     while (angle < -360F)
         angle += 360F;
     while (angle > 360F)
         angle -= 360F;
     return Mathf.Clamp (angle, min, max);
 }
}
