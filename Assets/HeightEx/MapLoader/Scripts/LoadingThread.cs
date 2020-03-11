using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//任务队列
public class LoadingThread : MonoBehaviour
{
    public static LoadingThread current { get; private set; }

    public static List<Action> commands = new List<Action>();

    public List<Action> localCommands = new List<Action>();

    public AutoResetEvent resetEvent;

    private Thread _thread;

    private bool isRunning;

    // Start is called before the first frame update
    void Awake()
    {
        if (current != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);
        current = this;
        isRunning = true;
        resetEvent = new AutoResetEvent(false);
        _thread = new Thread(Run);
        _thread.Start();
    }

    private void Run()
    {
        while (isRunning)
        {
            Thread.Sleep(10);
            lock (commands)
            {
                localCommands.AddRange(commands);
                commands.Clear();
            }

            foreach (var action in localCommands)
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                   Debug.Log(e);
                }
            }
            
            localCommands.Clear();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}

