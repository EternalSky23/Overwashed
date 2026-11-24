using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClothState
{
    Broken = -1,
    NotHandled,
    Washed,
    Dried,
    Finished,
}

public enum ClothType
{
    TShirt,
    Shirt,
    Sweater,
    Suit,
}

/// <summary>
/// Base class of every cloth. Each cloth type defines an ordered procedure (state machine).
/// Doing any step out of order breaks the cloth (for now; repair mechanics may come later).
/// A stained cloth must go through StainRemoval before anything else.
/// </summary>
public abstract class ClothBase : MonoBehaviour, IHoldable
{
    [SerializeField]
    private ClothState clothState = ClothState.NotHandled;
    [SerializeField]
    protected int profit = 0;
    [SerializeField]
    protected int compensate = 0;
    [SerializeField]
    protected int scoreGain = 0;

    [SerializeField]
    private bool isStained = false;
    [SerializeField]
    private int stepIndex = 0;

    public bool IsBroken => clothState < 0;
    public bool IsFinished => clothState == ClothState.Finished;
    public bool IsStained => isStained;
    public ClothState State => clothState;
    public int Profit => profit;
    public int Compensate => compensate;
    public int ScoreGain => scoreGain;
    public string BrokenReason { get; private set; } = "";

    /// <summary>The order this cloth belongs to (set by OrderManager).</summary>
    public Order Order { get; set; }

    /// <summary>The station the cloth is currently placed on (null when held by the player).</summary>
    public Station CurrentStation { get; set; }

    public event Action<ClothBase> OnBroken;
    public event Action<ClothBase> OnChanged;

    // ---- defined by every cloth type ----
    public abstract ClothType Type { get; }
    public abstract string DisplayName { get; }
    /// <summary>Correct procedure, without the optional stain removal step.</summary>
    protected abstract ProcessStep[] Procedure { get; }
    protected abstract int DefaultProfit { get; }
    protected abstract int DefaultCompensate { get; }
    protected abstract int DefaultScore { get; }
    public virtual bool CanBeStained => true;

    private SpriteRenderer bodyRenderer;
    private SpriteRenderer stainRenderer;
    private SpriteRenderer brokenRenderer;

    protected virtual void Awake()
    {
        if (profit == 0) profit = DefaultProfit;
        if (compensate == 0) compensate = DefaultCompensate;
        if (scoreGain == 0) scoreGain = DefaultScore;
        BuildVisual();
    }

    // Start is called before the first frame update
    void Start()
    {
        RefreshVisual();
    }

    // ---------------- State machine ----------------

    public ProcessStep[] GetProcedure()
    {
        return Procedure;
    }

    public int StepIndex => stepIndex;

    /// <summary>The step this cloth expects next, or null when broken / finished.</summary>
    public ProcessStep? NextStep
    {
        get
        {
            if (IsBroken) return null;
            if (isStained) return ProcessStep.StainRemoval;
            if (stepIndex >= Procedure.Length) return null;
            return Procedure[stepIndex];
        }
    }

    public void SetStained(bool stained)
    {
        isStained = stained && CanBeStained;
        RefreshVisual();
        OnChanged?.Invoke(this);
    }

    /// <summary>
    /// Try to apply a process step. Returns true when the step was correct.
    /// A wrong step breaks the cloth. Stain removal on a clean cloth does nothing and returns false.
    /// </summary>
    public bool TryProcess(ProcessStep step)
    {
        if (IsBroken) return false;

        if (step == ProcessStep.StainRemoval)
        {
            if (!isStained) return false;
            isStained = false;
            RefreshVisual();
            OnChanged?.Invoke(this);
            return true;
        }

        if (isStained)
        {
            Break($"{DisplayName}: the stain got set permanently! (remove stains first)");
            return false;
        }

        if (NextStep != step)
        {
            Break(WrongStepReason(step));
            return false;
        }

        stepIndex++;
        if (stepIndex >= Procedure.Length) ChangeState(ClothState.Finished);
        else if (ProcessStepUtil.IsDry(step)) ChangeState(ClothState.Dried);
        else if (ProcessStepUtil.IsWash(step)) ChangeState(ClothState.Washed);
        else ChangeState(clothState); // iron etc. keeps the state, just refresh

        return true;
    }

