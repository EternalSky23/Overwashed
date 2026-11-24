using UnityEngine;

/// <summary>
/// Drop real art here! Any sprite assigned in the Inspector replaces the generated placeholder pixel art.
/// (Lives on the "ArtLibrary" object in the MainGame scene.)
/// </summary>
public class ArtLibrary : MonoBehaviour
{
    public static ArtLibrary Instance { get; private set; }

    [Header("Characters")]
    public Sprite player;
    public Sprite kid;

    [Header("Stations")]
    public Sprite washer;
    public Sprite dryer;
    public Sprite dryCleaner;
    public Sprite handWash;
    public Sprite dryingRack;
    public Sprite stainStation;
    public Sprite ironBoard;
    public Sprite foldTable;
    public Sprite counter;
    public Sprite table;
    public Sprite trash;

    [Header("Clothes")]
    public Sprite tshirt;
    public Sprite tshirtFolded;
    public Sprite shirt;
    public Sprite shirtFolded;
    public Sprite sweater;
    public Sprite sweaterFolded;
    public Sprite suit;
    public Sprite suitFolded;
    public Sprite stainOverlay;
    public Sprite brokenOverlay;

    [Header("Items / Environment")]
    public Sprite upgradeKit;
    public Sprite floor;
    public Sprite wall;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Returns the Inspector override for a PixelArt key, or null.</summary>
    public Sprite Find(string key)
    {
        switch (key)
        {
            case "player": return player;
            case "kid": return kid;
            case "washer": return washer;
            case "dryer": return dryer;
            case "drycleaner": return dryCleaner;
            case "handwash": return handWash;
            case "rack": return dryingRack;
            case "stainstation": return stainStation;
            case "iron": return ironBoard;
            case "fold": return foldTable;
            case "counter": return counter;
            case "table": return table;
            case "trash": return trash;
            case "cloth_tshirt": return tshirt;
            case "cloth_tshirt_folded": return tshirtFolded;
            case "cloth_shirt": return shirt;
            case "cloth_shirt_folded": return shirtFolded;
            case "cloth_sweater": return sweater;
            case "cloth_sweater_folded": return sweaterFolded;
            case "cloth_suit": return suit;
            case "cloth_suit_folded": return suitFolded;
            case "stain": return stainOverlay;
            case "broken": return brokenOverlay;
            case "kit": return upgradeKit;
            case "floor": return floor;
            case "wall": return wall;
        }
        return null;
    }
}
