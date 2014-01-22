using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum IInputPlayerControllerType
{
    LATERAL_PLAYER_CONTROLLER = 0,
    THIRD_PERSON_PLAYER_CONTROLLER
}

public class InputControllerManager
{
    IInputPlayerController _activeInputPlayerController = null;

    Dictionary<IInputPlayerControllerType, IInputPlayerController>_inputPlayerControllers = null;

    public static void create(out InputControllerManager inputControllerManager)
    {
        inputControllerManager = new InputControllerManager();
        inputControllerManager.init();
    }

    InputControllerManager()
    {

    }

    void init()
    {
        _inputPlayerControllers = new Dictionary<IInputPlayerControllerType, IInputPlayerController>();

        InputLateralPlayerController iLateralPC = null;
        InputLateralPlayerController.create(out iLateralPC);
        _inputPlayerControllers.Add(IInputPlayerControllerType.LATERAL_PLAYER_CONTROLLER, iLateralPC);

        InputThirdPersonPlayerController iThirdPersonPC = null;
        InputThirdPersonPlayerController.create(out iThirdPersonPC);
        _inputPlayerControllers.Add(IInputPlayerControllerType.THIRD_PERSON_PLAYER_CONTROLLER, iThirdPersonPC);

        _activeInputPlayerController = _inputPlayerControllers[IInputPlayerControllerType.LATERAL_PLAYER_CONTROLLER];
    }

    public void changeToInputPlayerController(IInputPlayerControllerType type)
    {
        _activeInputPlayerController = _inputPlayerControllers[type];
    }

    public IInputPlayerController getActiveInputPlayerController()
    {
        return _activeInputPlayerController;
    }
}
