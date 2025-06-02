using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HKGameSave
{
    public class SlotsMenu : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button _backButton;
        [Header("Slots")]
        [SerializeField] private Transform _layout;
        [SerializeField] private GameObject _slotElement;
        [SerializeField] private GameObject _noSlotsInfoObj;
        [Header("Events")]
        public UnityEvent<int> OnShowSlotCPopup;
        public UnityEvent OnShowSaveLoadMenu;

        private List<SlotData> _slotDatas = new List<SlotData>();

        public class SlotData
        {
            public SlotElement Slot;
            public UnityAction SlotAction;

            public SlotData(SlotElement slot, UnityAction action)
            {
                Slot = slot;
                SlotAction = action;
            }
        }

        private void Start()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void OnBackButtonClicked()
        {
            DestroyAndClear();
            Hide();

            OnShowSaveLoadMenu.Invoke();
        }

        public void UpdateSlots()
        {
            if (SaveSystem.GetAllSlotInfo().Length > 0)
            {
                ShowSlots();
            }
            else
            {
                DestroyAndClear();

                _noSlotsInfoObj.SetActive(true);
                _backButton.Select();
            }
        }

        public void ShowSlots()
        {
            SetupSlots();
            Show();
        }

        private void SetupSlots()
        {
            DestroyAndClear();

            var slotInfos = SaveSystem.GetAllSlotInfo();

            bool selected = false;

            foreach (var slotInfo in slotInfos)
            {
                GameObject clonedSlotElement = Instantiate(_slotElement, _layout);
                SlotElement slotElement = clonedSlotElement.GetComponent<SlotElement>();

                slotElement.SetSlotElementData(slotInfo.slot, slotInfo.time.Value);

                UnityAction action = () => OnSlotClicked(slotInfo.slot);
                slotElement.SlotElementButton.onClick.AddListener(action);

                if (!selected)
                {
                    slotElement.SlotElementButton.Select();

                    selected = true;
                }

                _slotDatas.Add(new SlotData(slotElement, action));
            }

            if (slotInfos.Length > 0)
            {
                _noSlotsInfoObj.SetActive(false);
            }
            else
            {
                _noSlotsInfoObj.SetActive(true);
            }

            if (!selected)
            {
                _backButton.Select();
            }
        }

        private void OnSlotClicked(int slotNumber)
        {
            bool success = SaveSystem.LoadInstant(slotNumber);

            if (!success)
            {
                // Failed to load...
                OnShowSlotCPopup.Invoke(slotNumber);

                return;
            }

            Hide();
            DestroyAndClear();

            OnShowSaveLoadMenu.Invoke();
        }

        private void DestroyAndClear()
        {
            ResetButtonListeners();

            foreach (SlotData slotData in _slotDatas)
            {
                Destroy(slotData.Slot.gameObject);
            }

            _slotDatas.Clear();
        }

        private void OnDestroy()
        {
            ResetButtonListeners();
        }

        private void ResetButtonListeners()
        {
            for (int i = 0; i < _slotDatas.Count; i++)
            {
                if (_slotDatas.Count > i && _slotDatas[i] != null)
                {
                    _slotDatas[i].Slot.SlotElementButton.onClick.RemoveListener(_slotDatas[i].SlotAction);
                }
            }
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
}