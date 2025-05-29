using System.Collections;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class ParticleColorToPlayerHealth : NetworkBehaviour
{
    [SerializeField] private ParticleSystem fireflyParticles;
    [SerializeField] private float colorTransitionDuration = 1f;

    private IHealthService _healthService;
    private ParticleSystem.MainModule _mainModule;
    private Coroutine _colorCoroutine;

    [Inject]
    private void ResolveDependencies(IHealthService healthService)
    {
        _healthService = healthService;
    }

    private void Start()
    {
        _mainModule = fireflyParticles.main;
        _healthService.DamageAppliedEvent += OnDamageApplied;
    }

    private void OnDestroy()
    {
        _healthService.DamageAppliedEvent -= OnDamageApplied;
    }

    private void OnDamageApplied(object sender, DamageAppliedEventArgs e)
    {
        var playerHealth = NetworkManager.ClientManager.Connection.IsHost
            ? _healthService.FirstPlayerHealth
            : _healthService.SecondPlayerHealth;

        float healthFraction = Mathf.Clamp01((float)playerHealth / _healthService.MaxHealth);

        Color targetColor;
        if (healthFraction > 0.5f)
        {
            float t = (1f - healthFraction) / 0.5f;
            targetColor = Color.Lerp(Color.green, Color.yellow, t);
        }
        else
        {
            float t = (0.5f - healthFraction) / 0.5f;
            targetColor = Color.Lerp(Color.yellow, Color.red, t);
        }

        if (_colorCoroutine != null)
            StopCoroutine(_colorCoroutine);

        _colorCoroutine = StartCoroutine(TransitionColor(_mainModule.startColor.color, targetColor));
    }

    private IEnumerator TransitionColor(Color from, Color to)
    {
        float elapsed = 0f;
        while (elapsed < colorTransitionDuration)
        {
            elapsed += Time.deltaTime;
            Color lerped = Color.Lerp(from, to, elapsed / colorTransitionDuration);
            _mainModule.startColor = new ParticleSystem.MinMaxGradient(lerped);
            yield return null;
        }
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[fireflyParticles.main.maxParticles];
        int count = fireflyParticles.GetParticles(particles);
        fireflyParticles.SetParticles(particles, count);
        _mainModule.startColor = new ParticleSystem.MinMaxGradient(to);
    }
}
