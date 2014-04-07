﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AudioGap;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace AudioGapClient
{
    class Network
    {
        static Codec codec = new Codec();
        private static IPEndPoint ip;
        static UdpClient udpClient = new UdpClient();
        public static void connect(IPEndPoint endpoint, MMDevice device)
        {
            ip = endpoint;
            WasapiLoopbackCapture waveIn = new WasapiLoopbackCapture(device);
            waveIn.DataAvailable += SendData;
            waveIn.StartRecording();
        }

        static byte[][] splitByteArray(byte[] array, int maxArrayLength)
        {
            int length = array.Length;
            int arrayCount = (int)Math.Ceiling((double)length / maxArrayLength);
            int lastArrayLength = length - (maxArrayLength * (arrayCount - 1));

            byte[][] ByteArray = new byte[arrayCount][];

            for (int i = 0; i <= arrayCount - 1; i++) //array count isn't 0 indexed, starts at 1 so account for that
            {
                byte[] newArray;
                if (i == arrayCount - 1) //array count isn't 0 indexed, starts at 1 so account for that
                {
                    newArray = new byte[lastArrayLength];
                }
                else
                {
                    newArray = new byte[maxArrayLength];
                }

                int offset = maxArrayLength * i;
                Buffer.BlockCopy(array, offset, newArray, 0, newArray.Count());
                ByteArray[i] = newArray;
            }
            return ByteArray;
        }

        static void SendData(object sender, WaveInEventArgs e)
        {
            byte[] encoded = codec.Encode(e.Buffer, 0, e.BytesRecorded);

            int sl = 12000;

            foreach (byte[] bytes in splitByteArray(encoded, sl))
            {
                udpClient.Send(bytes, bytes.Length, ip);
            }
        }
    }
}
