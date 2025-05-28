using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FishNet.Object;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class BlackjackCardVisual : NetworkBehaviour
{
    [SerializeField] private Transform firstPlayerCardsSpawnPoint;
    [SerializeField] private Transform secondPlayerCardsSpawnPoint;
    [SerializeField] private Transform firstPlayerFirstCardPosition;
    [SerializeField] private Transform secondPlayerFirstCardPosition;
    
    [SerializeField] private Transform cardsParent;
    [SerializeField] private Texture unknownCardTexture;
    [SerializeField] private Texture cardsSpriteAtlas;
    
    
    [SerializeField] private GameObject cardPrefab;
    

    [SerializeField] private float cardSpacing = .3f;


    
    private IBlackjackService _blackjackService;
    private IAudioService _audioService;


    private int firstPlayerSpawnedCardsCount = 0;
    private int secondPlayerSpawnedCardsCount = 0;

    private MaterialPropertyBlock _mpb;
    private Queue<IEnumerator> _cardSpawnQueue = new Queue<IEnumerator>();
    private bool _isProcessingQueue = false;

    private AudioConfig _audioConfig;


    [Inject]
    private void ResolveSingleDependencies(IBlackjackService blackjackService, IAudioService audioService)
    {
        _blackjackService = blackjackService;
        _audioService = audioService;
    }

    public override void OnStartClient()
    {
        _blackjackService.CardVisualRequested += OnCardVisualRequested;
        _blackjackService.RevealCardsVisualEvent += OnCardsReveal;
        _mpb = new MaterialPropertyBlock();
        _audioConfig = DebugConfigLoader.Instance.GetConfig<AudioConfig>();
        foreach (Transform child in cardsParent) Destroy(child.gameObject);
    }

    private void OnCardsReveal(object sender, RevealCardsEventVisualArgs e)
    {
        foreach (Transform child in cardsParent)
        {
            if (child.TryGetComponent(out CardVisual cardVisual))
            {
                if(cardVisual.cardID != e.DummyCardID)
                    continue;
                cardVisual.cardID = e.OrginalCardID;
                var card = _blackjackService.GetCardByID(e.OrginalCardID);
                var uvCoordinates = GetUVCoordinatesForSprite(card.cardFacePath);
                if (uvCoordinates != Vector4.zero)
                {
                    _mpb.SetVector("_BaseMap_ST", new Vector4(uvCoordinates.z, uvCoordinates.w, uvCoordinates.x, uvCoordinates.y));
                    _mpb.SetTexture("_BaseMap", cardsSpriteAtlas);
                    cardVisual.particleSystemGameObject.SetActive(false);
                }
                else
                {
                    _mpb.SetVector("_BaseMap_ST", new Vector4(1, 1, 0, 0));
                    _mpb.SetTexture("_BaseMap", unknownCardTexture);
                    cardVisual.particleSystemGameObject.SetActive(true);
                }
                cardVisual.cardMeshRenderer.SetPropertyBlock(_mpb);
            }
            else
                Debug.LogWarning("There is a child in CardsParent without CardVisual Component.");
        }
    }

    private void OnCardVisualRequested(object sender, CardVisualRequestedEventArgs e)
    {
        switch (e.Owner)
        {
            case PlayerType.Player1:
                if (e.Transaction == TransactionType.ADD)
                    EnqueueCardSpawn(e.Card, firstPlayerCardsSpawnPoint, -0.4f, PlayerType.Player1, e.ShowParticles);
                else
                    RemoveCards(e.Card, ref firstPlayerSpawnedCardsCount, PlayerType.Player1);
                break;
            case PlayerType.Player2:
                if (e.Transaction == TransactionType.ADD)
                    EnqueueCardSpawn(e.Card, secondPlayerCardsSpawnPoint, -0.4f, PlayerType.Player2, e.ShowParticles);
                else
                    RemoveCards(e.Card, ref secondPlayerSpawnedCardsCount, PlayerType.Player2);
                break;
        }
    }
    
    
    

    private void RemoveCards(CardClientData removedCard, ref int spawnedCardsCount, PlayerType playerType)
    {
        foreach (Transform child in cardsParent)
        {
            if (child.TryGetComponent(out CardVisual cardVisual))
            {
                if (cardVisual.owner != playerType) continue;
                if (removedCard.CardID == cardVisual.cardID)
                {
                    Destroy(cardVisual.transform.gameObject);
                    spawnedCardsCount--;
                }
            }
            else
                Debug.LogWarning("There is a child in CardsParent without CardVisual Component.");
        }
    }

    private IEnumerator SpawnAndPositionCards(CardClientData card, Transform spawnPoint, float initialPositionOffset, PlayerType playerType, bool showParticles)
    {
        yield return new WaitForSeconds(0.15f);
        var cardVisualPrefab = Instantiate(cardPrefab, spawnPoint.position, NetworkManager.ClientManager.Connection.IsHost ? Quaternion.identity : Quaternion.Euler(new Vector3(0f, 180f, 0f)), cardsParent);
        if (cardVisualPrefab.TryGetComponent(out CardVisual cardVisual))
        {
            cardVisual.cardID = card.CardID;
            cardVisual.owner = playerType;

            var uvCoordinates = GetUVCoordinatesForSprite(card.CardFaceSpritePath);
            if (uvCoordinates != Vector4.zero)
            {
                _mpb.SetVector("_BaseMap_ST", new Vector4(uvCoordinates.z, uvCoordinates.w, uvCoordinates.x, uvCoordinates.y));
                _mpb.SetTexture("_BaseMap", cardsSpriteAtlas);
                cardVisual.particleSystemGameObject.SetActive(false);
            }
            else
            {
                _mpb.SetVector("_BaseMap_ST", new Vector4(1, 1, 0, 0));
                _mpb.SetTexture("_BaseMap", unknownCardTexture);
                cardVisual.particleSystemGameObject.SetActive(true);
            }
            
            if(showParticles)
                cardVisual.particleSystemGameObject.SetActive(true);
            
            cardVisual.cardMeshRenderer.SetPropertyBlock(_mpb);
        }

        if (playerType == PlayerType.Player1)
            firstPlayerSpawnedCardsCount++;
        else
            secondPlayerSpawnedCardsCount++;
        
        cardVisualPrefab.transform.DOMoveX(initialPositionOffset + 
                                           (playerType == PlayerType.Player1 
                                               ? firstPlayerSpawnedCardsCount
                                               : secondPlayerSpawnedCardsCount) * cardSpacing, 0.3f);
        cardVisualPrefab.transform.DOMoveZ(playerType == PlayerType.Player1
            ? firstPlayerFirstCardPosition.position.z
            : secondPlayerFirstCardPosition.position.z, 0.3f);
        
        _audioService.OnPlaySoundAtPosition(cardVisualPrefab.transform.position, _audioConfig.cardSwooshPaths[Random.Range(0, _audioConfig.cardSwooshPaths.Count)]);
    }
    
    private void EnqueueCardSpawn(CardClientData card, Transform spawnPoint, float initialPositionOffset, PlayerType playerType, bool showParticles)
    {
        _cardSpawnQueue.Enqueue(SpawnAndPositionCards(card, spawnPoint, initialPositionOffset, playerType, showParticles));
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



    private void OnDestroy()
    {
        _blackjackService.CardVisualRequested -= OnCardVisualRequested;
    }
}
