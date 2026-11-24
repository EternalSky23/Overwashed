using UnityEngine;

/// <summary>
/// 4-direction idle / walk animation from a Townsfolk sprite sheet (see ThirdPartyArt).
/// </summary>
public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private float idleFps = 2f;
    [SerializeField] private float walkFps = 8f;

    private SpriteRenderer sr;
    private Sprite[,] frames;
    private float time;
    private int lastDir;

    /// <summary>Adds the animator if the sheet exists, otherwise returns null (keep the placeholder sprite).</summary>
    public static CharacterAnimator TryAttach(SpriteRenderer renderer, string character, float pixelsPerUnit)
    {
        var frames = ThirdPartyArt.TryGetCharacter(character, pixelsPerUnit);
        if (frames == null) return null;
        var anim = renderer.gameObject.AddComponent<CharacterAnimator>();
        anim.sr = renderer;
        anim.frames = frames;
        renderer.flipX = false;
        renderer.sprite = frames[0, 0];
        return anim;
    }

    /// <summary>Call every frame with the facing direction and whether the character is walking.</summary>
    public void Set(Vector2 facing, bool moving)
    {
        time += Time.deltaTime;
        int dir = lastDir;
        if (facing.sqrMagnitude > 0.0001f)
        {
            if (Mathf.Abs(facing.x) > Mathf.Abs(facing.y)) dir = facing.x < 0f ? 1 : 2;
            else dir = facing.y > 0f ? 3 : 0;
        }
        lastDir = dir;

        int frame = moving ? 2 + (int)(time * walkFps) % 4 : (int)(time * idleFps) % 2;
        sr.sprite = frames[dir, frame];
    }
}
