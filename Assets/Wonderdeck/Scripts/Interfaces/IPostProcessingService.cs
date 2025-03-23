using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public interface IPostProcessingService
{
    public void RegisterVolume(Volume volume);
    public void ResetVolume();
    public Volume GetVolume();
}
