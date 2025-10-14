using TMPro;

public interface IUnreadableMasker
{
    void Refresh();
}
public interface IUnreadableConverter
{
    public enum TMP_WHERE
    {
        Unreadable,
        Content
    }
    public TextMeshProUGUI GetUnreadableTMP();
    public TextMeshProUGUI GetContentTMP();
    public bool GetPiontedIndex(TMP_WHERE where, out int index);
    public string GetTextUnreadable();
    public string GetTextContent();
    public void GenerateFullLengthMask();
}