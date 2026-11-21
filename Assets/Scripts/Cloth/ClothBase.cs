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

public abstract class ClothBase : MonoBehaviour
{
    [SerializeField]
    private ClothState clothState = ClothState.NotHandled;
    private int profit = 0;
    private int compensate = 0;
    private int scoreGain = 0;

    public bool IsBroken => clothState < 0;
    public bool IsFinished => clothState == ClothState.Finished;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
            GameManager.Instance.ModifyCash(-compensate);
            return;
        }
        if (this.clothState == ClothState.Finished)
        {
            GameManager.Instance.ModifyCash(profit);
            GameManager.Instance.ModifyScore(scoreGain);
        }
    }

    public abstract void Wash(WashMethod method, Dictionary<string, object> args);
    public abstract void Dry(DryMethod method, Dictionary<string, object> args);
}
