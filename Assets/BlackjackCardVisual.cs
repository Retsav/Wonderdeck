using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

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

    List<CardVisual> spawnedCardVisuals = new List<CardVisual>();
    private IBlackjackService _blackjackService;


    private int firstPlayerSpawnedCardsCount = 0;
    private int secondPlayerSpawnedCardsCount = 0;

    private MaterialPropertyBlock _mpb;

    [Inject]
    private void ResolveSingleDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }

    public override void OnStartClient()
    {
        _blackjackService.CardVisualRequested += OnCardVisualRequested;
        _mpb = new MaterialPropertyBlock();
        foreach (Transform child in cardsParent) Destroy(child.gameObject);
    }

    private void OnCardVisualRequested(object sender, CardVisualRequestedEventArgs e)
    {
        switch (e.Owner)
        {
            case PlayerType.Player1:
                if (e.Transaction == TransactionType.ADD)
                    SpawnAndPositionCards(e.Card, ref firstPlayerSpawnedCardsCount, firstPlayerCardsSpawnPoint, -0.4f, PlayerType.Player1);
                else
                    RemoveCards(e.Card, ref firstPlayerSpawnedCardsCount, PlayerType.Player1);
                break;
            case PlayerType.Player2:
                if (e.Transaction == TransactionType.ADD)
                    SpawnAndPositionCards(e.Card, ref secondPlayerSpawnedCardsCount, secondPlayerCardsSpawnPoint, -0.4f, PlayerType.Player2);
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

    private void SpawnAndPositionCards(CardClientData card, ref int spawnedCardsCount, Transform spawnPoint, float initialPositionOffset, PlayerType playerType)
    {
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
            }
            else
            {
                _mpb.SetVector("_BaseMap_ST", new Vector4(1, 1, 0, 0));
                _mpb.SetTexture("_BaseMap", unknownCardTexture);
            }
                
            cardVisual.cardMeshRenderer.SetPropertyBlock(_mpb);
        }

        spawnedCardsCount++;
        cardVisualPrefab.transform.DOMoveX(initialPositionOffset + spawnedCardsCount * cardSpacing, 0.3f);
        cardVisualPrefab.transform.DOMoveZ(playerType == PlayerType.Player1
            ? firstPlayerFirstCardPosition.position.z
            : secondPlayerFirstCardPosition.position.z, 0.3f);
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
