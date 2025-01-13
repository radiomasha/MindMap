using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class ActivateInput : MonoBehaviour
{
    [SerializeField] private GameObject _icons;
    [SerializeField] private GameObject _input;
    private bool isActivated;
    [SerializeField] private PokeInteractable _pokeInteractable;
    private PokeInteractable _current;
    

    void Start()
    {
        isActivated = false;
        _current = _pokeInteractable;
    }

    void Update()
    {
        if (_pokeInteractable.State == InteractableState.Select && !isActivated)
        {
           
            _input.SetActive(true);
            _icons.SetActive(false);
            isActivated = true;
        }
        isActivated = false;
        _current = null;
    }
}
