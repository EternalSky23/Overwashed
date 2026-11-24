using UnityEngine;

/// <summary>"+$60", "SPLAT!" popups that rise and fade.</summary>
public class FloatingText : MonoBehaviour
{
    private float life;
    private float maxLife;
    private TextMesh[] meshes;
    private Color[] baseColors;

    public static void Spawn(Vector3 pos, string text, Color color, float duration = 1.2f)
    {
        var go = new GameObject("Floating");
        go.transform.position = pos;
        var wt = WorldText.Create(go.transform, Vector2.zero, text, 0.36f, color, 40);
        var ft = go.AddComponent<FloatingText>();
        ft.maxLife = duration;
        ft.meshes = go.GetComponentsInChildren<TextMesh>();
        ft.baseColors = new Color[ft.meshes.Length];
        for (int i = 0; i < ft.meshes.Length; i++) ft.baseColors[i] = ft.meshes[i].color;
    }

    private void Update()
    {
        life += Time.deltaTime;
        transform.position += Vector3.up * Time.deltaTime * 0.9f;
        float a = 1f - life / maxLife;
        for (int i = 0; i < meshes.Length; i++)
        {
            var c = baseColors[i];
            c.a *= a;
            meshes[i].color = c;
        }
        if (life >= maxLife) Destroy(gameObject);
    }
}
