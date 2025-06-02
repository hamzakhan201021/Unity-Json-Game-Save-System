using UnityEngine;

public class SaveSystemCanvasManager : MonoBehaviour
{

    [Header("Menus")]
    [SerializeField] private SaveLoadMenu _saveLoadMenu;
    [SerializeField] private SlotsMenu _slotsMenu;
    [SerializeField] private SlotCorruptedPopup _slotCorruptedPopupMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupEvents();
        SetupDefaults();
    }

    private void SetupEvents()
    {
        _saveLoadMenu.OnShowSlots.AddListener(_slotsMenu.ShowSlots);
        _slotCorruptedPopupMenu.OnUpdateSlots.AddListener(_slotsMenu.UpdateSlots);
        _slotsMenu.OnShowSlotCPopup.AddListener(_slotCorruptedPopupMenu.ShowPopup);
        _slotsMenu.OnShowSaveLoadMenu.AddListener(_saveLoadMenu.OnReShow);
    }

    private void SetupDefaults()
    {
        _saveLoadMenu.gameObject.SetActive(true);
        _slotsMenu.gameObject.SetActive(false);
        _slotCorruptedPopupMenu.gameObject.SetActive(false);
    }
}