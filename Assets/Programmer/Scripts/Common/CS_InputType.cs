using UnityEngine.InputSystem;

public class CS_InputType
{
    public enum InputType
    {
        KeyboardMouse,
        Gamepad
    }

    static public InputType currentInputType;

    public void OnAnyAction(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (context.control.device is Gamepad)
            currentInputType = InputType.Gamepad;
        else if (context.control.device is Keyboard ||
                 context.control.device is Mouse)
            currentInputType = InputType.KeyboardMouse;
    }
}
