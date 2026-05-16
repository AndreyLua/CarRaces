using TMPro;
using UnityEngine;

public class SpeedUIScreen : UIScreen
{
    [SerializeField] private TMP_Text _gearText;
    private TMP_Text _speedText;

    private void Awake()
    {
        _speedText = gameObject.GetComponent<TMP_Text>();
    }

    public void SetSpeed(int speed)
    {
        _speedText.text = speed.ToString();
    }

    public void SetGear(int gear)
    {
        _gearText.text = gear.ToString();
    }
}