    /// <summary>Flavour text when a wrong step ruins this cloth. Cloth types can override it.</summary>
    protected virtual string WrongStepReason(ProcessStep step)
    {
        return $"{DisplayName} ruined by {ProcessStepUtil.ShortName(step)} (wrong step)!";
    }

    /// <summary>Random event: the cloth gets dirty again and must restart its procedure.</summary>
    public void MakeDirty()
    {
        if (IsBroken) return;
        isStained = true;
        stepIndex = 0;
        ChangeState(ClothState.NotHandled);
    }

    public void Break(string reason)
    {
        if (IsBroken) return;
        BrokenReason = reason;
        ChangeState(ClothState.Broken);
    }

    public void ChangeState(ClothState state)
    {
        if (this.clothState == ClothState.Broken)
        {
            // If broken, it can not be reverted.
            return;

            // TODO: we may add some repair mechanics later on
        }

        this.clothState = state;
        if (this.clothState == ClothState.Broken)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ModifyCash(-compensate);
            }
            RefreshVisual();
            OnChanged?.Invoke(this);
            OnBroken?.Invoke(this);
            return;
        }
        // NOTE: profit / score are now granted on delivery (see Deliver), not when Finished,
        // so a finished cloth that is never returned to the customer earns nothing.
        RefreshVisual();
        OnChanged?.Invoke(this);
    }

    /// <summary>Called when a finished cloth is handed back to the customer.</summary>
    public void Deliver()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.ModifyCash(profit);
        GameManager.Instance.ModifyScore(scoreGain);
        GameManager.Instance.ClothesDelivered++;
    }

    // Leader's original API: machines call these.
    public abstract void Wash(WashMethod method, Dictionary<string, object> args);
    public abstract void Dry(DryMethod method, Dictionary<string, object> args);

    // ---------------- IHoldable ----------------

    public void AttachTo(Transform anchor, int sortingOrder)
    {
        transform.SetParent(anchor, false);
        transform.localPosition = Vector3.zero;
        SetSorting(sortingOrder);
    }

    public void Detach()
    {
        CurrentStation = null;
    }

    // ---------------- Visual ----------------

    private void BuildVisual()
    {
        bodyRenderer = gameObject.AddComponent<SpriteRenderer>();
        bodyRenderer.sprite = PixelArt.Get(PixelArt.ClothKey(Type));

        stainRenderer = CreateLayer("Stain", PixelArt.Get("stain"));
        brokenRenderer = CreateLayer("Broken", PixelArt.Get("broken"));
        transform.localScale = Vector3.one * 0.85f;
        SetSorting(5);
    }

    private SpriteRenderer CreateLayer(string name, Sprite sprite)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = sprite;
        return r;
    }

    public void SetSorting(int order)
    {
        if (bodyRenderer == null) return;
        bodyRenderer.sortingOrder = order;
        stainRenderer.sortingOrder = order + 1;
        brokenRenderer.sortingOrder = order + 2;
    }

    private void RefreshVisual()
    {
        if (bodyRenderer == null) return;
        stainRenderer.enabled = isStained && !IsBroken;
        brokenRenderer.enabled = IsBroken;
        bodyRenderer.color = IsBroken ? new Color(0.55f, 0.55f, 0.55f) : Color.white;
        if (IsFinished) bodyRenderer.sprite = PixelArt.Get(PixelArt.ClothKey(Type) + "_folded");
        else bodyRenderer.sprite = PixelArt.Get(PixelArt.ClothKey(Type));
    }
}
