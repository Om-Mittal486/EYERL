using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class BlinkReceiver : MonoBehaviour
{
    public BlinkEffect blink;
    public EnemyBehaviour enemy;
    // A flag to indicate a blink was detected
    public static bool wasBlinkDetected = false;

    // UDP listener thread
    private Thread receiveThread;
    private UdpClient client;
    public int port = 5005; // Must match the port in the Python script

    // Reference to the enemy behaviour script
    [Header("Enemy Reference")]
    public EnemyBehaviour enemyScript; // Drag the enemy GameObject here in inspector

    void Start()
    {
        // If no enemy script is assigned, try to find it automatically
        if (enemyScript == null)
        {
            enemyScript = FindObjectOfType<EnemyBehaviour>();

            if (enemyScript == null)
            {
                Debug.LogWarning("No enemy_behaviour script found! Please assign it in the inspector.");
            }
        }

        // Start the listening thread
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        Debug.Log("Started listening for blinks from Python.");
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);

                // If we receive "true", set the flag
                if (text == "true")
                {

                    wasBlinkDetected = true;
                }
            }
            catch (Exception err)
            {
                Debug.LogError(err.ToString());
            }
        }
    }

    void Update()
    {
        // In the main game loop, check if the flag was set
        if (wasBlinkDetected)
        {
            // Call tp_ghost function from enemy_behaviour script
            if (enemyScript != null)
            {
                blink.TriggerBlink();
                enemy.TriggerJumpscare();
                Debug.Log("Blink Detected! Called tp_ghost function.");
            }
            else
            {
                Debug.LogWarning("Cannot call tp_ghost - enemy_behaviour script not found!");
            }

            // Reset the flag so the action only happens once per blink
            wasBlinkDetected = false;
        }
    }

    // Clean up the thread and socket when the application quits
    void OnApplicationQuit()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        if (client != null)
        {
            client.Close();
        }
    }
}