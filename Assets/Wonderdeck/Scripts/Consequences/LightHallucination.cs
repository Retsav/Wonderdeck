using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using Zenject;

public class LightHallucination : BaseConsequence
{
    private INetworkingService _networkingService;
    private IPostProcessingService _processingService;
    
    private Vignette _vignette;
    private FilmGrain _filmGrain;
    private DepthOfField _depthOfField;
    private float _defaultVignetteIntensity;
    private float _defaultFilmGrainIntensity;
    private float _defaultFocalLength;
    
    private CinemachineVirtualCamera _virtualCamera;
    private CinemachineBasicMultiChannelPerlin _noise;
    
    private bool _consequenceActive = false;
    
    private Sequence _toggleSequence;
    private Sequence _failsafeTween;

    [Inject]
    private void ResolveDependencies(INetworkingService networkingService, IPostProcessingService postProcessingService)
    {
        _networkingService = networkingService;
        _processingService = postProcessingService;
    }
    
    public override void ApplyConsequence()
    {
        Debug.Log("Consequence Applied.");
        var volume = _processingService.GetVolume();
        if (volume == null)
            return;
        if (volume.profile.TryGet<Vignette>(out _vignette))
            _defaultVignetteIntensity = _vignette.intensity.value;
        if (volume.profile.TryGet<FilmGrain>(out _filmGrain))
            _defaultFilmGrainIntensity = _filmGrain.intensity.value;
        if (volume.profile.TryGet<DepthOfField>(out _depthOfField))
            _defaultFocalLength = _depthOfField.focalLength.value;
        _consequenceActive = true;
        ApplyEffectsTween();
        GameObject playerGO = _networkingService.GetMyPlayer();
        if (playerGO != null)
        {
            _virtualCamera = playerGO.GetComponentInChildren<CinemachineVirtualCamera>();
            if (_virtualCamera != null)
            {
                _noise = _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                ApplyCameraEffectsTween();
            }
        }
        StartFailsafeTimer();
        StartRandomToggleLoop();
    }
    
    private void ApplyCameraEffectsTween()
    {
        if (_noise == null) return;
        DOTween.To(() => _noise.m_AmplitudeGain, x => _noise.m_AmplitudeGain = x, 0.5f, 0.5f).SetId(this).SetUpdate(true);
        DOTween.To(() => _noise.m_FrequencyGain, x => _noise.m_FrequencyGain = x, 0.5f, 0.5f).SetId(this).SetUpdate(true);
    }
    
    private void ResetCameraEffectsTween()
    {
        if (_noise == null) return;
        DOTween.To(() => _noise.m_AmplitudeGain, x => _noise.m_AmplitudeGain = x, 0f, 0.5f).SetId(this).SetUpdate(true);
        DOTween.To(() => _noise.m_FrequencyGain, x => _noise.m_FrequencyGain = x, 0f, 0.5f).SetId(this).SetUpdate(true);
    }
    
    private void ApplyEffectsTween()
    {
        if (_vignette != null)
            DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x, 0.4f, 0.5f).SetId(this).SetUpdate(true);
        if (_filmGrain != null)
            DOTween.To(() => _filmGrain.intensity.value, x => _filmGrain.intensity.value = x, 1f, 0.5f).SetId(this).SetUpdate(true);
        if (_depthOfField != null)
            DOTween.To(() => _depthOfField.focalLength.value, x => _depthOfField.focalLength.value = x, 300f, 0.5f).SetId(this).SetUpdate(true);
    }
    
    private void ResetEffectsTween()
    {
        if (_vignette != null)
            DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x, _defaultVignetteIntensity, 0.5f).SetId(this).SetUpdate(true);
        if (_filmGrain != null)
            DOTween.To(() => _filmGrain.intensity.value, x => _filmGrain.intensity.value = x, _defaultFilmGrainIntensity, 0.5f).SetId(this).SetUpdate(true);
        if (_depthOfField != null)
            DOTween.To(() => _depthOfField.focalLength.value, x => _depthOfField.focalLength.value = x, _defaultFocalLength, 0.5f).SetId(this).SetUpdate(true);
    }
    
    private void StartRandomToggleLoop()
    {
        if (!_consequenceActive) return;
        if (_toggleSequence != null && _toggleSequence.IsActive())
            _toggleSequence.Kill();
        float delayOn = Random.Range(4f, 10f);
        float delayOff = Random.Range(3f, 6f);
        _toggleSequence = DOTween.Sequence();
        _toggleSequence.AppendInterval(delayOn)
            .AppendCallback(() =>
            {
                if (!_consequenceActive) return;
                ResetEffectsTween();
                ResetCameraEffectsTween();
            })
            .AppendInterval(delayOff)
            .AppendCallback(() =>
            {
                if (!_consequenceActive) return;
                ApplyEffectsTween();
                ApplyCameraEffectsTween();
                StartFailsafeTimer();
            })
            .OnComplete(() =>
            {
                if (_consequenceActive)
                    StartRandomToggleLoop();
            })
            .SetId(this).SetUpdate(true);
    }
    
    private void StartFailsafeTimer()
    {
        if (_failsafeTween != null && _failsafeTween.IsActive())
            _failsafeTween.Kill();
        _failsafeTween = DOTween.Sequence().SetId(this).Append(DOVirtual.DelayedCall(12f, () =>
        {
            if (_consequenceActive)
            {
                Debug.Log("Failsafe: resetting effects.");
                ResetEffectsTween();
                ResetCameraEffectsTween();
            }
        }).SetUpdate(true));
    }
    
    public override void RemoveConsequence()
    {
        Debug.Log("Consequence removed");
        _consequenceActive = false;
        DOTween.Kill(this);
        ResetEffectsTween();
        ResetCameraEffectsTween();
    }
}
