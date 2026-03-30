using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PinManager : MonoBehaviour
{
    [Header("Drag Your Scene Pins Here")]
    [SerializeField] List<BowlingPin> pins = new List<BowlingPin>();

    [Header("Timing")]
    [SerializeField] float settleTime = 3f;

    void Start()
    {
        // Debug: Check if pins are assigned
        if (pins.Count == 0)
        {
            Debug.LogError("PinManager: NO PINS ASSIGNED! " +
                "Drag your pin objects into the Pins list in Inspector!");
        }
        else
        {
            Debug.Log($"PinManager: {pins.Count} pins ready");
            foreach (var pin in pins)
            {
                if (pin == null)
                    Debug.LogError("PinManager: One pin slot is EMPTY/NULL!");
                else
                    Debug.Log($"Pin ready: {pin.gameObject.name} at {pin.transform.position}");
            }
        }
    }

    public int CountAndMarkNewlyFallenPins()
    {
        if (pins.Count == 0)
        {
            Debug.LogError("NO PINS IN LIST! Returning 0");
            return 0;
        }

        int count = 0;
        foreach (var pin in pins)
        {
            if (pin != null && pin.CheckIfFallen() && !pin.HasBeenCounted)
            {
                pin.HasBeenCounted = true;
                count++;
                Debug.Log($"✅ Pin knocked: {pin.gameObject.name}");
            }
        }

        Debug.Log($"Total newly fallen: {count}/{pins.Count}");
        return count;
    }

    public bool AllPinsDown()
    {
        // CRITICAL: No pins = NOT a strike!
        if (pins.Count == 0)
        {
            Debug.LogError("AllPinsDown: No pins in list! Returning FALSE");
            return false;
        }

        int activeCount = 0;
        int fallenCount = 0;

        foreach (var pin in pins)
        {
            if (pin != null && pin.gameObject.activeSelf)
            {
                activeCount++;
                if (pin.CheckIfFallen())
                    fallenCount++;
            }
        }

        bool allDown = activeCount > 0 && fallenCount >= activeCount;
        Debug.Log($"AllPinsDown check: {fallenCount}/{activeCount} = {allDown}");
        return allDown;
    }

    public int CountFallenPins()
    {
        int count = 0;
        foreach (var pin in pins)
        {
            if (pin != null && pin.CheckIfFallen())
                count++;
        }
        return count;
    }

    public void ResetAllPins()
    {
        foreach (var pin in pins)
        {
            if (pin != null)
            {
                pin.gameObject.SetActive(true);
                pin.ResetPin();
            }
        }
        Debug.Log("All pins reset");
    }

    public void ClearFallenPins()
    {
        foreach (var pin in pins)
        {
            if (pin != null && pin.IsFallen)
                pin.gameObject.SetActive(false);
        }
    }

    public void ReactivateAllPins()
    {
        foreach (var pin in pins)
        {
            if (pin != null)
            {
                pin.gameObject.SetActive(true);
                pin.ResetPin();
            }
        }
    }
}