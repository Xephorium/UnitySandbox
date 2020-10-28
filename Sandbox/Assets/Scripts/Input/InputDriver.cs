// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/Input/InputDriver.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @InputDriver : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputDriver()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputDriver"",
    ""maps"": [
        {
            ""name"": ""FirstPersonCharacter"",
            ""id"": ""377374f0-1594-4000-a1ed-aab64fa6f0ca"",
            ""actions"": [
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""71c22441-91df-4fe3-b70f-934fd08b27f3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""PassThrough"",
                    ""id"": ""d5cde5bc-b576-4c3e-92bc-fcbf6994a3b6"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""LookMouse"",
                    ""type"": ""Value"",
                    ""id"": ""67ee5654-5432-49d5-bb16-da467ad51a72"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""LookStick"",
                    ""type"": ""Value"",
                    ""id"": ""8dc8ff72-d47d-43ed-ab6b-613f176d7878"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Run"",
                    ""type"": ""Button"",
                    ""id"": ""8b89ec50-d32f-4717-9582-e72871d263e5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Zoom"",
                    ""type"": ""Button"",
                    ""id"": ""98c66b5d-318e-4ad2-ba12-4002ed460a74"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Crouch"",
                    ""type"": ""Button"",
                    ""id"": ""a37cac3f-6c98-4784-8dfb-d73f6e659d3e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""15d41e17-746e-422f-8c47-538156fa08b9"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dc46608a-79d0-4d14-b8f0-c7101b038910"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Left Toggle Stick [Gamepad]"",
                    ""id"": ""d56377f9-6d38-44ca-b81f-c7d70a949b0a"",
                    ""path"": ""2DVector(mode=2)"",
                    ""interactions"": """",
                    ""processors"": ""StickDeadzone"",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""09eff8b1-ec28-47a7-a4df-fa406c623ead"",
                    ""path"": ""<XInputController>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""90bfa393-7f80-43c8-9127-467458555726"",
                    ""path"": ""<XInputController>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""7087d5be-4f75-4591-bf10-41f0d170eeeb"",
                    ""path"": ""<XInputController>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""d10636f6-1f60-4db1-84e5-95511f24911d"",
                    ""path"": ""<XInputController>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""WASD [Keyboard]"",
                    ""id"": ""70f20a08-47ad-4fd1-9e4f-2d434173ab5d"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""df5c558a-6d9c-48eb-80f7-c03a31ceb84e"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""70d66c79-15fe-4926-84b3-3ca46dfc3b6c"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""e6978646-0252-42a3-ae69-47d6677c5e53"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""d13e6d2d-1549-4894-90d1-95fc01f77519"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""53828a97-5439-49a7-8541-daac0ca8930b"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""LookMouse"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Right Toggle Stick [Gamepad]"",
                    ""id"": ""0d010015-d90a-4714-8c12-513d4c018de6"",
                    ""path"": ""2DVector(mode=2)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LookStick"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""9c72f838-895b-490a-b330-678c239d5b55"",
                    ""path"": ""<Gamepad>/rightStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""LookStick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""013ee986-15d0-45cf-ac8e-d0a353e83f9a"",
                    ""path"": ""<Gamepad>/rightStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""LookStick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""5e79da79-e091-497f-9da7-e9e035b225e0"",
                    ""path"": ""<Gamepad>/rightStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""LookStick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""ed139fa0-c4e7-4806-b97d-ab40f6cf527e"",
                    ""path"": ""<Gamepad>/rightStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""LookStick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""0d493ed7-8532-43f0-b4f4-0c2e88a655b1"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c4bcf35f-c4f8-4c21-a891-5c0aef22fd82"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e0172819-22e5-4409-af80-933c2c6d6cc9"",
                    ""path"": ""<Gamepad>/rightStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8d1380c7-195a-4f9c-af21-e7e2e09bc35b"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c108586f-dd03-4225-8dda-d6feeb7e68f3"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Game Input Scheme"",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Game Input Scheme"",
            ""bindingGroup"": ""Game Input Scheme"",
            ""devices"": [
                {
                    ""devicePath"": ""<XInputController>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // FirstPersonCharacter
        m_FirstPersonCharacter = asset.FindActionMap("FirstPersonCharacter", throwIfNotFound: true);
        m_FirstPersonCharacter_Jump = m_FirstPersonCharacter.FindAction("Jump", throwIfNotFound: true);
        m_FirstPersonCharacter_Move = m_FirstPersonCharacter.FindAction("Move", throwIfNotFound: true);
        m_FirstPersonCharacter_LookMouse = m_FirstPersonCharacter.FindAction("LookMouse", throwIfNotFound: true);
        m_FirstPersonCharacter_LookStick = m_FirstPersonCharacter.FindAction("LookStick", throwIfNotFound: true);
        m_FirstPersonCharacter_Run = m_FirstPersonCharacter.FindAction("Run", throwIfNotFound: true);
        m_FirstPersonCharacter_Zoom = m_FirstPersonCharacter.FindAction("Zoom", throwIfNotFound: true);
        m_FirstPersonCharacter_Crouch = m_FirstPersonCharacter.FindAction("Crouch", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // FirstPersonCharacter
    private readonly InputActionMap m_FirstPersonCharacter;
    private IFirstPersonCharacterActions m_FirstPersonCharacterActionsCallbackInterface;
    private readonly InputAction m_FirstPersonCharacter_Jump;
    private readonly InputAction m_FirstPersonCharacter_Move;
    private readonly InputAction m_FirstPersonCharacter_LookMouse;
    private readonly InputAction m_FirstPersonCharacter_LookStick;
    private readonly InputAction m_FirstPersonCharacter_Run;
    private readonly InputAction m_FirstPersonCharacter_Zoom;
    private readonly InputAction m_FirstPersonCharacter_Crouch;
    public struct FirstPersonCharacterActions
    {
        private @InputDriver m_Wrapper;
        public FirstPersonCharacterActions(@InputDriver wrapper) { m_Wrapper = wrapper; }
        public InputAction @Jump => m_Wrapper.m_FirstPersonCharacter_Jump;
        public InputAction @Move => m_Wrapper.m_FirstPersonCharacter_Move;
        public InputAction @LookMouse => m_Wrapper.m_FirstPersonCharacter_LookMouse;
        public InputAction @LookStick => m_Wrapper.m_FirstPersonCharacter_LookStick;
        public InputAction @Run => m_Wrapper.m_FirstPersonCharacter_Run;
        public InputAction @Zoom => m_Wrapper.m_FirstPersonCharacter_Zoom;
        public InputAction @Crouch => m_Wrapper.m_FirstPersonCharacter_Crouch;
        public InputActionMap Get() { return m_Wrapper.m_FirstPersonCharacter; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(FirstPersonCharacterActions set) { return set.Get(); }
        public void SetCallbacks(IFirstPersonCharacterActions instance)
        {
            if (m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface != null)
            {
                @Jump.started -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnJump;
                @Move.started -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnMove;
                @LookMouse.started -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnLookMouse;
                @LookMouse.performed -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnLookMouse;
                @LookMouse.canceled -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnLookMouse;
                @LookStick.started -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnLookStick;
                @LookStick.performed -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnLookStick;
                @LookStick.canceled -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnLookStick;
                @Run.started -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnRun;
                @Run.performed -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnRun;
                @Run.canceled -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnRun;
                @Zoom.started -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnZoom;
                @Zoom.performed -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnZoom;
                @Zoom.canceled -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnZoom;
                @Crouch.started -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnCrouch;
                @Crouch.performed -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnCrouch;
                @Crouch.canceled -= m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface.OnCrouch;
            }
            m_Wrapper.m_FirstPersonCharacterActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @LookMouse.started += instance.OnLookMouse;
                @LookMouse.performed += instance.OnLookMouse;
                @LookMouse.canceled += instance.OnLookMouse;
                @LookStick.started += instance.OnLookStick;
                @LookStick.performed += instance.OnLookStick;
                @LookStick.canceled += instance.OnLookStick;
                @Run.started += instance.OnRun;
                @Run.performed += instance.OnRun;
                @Run.canceled += instance.OnRun;
                @Zoom.started += instance.OnZoom;
                @Zoom.performed += instance.OnZoom;
                @Zoom.canceled += instance.OnZoom;
                @Crouch.started += instance.OnCrouch;
                @Crouch.performed += instance.OnCrouch;
                @Crouch.canceled += instance.OnCrouch;
            }
        }
    }
    public FirstPersonCharacterActions @FirstPersonCharacter => new FirstPersonCharacterActions(this);
    private int m_GameInputSchemeSchemeIndex = -1;
    public InputControlScheme GameInputSchemeScheme
    {
        get
        {
            if (m_GameInputSchemeSchemeIndex == -1) m_GameInputSchemeSchemeIndex = asset.FindControlSchemeIndex("Game Input Scheme");
            return asset.controlSchemes[m_GameInputSchemeSchemeIndex];
        }
    }
    public interface IFirstPersonCharacterActions
    {
        void OnJump(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnLookMouse(InputAction.CallbackContext context);
        void OnLookStick(InputAction.CallbackContext context);
        void OnRun(InputAction.CallbackContext context);
        void OnZoom(InputAction.CallbackContext context);
        void OnCrouch(InputAction.CallbackContext context);
    }
}
