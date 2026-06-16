using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class UIScreenRepository : MonoBehaviour
{
    private Dictionary<Type, UIScreen> _screens;

    private static UIScreenRepository _instance;

    public static UIScreenRepository Instance
    {
        get
        {
            if (_instance is null)
                Init();

            return _instance;
        }
    }

    public static Canvas Canvas { get; private set; }
    public static CanvasGroup CanvasGroup { get; private set; }
    public static GraphicRaycaster GraphicRaycaster { get; private set; }
    public static IReadOnlyDictionary<Type, UIScreen> Screens => Instance._screens;

    private static void Init()
    {
        if (_instance != null)
            return;

        _instance = FindObjectOfType<UIScreenRepository>();
        Instance._screens = new Dictionary<Type, UIScreen>();

        Canvas = FindObjectsOfType<Canvas>().Find(c => c.renderMode is RenderMode.ScreenSpaceOverlay);
        CanvasGroup = Canvas.GetComponent<CanvasGroup>();
        GraphicRaycaster = Canvas.GetComponent<GraphicRaycaster>();

        foreach (UIScreen screen in FindObjectsOfType<UIScreen>(true))
            Instance._screens.Add(screen.GetType(), screen);
    }

    public static TScreen GetScreen<TScreen>() where TScreen : UIScreen
    {
        if (Instance is null)
            throw new NullReferenceException($"Instance is null");

        if (Instance._screens.TryGetValue(typeof(TScreen), out UIScreen screen))
            return (TScreen)screen;

        return default;
    }

    public static void FadeAll(float alpha, float duration = 0.15f, params Type[] exclude)
    {
        foreach (UIScreen screen in _instance._screens.Values)
        {
            if (exclude.Contains(screen.GetType()))
                continue;

            screen.CanvasGroup.DOKill();

        
            screen.CanvasGroup.alpha = alpha;
        }
    }

    public static void SetActive(bool active, params ScreenTag[] tags)
    {
        foreach (UIScreen screen in _instance._screens.Values)
            if (screen.ScreenTags.Contains(t => tags.Contains(t)))
                screen.gameObject.SetActive(active);
    }
}
