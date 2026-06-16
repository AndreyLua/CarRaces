using TMPro;
using UnityEngine;

public class TimeUIScreen : UIScreen
{
    private TMP_Text _timeText;

    private void Awake()
    {
        _timeText = gameObject.GetComponent<TMP_Text>();
    }

    public void SeTime(float time)
    {
        _timeText.text = time.ToString("F1") +"s";
    }
}
