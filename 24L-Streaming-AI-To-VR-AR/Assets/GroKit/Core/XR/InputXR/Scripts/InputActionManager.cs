using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Input action assets to affect when inputs are enabled or disabled.")]
    List<InputActionAsset> m_ActionAssets;
    /// <summary>
    /// Input action assets to affect when inputs are enabled or disabled.
    /// </summary>
    public List<InputActionAsset> actionAssets
    {
        get => m_ActionAssets;
        set => m_ActionAssets = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void OnEnable()
    {
        EnableInput();
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void OnDisable()
    {
        DisableInput();
    }

    /// <summary>
    /// Enable all actions referenced by this component.
    /// </summary>
    /// <remarks>
    /// Unity will automatically call this function when this <see cref="InputActionManager"/> component is enabled.
    /// However, this method can be called to enable input manually, such as after disabling it with <see cref="DisableInput"/>.
    /// <br />
    /// Enabling inputs only enables the action maps contained within the referenced
    /// action map assets (see <see cref="actionAssets"/>).
    /// </remarks>
    /// <seealso cref="DisableInput"/>
    public void EnableInput()
    {
        if (m_ActionAssets == null)
            return;

        foreach (var actionAsset in m_ActionAssets)
        {
            if (actionAsset != null)
            {
                actionAsset.Enable();
            }
        }
    }

    /// <summary>
    /// Disable all actions referenced by this component.
    /// </summary>
    /// <remarks>
    /// This function will automatically be called when this <see cref="InputActionManager"/> component is disabled.
    /// However, this method can be called to disable input manually, such as after enabling it with <see cref="EnableInput"/>.
    /// <br />
    /// Disabling inputs only disables the action maps contained within the referenced
    /// action map assets (see <see cref="actionAssets"/>).
    /// </remarks>
    /// <seealso cref="EnableInput"/>
    public void DisableInput()
    {
        if (m_ActionAssets == null)
            return;

        foreach (var actionAsset in m_ActionAssets)
        {
            if (actionAsset != null)
            {
                actionAsset.Disable();
            }
        }
    }
}
