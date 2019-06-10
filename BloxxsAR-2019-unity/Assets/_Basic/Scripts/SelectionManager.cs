using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum SelectionMode
{
    None = 0,
    Selected = 1,
    Zooming = 2,
    Clicking = 8,
}

public class SelectionManager : MonoBehaviour
{
    [SerializeField] public string selectedTag = "Selectable";
    [SerializeField] public Material highlightMaterial;

    private Transform _selection;
    private Transform _oldSelection;
    private Material _selectionMaterial;
    private Material _oldSelectionMaterial;


    private SelectionMode _selectionMode;

    // Start is called before the first frame update
    void Start()
    {
        _selection = null;
        _oldSelection = null;
        setModeNone();
    }

    public Transform getSelection() {
        return _selection;
    }
    public bool hasValidSelection() {
        return _selection != null;
    }
    public void destroySelection() {
        if (_selection != null) {
            Destroy(_selection.gameObject);
            _selection = null;
            _oldSelection = null;
            _selectionMaterial = null;
            _oldSelectionMaterial = null;
            setModeNone();
        }
    }


    private void setModeZoom() {
        _selectionMode = SelectionMode.Zooming;
    }    
    private void setModeSelected() {
        _selectionMode = SelectionMode.Selected;
    }
    private void setModeClick() {
        _selectionMode = SelectionMode.Clicking;
    }
    private void setModeNone() {
        _selectionMode = SelectionMode.None;
    }

    public bool hasValidClickTouch() {
        if (hasValidSelection() && _selectionMode != SelectionMode.Zooming) {
            return Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended;
        }
        return false;
    }

    public bool hasValidNewPlacementTouch() {
        return !hasValidSelection() && Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended;
    }
    /* return a pinch factor from -1.0 - 1.0 with max pinch magnitude of +-/300.0f */
    public Tuple<bool, float> getValidPinchToZoomFactor() {
        if (hasValidSelection()) {
            // from https://www.youtube.com/watch?v=srcIPtyWlMs
            if (Input.touchCount == 2) {
                setModeZoom();
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);
                Vector2 prevZeroPos = touchZero.position - touchZero.deltaPosition;
                Vector2 prevOnePos = touchOne.position - touchOne.deltaPosition;
                float prevTouchDeltaMagnitude = (prevZeroPos - prevOnePos).magnitude;
                float touchDeltaMagnitude = (touchZero.position - touchOne.position).magnitude;
                float deltaMagnitude = prevTouchDeltaMagnitude - touchDeltaMagnitude;
                //Console.Write("deltaMagnitude="+deltaMagnitude.ToString());Console.WriteLine();
                float magnitude = Math.Min(deltaMagnitude, 300.0f);
                magnitude = Math.Max(magnitude, -300.0f);
                if (Math.Abs(magnitude) <= 1.0f) {
                    return Tuple.Create(true, 0.0f);
                } else {
                    return Tuple.Create(true, magnitude / 300.0f);
                }
            }
            if (_selectionMode == SelectionMode.Zooming && Input.touchCount == 0) {
                setModeSelected();
            }
        }
        return Tuple.Create(false, 0.0f);
    }

    private bool setNewLocalScaleVector(float factor) {
        var newScale = this.getSelection().localScale + new Vector3(factor, factor, factor);
        if (newScale.z > 0) {
            this.getSelection().localScale = newScale;
            return true;
        }
        return false;
    }
    public bool hasValidPinchZooming() {
        var pinchFactor = this.getValidPinchToZoomFactor();
        if (pinchFactor.Item1) {
            float factor = -0.1F * Math.Sign(pinchFactor.Item2);
            return this.setNewLocalScaleVector(factor);
        }
        return false;
    }


    // Update is called once per frame
    void Update()
    {
        if (_oldSelection != _selection && _oldSelection != null) {
            var oldSelectionRenderer = _oldSelection.GetComponentInChildren<Renderer>();
            if (oldSelectionRenderer != null && _oldSelectionMaterial != null) {
                oldSelectionRenderer.material = _oldSelectionMaterial;
                //Console.Write("material reset ("+_oldSelectionMaterial.ToString());Console.WriteLine();
            }
            _oldSelectionMaterial = null;
            _oldSelection = null;
        }
        var screenPoint = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var screenRay = Camera.current.ScreenPointToRay(screenPoint);
        RaycastHit hit;

        _oldSelection = _selection;
        _oldSelectionMaterial = _selectionMaterial;
        if (Physics.Raycast(screenRay, out hit)) {
            var selection = hit.transform;
            var rootSelection = selection.root;
            if (rootSelection.CompareTag(selectedTag)) {
                if (rootSelection != _oldSelection) {
                    //Console.Write("ray hit object tagged="+rootSelection.tag+", name="+rootSelection.name);Console.WriteLine();
                    var selectionRenderer = rootSelection.GetComponentInChildren<Renderer>();
                    if (selectionRenderer != null) {
                        _selectionMaterial = selectionRenderer.material;
                        selectionRenderer.material = highlightMaterial;
                        //Console.Write("material set");Console.WriteLine();
                        _selection = rootSelection;
                    } else {
                        _selectionMaterial = null;
                        _selection = null;
                    }
                }
            } else {
                _selectionMaterial = null;
                _selection = null;
            }
        } else {
            _selectionMaterial = null;
            _selection = null;
        }
    }
}
