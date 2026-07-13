public class CS_CustomInputActionManager
{
    private static CS_CustomInputActionManager myInstance;
    private CustomInputAction customInputAction;

    public static CS_CustomInputActionManager instance
    {
        get
        {
            if (myInstance == null)
            {
                myInstance = new CS_CustomInputActionManager();
            }
            return myInstance;
        }
    }

    private CS_CustomInputActionManager() 
    {
        customInputAction = new CustomInputAction();
        customInputAction.Enable();
    }

    ~CS_CustomInputActionManager()
    {
        customInputAction.Disable();
    }
}
