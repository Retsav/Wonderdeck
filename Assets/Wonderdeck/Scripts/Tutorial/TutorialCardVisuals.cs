using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FishNet.Managing;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class TutorialCardVisuals : MonoBehaviour
{
    [SerializeField] private Transform firstPlayerCardsSpawnPoint;
    [SerializeField] private Transform secondPlayerCardsSpawnPoint;
    [SerializeField] private Transform firstPlayerFirstCardPosition;
    [SerializeField] private Transform secondPlayerFirstCardPosition;
    
    [SerializeField] private Transform cardsParent;
    [SerializeField] private Texture unknownCardTexture;
    [SerializeField] private Texture cardsSpriteAtlas;
    
    
    [SerializeField] private GameObject cardPrefab;

    private MaterialPropertyBlock _mpb;
    
    private float _positionOffset = -0.4f;    
    private int firstPlayerSpawnedCardsCount = 0;
    private int secondPlayerSpawnedCardsCount = 0;


    [SerializeField] private float cardSpacing = .3f;

    private AudioConfig _audioConfig;
    private Queue<IEnumerator> _cardSpawnQueue = new Queue<IEnumerator>();
    private CardVisual _hiddenCard = null;
    
    private List<CardVisual> _spawnedCardVisuals;
    private bool _isProcessingQueue;

    private IBlackjackService _blackjackService;

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }


    private void Start()
    {
        _mpb = new MaterialPropertyBlock();
        _audioConfig = DebugConfigLoader.Instance.GetConfig<AudioConfig>();
    }

    public void SpawnCardVisual(CardSO card, PlayerType playerType, bool hidden)
    {
        switch (playerType)
        {
            case PlayerType.Player1:
                EnqueueCardSpawn(card, firstPlayerCardsSpawnPoint, playerType, hidden);
                break;
            case PlayerType.Player2:
                EnqueueCardSpawn(card, secondPlayerCardsSpawnPoint, playerType, hidden);
                break;
        }
    }
    
    public void DestroyCard(string cardID, PlayerType playerType)
    {
        foreach (Transform child in cardsParent)
        {
            if (child.TryGetComponent(out CardVisual cardVisual))
            {
                if (cardVisual.owner != playerType) continue;
                if (cardID == cardVisual.cardID)
                {
                    Destroy(cardVisual.transform.gameObject);
                    if (cardVisual.owner == PlayerType.Player1)
                        firstPlayerSpawnedCardsCount--;
                    else
                        secondPlayerSpawnedCardsCount--;
                }
            }
            else
                Debug.LogWarning("There is a child in CardsParent without CardVisual Component.");
        }
    }

    public void DestroyAllCards()
    {
        foreach (Transform child in cardsParent)
        {
            if (child.TryGetComponent(out CardVisual cardVisual))
                Destroy(cardVisual.transform.gameObject);
            else
                Debug.LogWarning("There is a child in CardsParent without CardVisual Component.");
        }
        firstPlayerSpawnedCardsCount = 0;
        secondPlayerSpawnedCardsCount = 0;
    }
    

    private void EnqueueCardSpawn(CardSO card, Transform spawnPoint, PlayerType playerType, bool hidden)
    {
        _cardSpawnQueue.Enqueue(SpawnAndPositionCards(card, spawnPoint, playerType, hidden));
        if (!_isProcessingQueue) StartCoroutine(ProcessCardSpawnQueue());
    }

    private IEnumerator ProcessCardSpawnQueue()
    {
        _isProcessingQueue = true;
        while (_cardSpawnQueue.Count > 0)
        {
            yield return StartCoroutine(_cardSpawnQueue.Dequeue());
            yield return new WaitForSeconds(0.15f); 
        }
        _isProcessingQueue = false;
    }
    

    private IEnumerator SpawnAndPositionCards(CardSO card, Transform spawnPoint, PlayerType playerType, bool hidden)
    {
        yield return new WaitForSeconds(0.15f);
        var cardVisualPrefab = Instantiate(cardPrefab, spawnPoint.position, Quaternion.identity, cardsParent);
        if (cardVisualPrefab.TryGetComponent(out CardVisual cardVisual))
        {
            cardVisual.cardID = card.CardId;
            cardVisual.owner = playerType;
            var uvCoordinates = GetUVCoordinatesForSprite(card.cardFacePath);
            var scaleX  = uvCoordinates.z;
            var scaleY  = uvCoordinates.w;
            var offsetX = uvCoordinates.x;
            var offsetY = uvCoordinates.y;
            if (!hidden)
            {
                if (uvCoordinates != Vector4.zero)
                {
                    _mpb.SetTexture("_BaseMap", cardsSpriteAtlas);
                    _mpb.SetVector("_Tiling", new Vector2(scaleX, scaleY));
                    _mpb.SetVector("_Offest", new Vector2(offsetX, offsetY));
                    _mpb.SetFloat("_Dissolve", 0);
                }
                else
                {
                    _mpb.SetTexture("_BaseMap", unknownCardTexture);
                    _mpb.SetVector("_Tiling", new Vector2(1, 1));
                    _mpb.SetVector("_Offest", new Vector2(0, 0));
                    _mpb.SetFloat("_Dissolve", 0);
                }
            }
            else
            {
                _mpb.SetTexture("_BaseMap", unknownCardTexture);
                _mpb.SetVector("_Tiling", new Vector2(1, 1));
                _mpb.SetVector("_Offest", new Vector2(0, 0));
                _mpb.SetFloat("_Dissolve", 0);
                _hiddenCard = cardVisual;
            }
            cardVisual.cardMeshRenderer.SetPropertyBlock(_mpb);
        }
        cardVisual.particleSystemGameObject.SetActive(false);

        if (playerType == PlayerType.Player1)
            firstPlayerSpawnedCardsCount++;
        else
            secondPlayerSpawnedCardsCount++;
        TutorialAudioManager.Instance.OnPlaySoundAtPosition(cardVisualPrefab.transform.position, _audioConfig.cardSwooshPaths[Random.Range(0, _audioConfig.cardSwooshPaths.Count)]);
        cardVisualPrefab.transform.DOMoveX(_positionOffset + 
                                           (playerType == PlayerType.Player1 
                                               ? firstPlayerSpawnedCardsCount
                                               : secondPlayerSpawnedCardsCount) * cardSpacing, 0.3f);
        cardVisualPrefab.transform.DOMoveZ(playerType == PlayerType.Player1
            ? firstPlayerFirstCardPosition.position.z
            : secondPlayerFirstCardPosition.position.z, 0.3f);
    }
    
    public void RevealHiddenCards()
    {
        var uvCoordinates = GetUVCoordinatesForSprite(_blackjackService.GetCardByID(_hiddenCard.cardID).cardFacePath);
        var scaleX  = uvCoordinates.z;
        var scaleY  = uvCoordinates.w;
        var offsetX = uvCoordinates.x;
        var offsetY = uvCoordinates.y;
        if (uvCoordinates != Vector4.zero)
        {
            _mpb.SetTexture("_BaseMap", cardsSpriteAtlas);
            _mpb.SetVector("_Tiling", new Vector2(scaleX, scaleY));
            _mpb.SetVector("_Offest", new Vector2(offsetX, offsetY));
            _mpb.SetFloat("_Dissolve", 0);
        }
        else
        {
            _mpb.SetTexture("_BaseMap", unknownCardTexture);
            _mpb.SetVector("_Tiling", new Vector2(1, 1));
            _mpb.SetVector("_Offest", new Vector2(0, 0));
            _mpb.SetFloat("_Dissolve", 0);
        }
        _hiddenCard.cardMeshRenderer.SetPropertyBlock(_mpb);
    }

    private Vector4 GetUVCoordinatesForSprite(string spritePath)
    {
        if (string.IsNullOrEmpty(spritePath))
            return Vector4.zero;


        string[] parts = spritePath.Split('|');
        if (parts.Length != 2)
        {
            Debug.LogWarning($"Invalid sprite path format: {spritePath}. Expected format: 'AtlasPath|SpriteName'.");
            return Vector4.zero;
        }

        string atlasPath = parts[0];
        string spriteName = parts[1];


        Sprite[] sprites = Resources.LoadAll<Sprite>(atlasPath);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"No sprites found in atlas at path: {atlasPath}");
            return Vector4.zero;
        }


        Sprite sprite = Array.Find(sprites, s => s.name == spriteName);
        if (sprite == null)
        {
            Debug.LogWarning($"Sprite '{spriteName}' not found in atlas '{atlasPath}'");
            return Vector4.zero;
        }


        Rect textureRect = sprite.textureRect;
        Texture texture = sprite.texture;
        if (texture == null)
        {
            Debug.LogWarning($"Texture for sprite '{spriteName}' in atlas '{atlasPath}' is null.");
            return Vector4.zero;
        }

        float atlasWidth = texture.width;
        float atlasHeight = texture.height;

        return new Vector4(
            textureRect.x / atlasWidth,        
            textureRect.y / atlasHeight,       
            textureRect.width / atlasWidth,    
            textureRect.height / atlasHeight   
        );
    }



}
