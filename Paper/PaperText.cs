using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DrakesEssentials.Paper;

/// <summary>
/// Draws a paper sign's writing on the sheet in ink, with Unity UI <see cref="Text"/> (the sign's own TextMeshPro
/// board is hidden). The text comes from the sign's widget, which Valheim keeps up to date (sync, filtering), and is
/// drawn on both faces of the sheet, so it reads correctly from whichever side you see; the sheet hides the other.
/// </summary>
public class PaperText : MonoBehaviour
{
    /// <summary>Pinned to a wall (sheet standing, hanging below the pivot) or lying flat (sheet ahead of the pivot).</summary>
    public bool m_wall = true;

    // RenameIt's sheet: US letter x 1.5 (Packs/Paper recipes).
    private const float SheetWidth = 0.3239f;
    private const float SheetHeight = 0.4191f;
    private const float Margin = 0.03f;
    /// <summary>Canvas units per metre: 1 unit = 1 mm, so font sizes are letter heights in mm.</summary>
    private const float UnitsPerMetre = 1000f;
    private const int MinFontSize = 14;
    private const int MaxFontSize = 48;
    private const float RefreshSeconds = 0.25f;

    private static readonly Color Ink = new(0.13f, 0.09f, 0.06f, 1f);
    private static Font? _font;

    private Sign? _sign;
    private Text[] _faces = new Text[0];
    private string? _shown;
    private float _nextRefresh;

    private void Start()
    {
        _sign = GetComponent<Sign>();
        if (_sign == null)
            return;

        // Valheim keeps setting the sign's TMP text; it just isn't drawn any more.
        if (_sign.m_textWidget != null && _sign.m_textWidget.GetComponentInParent<Canvas>(true) is { } board)
            board.gameObject.SetActive(false);

        _faces = m_wall
            ? new[]
            {
                Face("drake_paper_text_front", new Vector3(0f, -SheetHeight / 2f, 0.004f), Quaternion.LookRotation(Vector3.back, Vector3.up)),
                Face("drake_paper_text_back", new Vector3(0f, -SheetHeight / 2f, -0.004f), Quaternion.LookRotation(Vector3.forward, Vector3.up)),
            }
            : new[]
            {
                // Top of the writing at the sheet's top edge (local -Z, see the pack sprite).
                Face("drake_paper_text_top", new Vector3(0f, 0.006f, SheetHeight / 2f), Quaternion.LookRotation(Vector3.down, Vector3.back)),
                Face("drake_paper_text_under", new Vector3(0f, -0.002f, SheetHeight / 2f), Quaternion.LookRotation(Vector3.up, Vector3.back)),
            };
        Refresh();
    }

    private void Update()
    {
        if (Time.time < _nextRefresh)
            return;
        _nextRefresh = Time.time + RefreshSeconds;
        Refresh();
    }

    private void Refresh()
    {
        if (_sign == null || _sign.m_textWidget == null)
            return;
        var text = _sign.m_textWidget.text ?? "";
        if (text == _shown)
            return;
        _shown = text;
        foreach (var face in _faces)
            face.text = text;
    }

    private Text Face(string name, Vector3 position, Quaternion rotation)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var rect = (RectTransform)go.transform;
        rect.sizeDelta = new Vector2((SheetWidth - Margin * 2) * UnitsPerMetre, (SheetHeight - Margin * 2) * UnitsPerMetre);
        rect.localPosition = position;
        rect.localRotation = rotation;
        rect.localScale = Vector3.one / UnitsPerMetre;

        var textGo = new GameObject("text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var textRect = (RectTransform)textGo.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var text = textGo.AddComponent<Text>();
        text.font = InkFont();
        text.color = Ink;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = MinFontSize;
        text.resizeTextMaxSize = MaxFontSize;
        text.fontSize = MaxFontSize;
        text.supportRichText = false;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>Valheim's serif (Averia) when it's loaded as a Unity font, else Unity's built-in one.</summary>
    private static Font? InkFont()
    {
        if (_font != null)
            return _font;
        _font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(f => f != null && f.name.StartsWith("AveriaSerifLibre-Bold"))
                ?? Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(f => f != null && f.name.StartsWith("AveriaSerif"));
        if (_font == null)
        {
            foreach (var builtin in new[] { "LegacyRuntime.ttf", "Arial.ttf" })
            {
                try
                {
                    _font = Resources.GetBuiltinResource<Font>(builtin);
                }
                catch
                {
                    // Not in this Unity version; try the next name.
                }
                if (_font != null)
                    break;
            }
        }
        return _font;
    }
}
