using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HKGameSave
{
    public class SlotElement : MonoBehaviour
    {
        [Header("Slot Element UI")]
        [SerializeField] private TMP_Text _slotNumberText;
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] public Button SlotElementButton;

        public void SetSlotElementData(int slotNumber, DateTime date)
        {
            _slotNumberText.text = "Slot " + slotNumber;
            _dateText.text = date.ToLocalTime().ToString();
        }
    }
}