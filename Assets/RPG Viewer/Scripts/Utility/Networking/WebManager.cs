﻿using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RPG;
using UnityEngine;

namespace Networking
{
    public class WebManager
    {
        public static async void Download(string _path, bool useCache, Action<byte[]> callback)
        {
            await UniTask.SwitchToMainThread();
            string path = $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}{_path}";
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
                    SocketManager.EmitAsync("download-image", async (callback1) =>
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
                    }, path);
                }
            }
            else
            {
                SocketManager.EmitAsync("download-image", (callback1) =>
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
                }, path);
            }
        }
        public static async void DownloadLocal(string _path, Action<byte[]> callback)
        {
            await UniTask.SwitchToMainThread();
            string path = $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}{_path}";
            if (File.Exists(path))
            {
                await File.ReadAllBytesAsync(path).ContinueWith((bytes) =>
                {
                    callback(bytes.Result);
                });
            }
            else callback(null);
        }
    }
}