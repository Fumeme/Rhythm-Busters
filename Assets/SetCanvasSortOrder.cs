using UnityEngine;

public class SetCanvasSortOrder : MonoBehaviour
{
    [SerializeField] int _newSortOrder = 10;
    Canvas _canvas;
    int _oldSortOrder;
    bool _oldOverrideSorting;

    private void Awake()
    {
        _canvas = FindAnyObjectByType<Canvas>();
        if (_canvas == null)
        {
            Debug.LogError("No Canvas found in the scene.");
            return;
        }
        SetOrder();
    }
    private void OnEnable()
    {
        SetOrder();


    }
    void SetOrder()
    {
       // _canvas = FindAnyObjectByType<Canvas>();
        _oldSortOrder = _canvas.sortingOrder;
        _canvas.sortingOrder = _newSortOrder;
        _oldOverrideSorting = _canvas.overrideSorting;
        _canvas.overrideSorting = true;
    }
    private void OnDisable()
    {
        _canvas.sortingOrder = _oldSortOrder;
        _canvas.overrideSorting = _oldOverrideSorting;
    }
}