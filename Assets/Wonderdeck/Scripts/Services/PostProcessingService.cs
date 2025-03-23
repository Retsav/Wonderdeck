using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingService : IPostProcessingService
{
    private Volume _volume;
    public void RegisterVolume(Volume volume) => _volume = volume;
    public void ResetVolume() => _volume = null;
    public Volume GetVolume()
    {
        if (_volume != null) return _volume;
        Debug.LogError($"Post processing volume is null");
        return null;
    }
}
