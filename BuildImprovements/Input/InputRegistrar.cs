using BuildImprovements.Preferences;
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

    internal struct InputEventStore
    {
        internal InputEventBinding GadgetLockBinding;
        internal InputEventBinding SmoothNudgeBinding;
        internal InputEventBinding GadgetEyedropBinding;
        internal InputEventBinding NudgeUpDownBinding; 
        internal InputEventBinding NudgeForwardBackBinding, NudgeLeftRightBinding; // WS, AD
        internal InputEventBinding NudgeHorizontalBinding; // WASD

        internal InputEvent GadgetLock => GadgetLockBinding.InputEvents[0];
        internal InputEvent SmoothNudge => SmoothNudgeBinding.InputEvents[0];
        internal InputEvent GadgetEyedrop => GadgetEyedropBinding.InputEvents[0];
        internal InputEvent NudgeUpDown => NudgeUpDownBinding.InputEvents[0];
        internal InputEvent NudgeForwardBack => NudgeForwardBackBinding.InputEvents[0];
        internal InputEvent NudgeLeftRight => NudgeLeftRightBinding.InputEvents[0];
        internal InputEvent NudgeHorizontal => NudgeHorizontalBinding.InputEvents[0];
    };

    internal static InputEventStore EventStore = new();

    // In the future all our input checking will be moved to be registered here.
    internal static void RegisterPlacementImprovementsInputs()
    {
        EventStore.GadgetLockBinding = RegisterInputForKey(KeyCodeToKey(PreferenceDirector.PlacementLockBind), "Lock Gadget", PlacementInputDirector.GadgetLock_Performed);
        EventStore.GadgetEyedropBinding = PreferenceDirector.bGadgetEyedropperMiddleClick ? RegisterInputForMouse(MouseButton.Middle, "Gadget Eyedropper", PlacementInputDirector.GadgetEyedropper_Performed) :
            RegisterInputForKey(KeyCodeToKey(PreferenceDirector.GadgetEyedropperBind), "Gadget Eyedropper", PlacementInputDirector.GadgetEyedropper_Performed);

        // Unused dummy actions. These are enabled like normal but are only used for display purposes, actual logic is still handled by InputEUtil.
        EventStore.SmoothNudgeBinding = RegisterInputForKey(KeyCodeToKey(PreferenceDirector.SmoothNudgeBind), "Smooth Nudge");
        EventStore.NudgeUpDownBinding = RegisterInputForKeys(new[] { KeyCodeToKey(PreferenceDirector.NudgeUpBind), KeyCodeToKey(PreferenceDirector.NudgeDownBind) }, "Nudge Up/Down");
        EventStore.NudgeForwardBackBinding = RegisterInputForKeys(new[] { KeyCodeToKey(PreferenceDirector.NudgeForwardBind), KeyCodeToKey(PreferenceDirector.NudgeBackwardBind) }, "Nudge Forward/Back");
        EventStore.NudgeLeftRightBinding = RegisterInputForKeys(new[] { KeyCodeToKey(PreferenceDirector.NudgeLeftBind), KeyCodeToKey(PreferenceDirector.NudgeRightBind) } , "Nudge Left/Right");
        EventStore.NudgeHorizontalBinding = RegisterInputForKeys(
            new[] { KeyCodeToKey(PreferenceDirector.NudgeForwardBind), KeyCodeToKey(PreferenceDirector.NudgeLeftBind), KeyCodeToKey(PreferenceDirector.NudgeBackwardBind), KeyCodeToKey(PreferenceDirector.NudgeRightBind) }, 
            "Nudge Horizontal");
    }

    // Goes through all event bindings in the InputEventStore and modifies their controls to match the preference director.
    internal static void ResetPlacementImprovementsInputs()
    {
        ModifyControlsForEventBinding(EventStore.GadgetLockBinding, InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.PlacementLockBind)));
        ModifyControlsForEventBinding(EventStore.GadgetEyedropBinding, PreferenceDirector.bGadgetEyedropperMiddleClick ? 
            InputControlFromMouse(MouseButton.Middle) : InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.GadgetEyedropperBind)));

        // Unused dummy actions for UI
        ModifyControlsForEventBinding(EventStore.SmoothNudgeBinding, InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.SmoothNudgeBind)));
        ModifyControlsForEventBinding(EventStore.NudgeUpDownBinding, new[] {
            InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.NudgeUpBind)),
            InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.NudgeDownBind)),
        });
        ModifyControlsForEventBinding(EventStore.NudgeForwardBackBinding, new[] {
            InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.NudgeForwardBind)),
            InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.NudgeBackwardBind)),
        });
        ModifyControlsForEventBinding(EventStore.NudgeLeftRightBinding, new[] { 
            InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.NudgeLeftBind)),
            InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.NudgeRightBind)),
        });
        ModifyControlsForEventBinding(EventStore.NudgeHorizontalBinding, new[] {
            InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.NudgeForwardBind)),
            InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.NudgeLeftBind)),
            InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.NudgeBackwardBind)),
            InputControlFromKeyboardKey(KeyCodeToKey(PreferenceDirector.NudgeRightBind)),
        });
    }

    public static void ModifyControlsForEventBinding(InputEventBinding EventBinding, InputControl NewControl) => ModifyControlsForEventBinding(EventBinding, new[] { NewControl });
    public static void ModifyControlsForEventBinding(InputEventBinding EventBinding, InputControl[] NewControls)
    {
        EventBinding.UnbindInput();

        string PrevName = EventBinding.ActionInstance.name;
        EventBinding.ActionInstance.RemoveAction();

        InputBinding[] Bindings = new InputBinding[NewControls.Length];
        for(int i = 0; i <  NewControls.Length; i++)
            Bindings[i] = MakeInputBinding(PrevName, NewControls[i]);

        EventBinding.ActionInstance = MakeInputAction(PrevName, Bindings);
        EventBinding._input = InputActionReference.Create(EventBinding.ActionInstance);


        EventBinding.BindInput();
    }

    // Encapsulates everything necessary to set up a callback for Key. Only possible while the InputDirector._mainGame.Map.Asset is disabled.
    public static InputEventBinding RegisterInputForKey(Key Key, string ActionName,  Action<InputEventData>? Performed = null, Action<InputEventData>? Started = null, Action<InputEventData>? Canceled = null, InputActionType ActionType = InputActionType.Button) => 
        RegisterInputForControl(InputControlFromKeyboardKey(Key), ActionName, Performed, Started, Canceled, ActionType);
    public static InputEventBinding RegisterInputForMouse(MouseButton Button, string ActionName, Action<InputEventData>? Performed = null, Action<InputEventData>? Started = null, Action<InputEventData>? Canceled = null, InputActionType ActionType = InputActionType.Button) =>
        RegisterInputForControl(InputControlFromMouse(Button), ActionName, Performed, Started, Canceled, ActionType);

    public static InputEventBinding RegisterInputForKeys(Key[] Keys, string ActionName, Action<InputEventData>? Performed = null, Action<InputEventData>? Started = null, Action<InputEventData>? Canceled = null, InputActionType ActionType = InputActionType.Button)
    {
        InputControl[] Controls = new InputControl[Keys.Length];

        for (int i = 0; i < Keys.Length; i++)
            Controls[i] = InputControlFromKeyboardKey(Keys[i]);

        return RegisterInputMultiControl(Controls, ActionName, Performed, Started, Canceled, ActionType);
    }

    public static InputEventBinding RegisterInputForControl(InputControl Control, string ActionName, Action<InputEventData>? Performed = null, Action<InputEventData>? Started = null, Action<InputEventData>? Canceled = null, InputActionType ActionType = InputActionType.Button)
        => RegisterInputMultiControl(new[] { Control }, ActionName, Performed, Started, Canceled, ActionType);

    public static InputEventBinding RegisterInputMultiControl(InputControl[] Controls, string ActionName, Action<InputEventData>? Performed = null, Action<InputEventData>? Started = null, Action<InputEventData>? Canceled = null, InputActionType ActionType = InputActionType.Button)
    {
        InputBinding[] Bindings = new InputBinding[Controls.Length];

        for (int i = 0; i < Controls.Length; i++)
            Bindings[i] = MakeInputBinding(ActionName, Controls[i]);

        InputAction Action = MakeInputAction(ActionName, Bindings, ActionType);
        InputEvent Event = new();
        if (Performed != null)
            SubscribeToInputEvent(Event, Performed, Started, Canceled);

        InputEventBinding EventBinding = MakeEventBinding(Action, Event);

        return EventBinding;
    }

    public static InputBinding MakeInputBinding(string ActionName, InputControl Control, string group = "PC_KeyboardMouse")
    {
        InputBinding Binding = new()
        {
            action = ActionName,
            // This might not be necessary but I'll leave it in for now.
            path = Control.path.Replace("/Keyboard", "<Keyboard>").Replace("/Mouse", "<Mouse>"),
            groups = group
        };

        MelonLogger.Msg("InputBinding effective path: " + Binding.effectivePath);

        return Binding;
    }
    public static InputAction MakeInputAction(string Name, InputBinding Binding, InputActionType ActionType = InputActionType.Button) => MakeInputAction(Name, new[] {Binding}, ActionType);
    public static InputAction MakeInputAction(string Name, InputBinding[] Bindings, InputActionType ActionType = InputActionType.Button)
    {
        InputAction NewAction = InputActionSetupExtensions.AddAction(InputDirector._mainGame, Name, ActionType);

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

    // Converts KeyCodes from the old input system (the one used for keybind settings with Starlight) to the new input system.
    // Bless you: https://discussions.unity.com/t/convert-from-old-keycode-to-the-new-key-enum/801672/2
    public static Key KeyCodeToKey(KeyCode keyCode, Key unknownKey = Key.None, Key mouseKey = Key.None, Key joystickKey = Key.None)
    {
        switch (keyCode)
        {
            case KeyCode.None: return Key.None;
            case KeyCode.Backspace: return Key.Backspace;
            case KeyCode.Delete: return Key.Delete;
            case KeyCode.Tab: return Key.Tab;
            case KeyCode.Clear: return unknownKey; // Conversion unknown.
            case KeyCode.Return: return Key.Enter;
            case KeyCode.Pause: return Key.Pause;
            case KeyCode.Escape: return Key.Escape;
            case KeyCode.Space: return Key.Space;
            case KeyCode.Keypad0: return Key.Numpad0;
            case KeyCode.Keypad1: return Key.Numpad1;
            case KeyCode.Keypad2: return Key.Numpad2;
            case KeyCode.Keypad3: return Key.Numpad3;
            case KeyCode.Keypad4: return Key.Numpad4;
            case KeyCode.Keypad5: return Key.Numpad5;
            case KeyCode.Keypad6: return Key.Numpad6;
            case KeyCode.Keypad7: return Key.Numpad7;
            case KeyCode.Keypad8: return Key.Numpad8;
            case KeyCode.Keypad9: return Key.Numpad9;
            case KeyCode.KeypadPeriod: return Key.NumpadPeriod;
            case KeyCode.KeypadDivide: return Key.NumpadDivide;
            case KeyCode.KeypadMultiply: return Key.NumpadMultiply;
            case KeyCode.KeypadMinus: return Key.NumpadMinus;
            case KeyCode.KeypadPlus: return Key.NumpadPlus;
            case KeyCode.KeypadEnter: return Key.NumpadEnter;
            case KeyCode.KeypadEquals: return Key.NumpadEquals;
            case KeyCode.UpArrow: return Key.UpArrow;
            case KeyCode.DownArrow: return Key.DownArrow;
            case KeyCode.RightArrow: return Key.RightArrow;
            case KeyCode.LeftArrow: return Key.LeftArrow;
            case KeyCode.Insert: return Key.Insert;
            case KeyCode.Home: return Key.Home;
            case KeyCode.End: return Key.End;
            case KeyCode.PageUp: return Key.PageUp;
            case KeyCode.PageDown: return Key.PageDown;
            case KeyCode.F1: return Key.F1;
            case KeyCode.F2: return Key.F2;
            case KeyCode.F3: return Key.F3;
            case KeyCode.F4: return Key.F4;
            case KeyCode.F5: return Key.F5;
            case KeyCode.F6: return Key.F6;
            case KeyCode.F7: return Key.F7;
            case KeyCode.F8: return Key.F8;
            case KeyCode.F9: return Key.F9;
            case KeyCode.F10: return Key.F10;
            case KeyCode.F11: return Key.F11;
            case KeyCode.F12: return Key.F12;
            case KeyCode.F13: return unknownKey; // Conversion unknown.
            case KeyCode.F14: return unknownKey; // Conversion unknown.
            case KeyCode.F15: return unknownKey; // Conversion unknown.
            case KeyCode.Alpha0: return Key.Digit0;
            case KeyCode.Alpha1: return Key.Digit1;
            case KeyCode.Alpha2: return Key.Digit2;
            case KeyCode.Alpha3: return Key.Digit3;
            case KeyCode.Alpha4: return Key.Digit4;
            case KeyCode.Alpha5: return Key.Digit5;
            case KeyCode.Alpha6: return Key.Digit6;
            case KeyCode.Alpha7: return Key.Digit7;
            case KeyCode.Alpha8: return Key.Digit8;
            case KeyCode.Alpha9: return Key.Digit9;
            case KeyCode.Exclaim: return unknownKey; // Conversion unknown.
            case KeyCode.DoubleQuote: return unknownKey; // Conversion unknown.
            case KeyCode.Hash: return unknownKey; // Conversion unknown.
            case KeyCode.Dollar: return unknownKey; // Conversion unknown.
            case KeyCode.Percent: return unknownKey; // Conversion unknown.
            case KeyCode.Ampersand: return unknownKey; // Conversion unknown.
            case KeyCode.Quote: return Key.Quote;
            case KeyCode.LeftParen: return unknownKey; // Conversion unknown.
            case KeyCode.RightParen: return unknownKey; // Conversion unknown.
            case KeyCode.Asterisk: return unknownKey; // Conversion unknown.
            case KeyCode.Plus: return Key.None; // TODO
            case KeyCode.Comma: return Key.Comma;
            case KeyCode.Minus: return Key.Minus;
            case KeyCode.Period: return Key.Period;
            case KeyCode.Slash: return Key.Slash;
            case KeyCode.Colon: return unknownKey; // Conversion unknown.
            case KeyCode.Semicolon: return Key.Semicolon;
            case KeyCode.Less: return Key.None;
            case KeyCode.Equals: return Key.Equals;
            case KeyCode.Greater: return unknownKey; // Conversion unknown.
            case KeyCode.Question: return unknownKey; // Conversion unknown.
            case KeyCode.At: return unknownKey; // Conversion unknown.
            case KeyCode.LeftBracket: return Key.LeftBracket;
            case KeyCode.Backslash: return Key.Backslash;
            case KeyCode.RightBracket: return Key.RightBracket;
            case KeyCode.Caret: return Key.None; // TODO
            case KeyCode.Underscore: return unknownKey; // Conversion unknown.
            case KeyCode.BackQuote: return Key.Backquote;
            case KeyCode.A: return Key.A;
            case KeyCode.B: return Key.B;
            case KeyCode.C: return Key.C;
            case KeyCode.D: return Key.D;
            case KeyCode.E: return Key.E;
            case KeyCode.F: return Key.F;
            case KeyCode.G: return Key.G;
            case KeyCode.H: return Key.H;
            case KeyCode.I: return Key.I;
            case KeyCode.J: return Key.J;
            case KeyCode.K: return Key.K;
            case KeyCode.L: return Key.L;
            case KeyCode.M: return Key.M;
            case KeyCode.N: return Key.N;
            case KeyCode.O: return Key.O;
            case KeyCode.P: return Key.P;
            case KeyCode.Q: return Key.Q;
            case KeyCode.R: return Key.R;
            case KeyCode.S: return Key.S;
            case KeyCode.T: return Key.T;
            case KeyCode.U: return Key.U;
            case KeyCode.V: return Key.V;
            case KeyCode.W: return Key.W;
            case KeyCode.X: return Key.X;
            case KeyCode.Y: return Key.Y;
            case KeyCode.Z: return Key.Z;
            case KeyCode.LeftCurlyBracket: return unknownKey; // Conversion unknown.
            case KeyCode.Pipe: return unknownKey; // Conversion unknown.
            case KeyCode.RightCurlyBracket: return unknownKey; // Conversion unknown.
            case KeyCode.Tilde: return unknownKey; // Conversion unknown.
            case KeyCode.Numlock: return Key.NumLock;
            case KeyCode.CapsLock: return Key.CapsLock;
            case KeyCode.ScrollLock: return Key.ScrollLock;
            case KeyCode.RightShift: return Key.RightShift;
            case KeyCode.LeftShift: return Key.LeftShift;
            case KeyCode.RightControl: return Key.RightCtrl;
            case KeyCode.LeftControl: return Key.LeftCtrl;
            case KeyCode.RightAlt: return Key.RightAlt;
            case KeyCode.LeftAlt: return Key.LeftAlt;
            case KeyCode.LeftCommand: return Key.LeftCommand;
            // case KeyCode.LeftApple: (same as LeftCommand)
            case KeyCode.LeftWindows: return Key.LeftWindows;
            case KeyCode.RightCommand: return Key.RightCommand;
            // case KeyCode.RightApple: (same as RightCommand)
            case KeyCode.RightWindows: return Key.RightWindows;
            case KeyCode.AltGr: return Key.AltGr;
            case KeyCode.Help: return unknownKey; // Conversion unknown.
            case KeyCode.Print: return Key.PrintScreen;
            case KeyCode.SysReq: return unknownKey; // Conversion unknown.
            case KeyCode.Break: return unknownKey; // Conversion unknown.
            case KeyCode.Menu: return Key.ContextMenu;
            case KeyCode.Mouse0:
            case KeyCode.Mouse1:
            case KeyCode.Mouse2:
            case KeyCode.Mouse3:
            case KeyCode.Mouse4:
            case KeyCode.Mouse5:
            case KeyCode.Mouse6:
                return mouseKey; // Not supported anymore.

            // All other keys are joystick keys which do not
            // exist anymore in the new input system.
            default:
                return joystickKey; // Not supported anymore.
        }
    }
}