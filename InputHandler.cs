using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using inputConstants;

public class InputHandler : MonoBehaviour
{

    [Tooltip("Sensitivity multiplier for moving the camera around")]
    public float LookSensitivity = 2f;

    bool SpaceInputWasHeld;
    bool ShiftInputWasHeld;
    bool PrimaryClickWasHeld;
    bool SecondaryClickWasHeld;

    bool InvertYAxis = true;
    bool InvertXAxis = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        SpaceInputWasHeld = GetSpaceButton();
        ShiftInputWasHeld = GetShiftButton();
        PrimaryClickWasHeld = GetPrimaryMouseClick();
        SecondaryClickWasHeld = GetSecondaryMouseClick();
    }

    public bool CanProcessInput()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }

    public Vector3 GetMoveInput()
    {
        if (CanProcessInput())
        {
            Vector3 move = new Vector3(Input.GetAxisRaw(InputConstants.RightAxis), 0f, Input.GetAxisRaw(InputConstants.ForwardAxis));

            // constrain move input to a maximum magnitude of 1, otherwise diagonal movement might exceed the max move speed defined
            move = Vector3.ClampMagnitude(move, 1);

            return move;
        }

        return Vector3.zero;
    }

    public float GetRightAxis()
    {
        if (CanProcessInput()) 
        {
            float move = Input.GetAxisRaw(InputConstants.RightAxis);

            return move;
        }

        return 0.0f;
    }

    public float GetForwardAxis()
    {
        if (CanProcessInput())
        {
            float move = Input.GetAxisRaw(InputConstants.ForwardAxis);

            return move;
        }

        return 0.0f;
    }

    public float GetRollAxis()
    {
        if (CanProcessInput())
        {
            float move = Input.GetAxisRaw(InputConstants.RollAxis);

            return move;
        }

        return 0.0f;
    }

    public float GetLookInputsHorizontal()
    {   
        float horizontalInput = Input.GetAxis(InputConstants.MouseAxisNameHorizontal);
        return InvertXAxis ?  horizontalInput * -1 : horizontalInput;
    }

    public float GetLookInputsVertical()
    {   
        float verticalInput = Input.GetAxis(InputConstants.MouseAxisNameVertical);
        return InvertYAxis ? verticalInput * -1 : verticalInput;
    }

    public bool GetPrimaryMouseClickDown()
    {  
        return Input.GetButtonDown(InputConstants.ButtonNamePrimaryClick);
    }

    public bool GetPrimaryMouseClick()
    {  
        return Input.GetButton(InputConstants.ButtonNamePrimaryClick);
    }

    public bool GetPrimaryMouseClickReleased()
    {  
        if (CanProcessInput())
        {
            return !GetPrimaryMouseClick() && PrimaryClickWasHeld;
        }

        return false;
    }

    public bool GetSecondaryMouseClickDown()
    {  
        return Input.GetButtonDown(InputConstants.ButtonNameSecondaryClick);
    }

    public bool GetSecondaryMouseClick()
    {  
        return Input.GetButton(InputConstants.ButtonNameSecondaryClick);
    }

    public bool GetSecondaryMouseClickReleased()
    {  
        if (CanProcessInput())
        {
            return !GetSecondaryMouseClick() && SecondaryClickWasHeld;
        }

        return false;
    }

    public bool GetSpaceButtonDown()
    {
        if (CanProcessInput())
        {
            return Input.GetButtonDown(InputConstants.ButtonNameSpace);
        }

        return false;
    }

    public bool GetSpaceButton()
    {
        if (CanProcessInput())
        {
            return Input.GetButton(InputConstants.ButtonNameSpace);
        }

        return false;
    }

    public bool GetSpaceButtonReleased()
    {
        if (CanProcessInput())
        {
            return !GetSpaceButton() && SpaceInputWasHeld;
        }

        return false;
    }

    public bool GetShiftButtonDown()
    {
        if (CanProcessInput())
        {
            return Input.GetButtonDown(InputConstants.ButtonNameShift);
        }

        return false;
    }

    public bool GetShiftButton()
    {
        if (CanProcessInput())
        {
            return Input.GetButton(InputConstants.ButtonNameShift);
        }

        return false;
    }

    public bool GetShiftButtonReleased()
    {
        if (CanProcessInput())
        {
            return !GetShiftButton() && ShiftInputWasHeld;
        }

        return false;
    }
}
