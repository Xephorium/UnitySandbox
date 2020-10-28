// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/Input/InputMaster.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @InputMaster : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputMaster()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputMaster"",
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
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""15d41e17-746e-422f-8c47-538156fa08b9"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox 360 Control Scheme"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0a6b4b24-8d33-4489-a880-c791e796076c"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox 360 Control Scheme"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""d56377f9-6d38-44ca-b81f-c7d70a949b0a"",
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
                    ""id"": ""09eff8b1-ec28-47a7-a4df-fa406c623ead"",
                    ""path"": ""<XInputController>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox 360 Control Scheme"",
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
                    ""groups"": ""Xbox 360 Control Scheme"",
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
                    ""groups"": ""Xbox 360 Control Scheme"",
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
                    ""groups"": ""Xbox 360 Control Scheme"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Xbox 360 Control Scheme"",
            ""bindingGroup"": ""Xbox 360 Control Scheme"",
            ""devices"": [
                {
                    ""devicePath"": ""<XInputController>"",
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
    public struct FirstPersonCharacterActions
    {
        private @InputMaster m_Wrapper;
        public FirstPersonCharacterActions(@InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @Jump => m_Wrapper.m_FirstPersonCharacter_Jump;
        public InputAction @Move => m_Wrapper.m_FirstPersonCharacter_Move;
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
            }
        }
    }
    public FirstPersonCharacterActions @FirstPersonCharacter => new FirstPersonCharacterActions(this);
    private int m_Xbox360ControlSchemeSchemeIndex = -1;
    public InputControlScheme Xbox360ControlSchemeScheme
    {
        get
        {
            if (m_Xbox360ControlSchemeSchemeIndex == -1) m_Xbox360ControlSchemeSchemeIndex = asset.FindControlSchemeIndex("Xbox 360 Control Scheme");
            return asset.controlSchemes[m_Xbox360ControlSchemeSchemeIndex];
        }
    }
    public interface IFirstPersonCharacterActions
    {
        void OnJump(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
    }
}
