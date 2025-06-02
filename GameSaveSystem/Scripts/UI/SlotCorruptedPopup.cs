using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SlotCorruptedPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button _delete;
    [SerializeField] private Button _selectAnother;
    [Header("Events")]
    public UnityEvent OnUpdateSlots;

    private UnityAction _deleteAction;
    private bool _deleteSub = false;

    public void ShowPopup(int slotNum)
    {
        Show();
        RemoveDeleteButtonSub();

        _deleteAction = () => OnDeleteClicked(slotNum);
        _delete.onClick.AddListener(_deleteAction);
        _selectAnother.onClick.AddListener(OnSelectAnotherClicked);

        _deleteSub = true;
        _delete.Select();
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        _selectAnother.onClick.RemoveListener(OnSelectAnotherClicked);

        RemoveDeleteButtonSub();
    }

    private void RemoveDeleteButtonSub()
    {
        if (_deleteSub)
        {
            _delete.onClick.RemoveListener(_deleteAction);
        }
    }

    private void OnDeleteClicked(int slotNumber)
    {
        SaveSystem.DeleteSlot(slotNumber);

        Hide();

        OnUpdateSlots.Invoke();
    }

    private void OnSelectAnotherClicked()
    {
        Hide();

        OnUpdateSlots.Invoke();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}