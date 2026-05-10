using System.Collections.Generic;
using UnityEngine;

public abstract class UIScreen : MonoBehaviour 
{
    [SerializeField] private ScreenTag[] _screenTags;

    private CanvasGroup _canvasGroup;

    public IReadOnlyList<ScreenTag> ScreenTags => _screenTags;
    public CanvasGroup CanvasGroup
    {
        get
        {
            if (!_canvasGroup)
                gameObject.TryGetComponent(out _canvasGroup);

            if (!_canvasGroup)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            return _canvasGroup;
        }
    }
}
