using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] public string selectedTag = "Selectable";
    [SerializeField] public Material highlightMaterial;

    private Transform _selection;
    private Transform _oldSelection;
    private Material _selectionMaterial;
    private Material _oldSelectionMaterial;

    // Start is called before the first frame update
    void Start()
    {
        _selection = null;
        _oldSelection = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (_oldSelection != _selection && _oldSelection != null) {
            var oldSelectionRenderer = _oldSelection.GetComponentInChildren<Renderer>();
            if (oldSelectionRenderer != null && _oldSelectionMaterial != null) {
                oldSelectionRenderer.material = _oldSelectionMaterial;
                    Console.Write("material reset ("+_oldSelectionMaterial.ToString());Console.WriteLine();
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
                    Console.Write("ray hit object tagged="+rootSelection.tag+", name="+rootSelection.name);Console.WriteLine();
                    var selectionRenderer = rootSelection.GetComponentInChildren<Renderer>();
                    if (selectionRenderer != null) {
                        _selectionMaterial = selectionRenderer.material;
                        selectionRenderer.material = highlightMaterial;
                        Console.Write("material set");Console.WriteLine();
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
