using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;

public class GameboardGenerator : MonoBehaviour
{

    [SerializeField]
    private GameObject diamondStandard;

    [SerializeField]
    private GameObject diamondExtra;

    [SerializeField]
    private GameObject diamondExplosion;

    [SerializeField]
    private GameObject tree;

    [SerializeField]
    private Button startButton;

    private bool gameStarted = false;
    private ARPlane board;

    public void StartGame() {
        if (this.isReadyToStart()) {
            gameStarted = true;
            startButton.GetComponentInChildren<Text>().text = "Stop";
        } else if (this.isStarted()) {
            gameStarted = false;
            board = null;
            startButton.GetComponentInChildren<Text>().text = "Place";
        }
    }

    public bool isStarted() {
        return gameStarted;
    }
    public bool isStopped() {
        return !gameStarted;
    }
    public bool isReadyToStart() {
        return isStopped() && this.board != null;
    }

    public void setBoard(ARPlane board) {
        startButton.GetComponentInChildren<Text>().text = "Start";
        this.board = board;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
