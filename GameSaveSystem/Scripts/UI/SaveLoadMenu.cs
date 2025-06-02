using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SaveLoadMenu : MonoBehaviour
{
    [Header("Save Testing UI")]
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private TMP_InputField _inputText;
    [Header("Saving Options")]
    [SerializeField] private bool _useAsync = true;
    [Header("Debugging")]
    [SerializeField] private bool _debugSlotInfo = true;
    [SerializeField] private bool _logInfo = false;
    [Header("Events")]
    public UnityEvent OnShowSlots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _saveButton.onClick.AddListener(OnSaveButtonClicked);
        _loadButton.onClick.AddListener(OnLoadButtonClicked);

        SaveSystem.OnPrepareForSave.AddListener(PrepForSave);
        SaveSystem.OnHandleLoad.AddListener(HandleNewData);
    }

    public void OnReShow()
    {
        _loadButton.Select();
    }

    private void Update()
    {
        if (_debugSlotInfo)
        {
            SlotInfo();
        }
    }

    private void OnSaveButtonClicked()
    {
        if (_useAsync)
        {
            SaveSystem.SaveAsyncNoAwait();
        }
        else
        {
            SaveSystem.SaveInstant();
        }
    }

    private void OnLoadButtonClicked()
    {
        OnShowSlots.Invoke();
    }

    private void SlotInfo()
    {
        var slotInfo = SaveSystem.GetAllSlotInfo();

        foreach (var (slot, time) in slotInfo)
        {
            if (time.HasValue)
            {
#if UNITY_EDITOR
                if (_logInfo)
                {
                    Debug.Log($"Slot {slot}: Saved at {time.Value.ToLocalTime()}");
                }
#endif
            }
            else
            {
#if UNITY_EDITOR
                if (_logInfo)
                {
                    Debug.Log($"Slot {slot}: [Empty]");
                }
#endif
            }
        }
    }

    private void PrepForSave()
    {
        SaveSystem.SaveDataHolder.TextValue = _inputText.text;
    }

    private void HandleNewData()
    {
        _inputText.text = SaveSystem.SaveDataHolder.TextValue;
    }
}