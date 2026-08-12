using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.Input;
using Il2CppSystem.Dynamic.Utils;
using MelonLoader;
using Starlight.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using InputEvent = Il2CppMonomiPark.SlimeRancher.Input.InputEvent;


namespace BuildImprovements.Input;

// Utility class for registering InputActions, InputEvents and everything that comes with.
internal static class InputRegistrar
{
    private static InputDirector InputDirector => GameContext.Instance.InputDirector;

    // In the future all our input checking will be moved to be registered here.
    internal static void RegisterPlacementImprovementsInputs() { }

    // Encapsulates everything necessary to set up a callback for Key. Only possible while the InputDirector._mainGame action map is disabled.
    public static InputEvent RegisterInputForKey(Key Key, string ActionName, Action<InputEventData>? Performed = null, Action<InputEventData>? Started = null, Action<InputEventData>? Canceled = null) => 
        RegisterInput(InputControlFromKeyboardKey(Key), ActionName, Performed, Started, Canceled);
    public static InputEvent RegisterInputForMouse(MouseButton Button, string ActionName, Action<InputEventData>? Performed = null, Action<InputEventData>? Started = null, Action<InputEventData>? Canceled = null) =>
        RegisterInput(InputControlFromMouse(Button), ActionName, Performed, Started, Canceled);

    public static InputEvent RegisterInput(InputControl Control, string ActionName, Action<InputEventData>? Performed = null, Action<InputEventData>? Started = null, Action<InputEventData>? Canceled = null)
    {
        InputBinding Binding = MakeInputBinding(ActionName, Control);
        InputAction Action = MakeInputAction(ActionName, Binding);
        InputEvent Event = new(); 
        if(Performed != null)
            SubscribeToInputEvent(Event, Performed, Started, Canceled);

        InputEventBinding EventBinding = MakeEventBinding(Action, Event);

        MelonLogger.Msg("Upon completing input registration the effective path of the InputBinding is " + Binding.effectivePath);

        return Event;
    }

    public static InputBinding MakeInputBinding(string ActionName, InputControl Control, string group = "PC_KeyboardMouse")
    {
        InputBinding Binding = new()
        {
            action = ActionName,
            path = Control.path.Replace("/Keyboard", "<Keyboard>"),
            groups = group
        };

        return Binding;
    }
    public static InputAction MakeInputAction(string Name, InputBinding Binding) => MakeInputAction(Name, new[] {Binding});
    public static InputAction MakeInputAction(string Name, InputBinding[] Bindings)
    {
        InputAction NewAction = InputActionSetupExtensions.AddAction(InputDirector._mainGame, Name, InputActionType.Button);

        foreach (var Binding in Bindings)
            NewAction.AddBinding(Binding);

        return NewAction;
    }
    public static void SubscribeToInputEvent(InputEvent Event, Action<InputEventData> Performed, Action<InputEventData>? Started = null, Action<InputEventData>? Canceled = null)
    {
        if(Canceled != null) Event.Canceled += Canceled;
        if(Started != null) Event.Started += Started;
        Event.Performed += Performed;
    }
    // Make sure your InputAction is set up before you call this.
    public static InputEventBinding MakeEventBinding(InputAction Action, InputEvent Event) => MakeEventBinding(Action, new[] { Event });
    public static InputEventBinding MakeEventBinding(InputAction Action, InputEvent[] Events)
    {
        InputEventBinding Binding = new()
        {
            _input = InputActionReference.Create(Action),
            _inputEvents = Events.ToIl2CppArray()
        };

        List<InputEventBinding> InputBindings = InputDirector.InputActionController._inputBindings.ToNetList();
        InputBindings.Add(Binding);
        InputDirector.InputActionController._inputBindings = InputBindings.ToIl2CppArray();
        Binding.BindInput();

        return Binding;
    }
    // Converts KeyCodes from the old input system (the one used for keybind settings with Starlight) to the new input system. THIS NEEDS SOME MORE WORK, THIS IS BROKEN.
    public static Key KeyCodeToKey(KeyCode keyCode) => (Key)((int)keyCode);
    public static InputControl InputControlFromKeyboardKey(Key InKey) => Keyboard.current[InKey];
    public static InputControl InputControlFromMouse(MouseButton Button)
    {
        switch(Button)
        {
            case MouseButton.Left: return Mouse.current.leftButton;
            case MouseButton.Right: return Mouse.current.rightButton;
            case MouseButton.Middle: return Mouse.current.middleButton;
            case MouseButton.Forward: return Mouse.current.forwardButton;
            case MouseButton.Back: return Mouse.current.backButton;

            // This will never happen. MouseButton has no possible states other than the above.
            default: return Mouse.current.leftButton;
        }
    }
}