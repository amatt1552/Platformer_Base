using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [SerializeField]
    private EnvironmentSettings[] environments;
    public EnvironmentSettings PreviousEnvironment { get; private set; }
    public EnvironmentSettings CurrentEnvironment { get; private set; }
    private readonly Dictionary<LayerMask, EnvironmentSettings> _allEnvironments = new Dictionary<LayerMask, EnvironmentSettings>();

    public delegate void EnvironmentChanged(EnvironmentSettings currentEnv, EnvironmentSettings previousEnv);
    public event EnvironmentChanged OnEnvironmentChanged;

    private void Awake()
    {
        PreviousEnvironment = new();
        CurrentEnvironment = new();
        UpdateEnvironmentSettings();
    }

    public void UpdateEnvironmentSettings() 
    {
        _allEnvironments.Clear();
        foreach (EnvironmentSettings env in environments)
        {
            _allEnvironments.Add(env.layer, env);
        }
    }

    /// <summary>
    /// Attempts to Update Current Environment based on provided tag.
    /// If tag does not exist Current Environment wont change.
    /// </summary>
    /// <param name="layer"></param>
    public void UpdateEnvironment(LayerMask layer)
    {
        EnvironmentSettings tempSettings;
        if (_allEnvironments.TryGetValue(layer, out tempSettings)) 
        {
            if(!tempSettings.layer.Equals(CurrentEnvironment.layer)) 
            {
                PreviousEnvironment = CurrentEnvironment;
                Debug.Log($"Environment switched.");
                CurrentEnvironment = tempSettings;
                OnEnvironmentChanged.Invoke(CurrentEnvironment, PreviousEnvironment);
            }
        }
    }

}

[Serializable]
public class EnvironmentSettings
{
    [Header("Environment Settings")]
    [Tooltip("Setting multiple layers is currently unsupported.")]
    public LayerMask layer;
    public float speedModifier;
    public float accelerationModifier;
    public float decelerationModifier;
    public float drag;
    [Header("Animation Settings")]
    public string animationLayerName;
    public float enabledWeight;
    public float disabledWeight;

    public EnvironmentSettings()
    {
        layer.value = 1;
        speedModifier = 1.0f;
        accelerationModifier = 1.0f;
        decelerationModifier = 1.0f;
        drag = 0f;
        animationLayerName = "Base Layer";
        enabledWeight = 1.0f;
        disabledWeight = 0f;
    }

}
