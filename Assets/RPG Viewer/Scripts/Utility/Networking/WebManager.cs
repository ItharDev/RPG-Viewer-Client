using UnityEngine;
using System.IO;
using System;
using RPG;

namespace Networking
{
    public class WebManager
    {
        public static async void Download(string id, bool useCache, Action<byte[]> callback)
        {
            string path = $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}{id}";
            if (useCache)
            {
                if (File.Exists(path))
                {
                    await File.ReadAllBytesAsync(path).ContinueWith((bytes) => 
                    {
                        callback(bytes.Result);
                    });
                }
                else
                {
                    await SocketManager.Socket.EmitAsync("download-image", async (callback1) =>
                    {
                        if (callback1.GetValue().GetBoolean())
                        {
                            byte[] bytes = Convert.FromBase64String(callback1.GetValue(1).GetString());
                            await File.WriteAllBytesAsync(path, bytes);
                            callback(bytes);
                        }
                        else
                        {
                            MessageManager.QueueMessage(callback1.GetValue(1).GetString());
                            callback(null);
                        }
                    }, id);
                }
            }
            else
            {
                await SocketManager.Socket.EmitAsync("download-image", (callback1) =>
                {
                    if (callback1.GetValue().GetBoolean())
                    {
                        byte[] bytes = Convert.FromBase64String(callback1.GetValue(1).GetString());
                        callback(bytes);
                    }
                    else
                    {
                        MessageManager.QueueMessage(callback1.GetValue(1).GetString());
                        callback(null);
                    }
                }, id);
            }       
        }
    }
}