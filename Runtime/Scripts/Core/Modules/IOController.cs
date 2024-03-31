using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

namespace core
{
    public class IOController
    {
        public static void ReadDataFromFile(string _FilePath, bool isAsync = false, Action<string> _callBackSuccess = null, Action _callBackFailure = null)
        {
            if (!File.Exists(_FilePath))
            {
                if (_callBackFailure != null)
                    _callBackFailure.Invoke();

                return;
            }

            // On Main thread
            if (!isAsync)
            {
                StreamReader reader = new StreamReader(_FilePath);
                string _data = reader.ReadToEnd();
                reader.Close();

                if (_callBackSuccess != null)
                    _callBackSuccess.Invoke(_data);
            }
            // Async task
            else
                ReadTextAsync(_FilePath, _callBackSuccess);
        }

        public static void ReadDataFromFile<T>(string _FilePath, bool isAsync = false, Action<T> _callBackSuccess = null, Action _callBackFailure = null)
        {
            if (isAsync)
            {
                ReadDataAsync<T>(_FilePath, _callBackSuccess, _callBackFailure);
                return;
            }

            FileStream file;

            if (!File.Exists(_FilePath))
            {
                if (_callBackFailure != null)
                    _callBackFailure.Invoke();

                return;
            }
            else
                file = File.OpenRead(_FilePath);

            BinaryFormatter bf = new BinaryFormatter();
            T _data = (T)bf.Deserialize(file);
            file.Close();

            if (_callBackSuccess != null)
                _callBackSuccess.Invoke(_data);
        }

        #region Async Methods
        private static async Task ReadTextAsync(string _FilePath, Action<string> _callBackSuccess = null)
        {
            StreamReader reader = new StreamReader(_FilePath);
            string _data = await reader.ReadToEndAsync();
            reader.Close();

            if (_callBackSuccess != null)
                _callBackSuccess.Invoke(_data);
        }

        private static async Task ReadDataAsync<T>(string _FilePath, Action<T> _callBackSuccess = null, Action _callBackFailure = null)
        {
            if (!File.Exists(_FilePath))
            {
                if (_callBackFailure != null)
                    _callBackFailure.Invoke();
            }
            else
            {
                byte[] result;

                using (FileStream SourceStream = File.Open(_FilePath, FileMode.Open))
                {
                    result = new byte[SourceStream.Length];
                    await SourceStream.ReadAsync(result, 0, (int)SourceStream.Length);
                }

                BinaryFormatter bf = new BinaryFormatter();
                T _data;

                using (MemoryStream ms = new MemoryStream(result))
                {
                    _data = (T)bf.Deserialize(ms);
                }

                if (_callBackSuccess != null)
                    _callBackSuccess.Invoke(_data);
            }
        }

        private static async Task WriteTextAsync(string _FilePath, string _data, Action _callBackSuccess = null)
        {
            StreamWriter writer = new StreamWriter(_FilePath, false);
            await writer.WriteAsync(_data);
            writer.Close();

            if (_callBackSuccess != null)
                _callBackSuccess.Invoke();
        }

        private static async Task WriteBinaryAsync<T>(string _FilePath, T _data, bool canOverwrite = true, Action _callBackSuccess = null, Action _callBackFailure = null)
        {
            if (File.Exists(_FilePath))
            {
                if (!canOverwrite && _callBackFailure != null)
                {
                    _callBackFailure.Invoke();
                    return;
                }
            }

            byte[] result;
            BinaryFormatter bf = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, _data);
                result = ms.ToArray();
            }

            using (FileStream fs = new FileStream(_FilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await fs.WriteAsync(result, 0, result.Length);
            }

            if (_callBackSuccess != null)
                _callBackSuccess.Invoke();
        }

        #endregion

        public static void WriteDataToFile(string _FilePath, string _data, bool canOverwrite = true, bool isAsync = false, Action _callBackSuccess = null, Action _callBackFailure = null)
        {
            if (File.Exists(_FilePath) && !canOverwrite)
            {
                if (_callBackFailure != null)
                    _callBackFailure.Invoke();

                return;
            }

            if (!isAsync)
            {
                StreamWriter writer = new StreamWriter(_FilePath, false);
                writer.Write(_data);
                writer.Close();

                if (_callBackSuccess != null)
                    _callBackSuccess.Invoke();
            }
            else
                WriteTextAsync(_FilePath, _data, _callBackSuccess);
        }

        public static void WriteDataToFile<T>(string _FilePath, T _data, bool canOverwrite = true, bool isAsync = false, Action _callBackSuccess = null, Action _callBackFailure = null)
        {
            if (isAsync)
            {
                WriteBinaryAsync<T>(_FilePath, _data, canOverwrite, _callBackSuccess, _callBackFailure);
                return;
            }

            FileStream file;

            if (File.Exists(_FilePath))
            {
                if (canOverwrite)
                    file = File.OpenRead(_FilePath);
                else
                {
                    if (_callBackFailure != null)
                        _callBackFailure.Invoke();

                    return;
                }
            }
            else
                file = File.Create(_FilePath);

            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, _data);
            file.Close();

            if (_callBackSuccess != null)
                _callBackSuccess.Invoke();
        }

        public static void ReadJSONFromFile<T>(string _FilePath, bool isAsync, Action<T> _callBackSuccess = null, Action _callBackFailure = null)
        {
            ReadDataFromFile(_FilePath, isAsync, (string _data) =>
            {
                if (_callBackSuccess != null)
                    _callBackSuccess.Invoke(JsonUtility.FromJson<T>(_data));
            }, _callBackFailure);
        }

        public static void WriteJSONToFile<T>(string _FilePath, T _data, bool canOverwrite = true, bool isAsync = false, Action _callBackSuccess = null, Action _callBackFailure = null)
        {
            string _dataParsed = JsonUtility.ToJson(_data);

            WriteDataToFile(_FilePath, _dataParsed, canOverwrite, isAsync, () =>
            {
                if (_callBackSuccess != null)
                    _callBackSuccess.Invoke();

            }, _callBackFailure);
        }

        // Convert an object to a byte array
        private static byte[] ObjectToByteArray<T>(T obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        // Convert a byte array to an Object
        private static T ByteArrayToObject<T>(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            T obj = (T)binForm.Deserialize(memStream);

            return obj;
        }

        public static string DataToBinaryString<T>(T _originalData)
        {
            byte[] _dataBytes = IOController.ObjectToByteArray(_originalData);
            string _dataParse = BitConverter.ToString(_dataBytes);

            return _dataParse;
        }

        public static T BinaryStringToData<T>(string _byteStringData)
        {
            byte[] data = Array.ConvertAll<string, byte>(_byteStringData.Split('-'), s => Convert.ToByte(s, 16));
            return IOController.ByteArrayToObject<T>(data);
        }
    }
}