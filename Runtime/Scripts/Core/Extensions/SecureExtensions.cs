using System.Collections;
using System.Collections.Generic;
using core.modules;
using UnityEngine;
using static core.GameManager;

namespace core.secure
{
    // Secure structs with tamper detection that work directly with CodeFort
    // Replaces ints, floats and strings
    // Only use them in strategic places

    [System.Serializable]
    public struct SecureInt
    {
        private int hiddenValue;
        private int randomKey;
        private int checksum;

        public SecureInt(int value)
        {
            randomKey = Random.Range(int.MinValue, int.MaxValue);
            hiddenValue = value ^ randomKey;
            checksum = value;
        }

        public int Value
        {
            get
            {
                int real = hiddenValue ^ randomKey;
                if (real != checksum)
                {
                    ActOnModule((CodeFortManager _ref) =>
                    {
                        _ref.EmitSurfaceAlert();
                    });

                    return 0;
                }
                return real;
            }
            set
            {
                randomKey = Random.Range(int.MinValue, int.MaxValue);
                hiddenValue = value ^ randomKey;
                checksum = value;
            }
        }

        public static implicit operator int(SecureInt s) => s.Value;
        public static implicit operator SecureInt(int v) => new SecureInt(v);

        public override string ToString() => Value.ToString();
    }


    [System.Serializable]
    public struct SecureFloat
    {
        private int hiddenValue;
        private int randomKey;
        private float checksum;

        public SecureFloat(float value)
        {
            randomKey = Random.Range(int.MinValue, int.MaxValue);
            int intValue = System.BitConverter.SingleToInt32Bits(value);
            hiddenValue = intValue ^ randomKey;
            checksum = value;
        }

        public float Value
        {
            get
            {
                int intValue = hiddenValue ^ randomKey;
                float real = System.BitConverter.Int32BitsToSingle(intValue);
                if (Mathf.Abs(real - checksum) > 0.0001f)
                {
                    ActOnModule((CodeFortManager _ref) =>
                    {
                        _ref.EmitSurfaceAlert();
                    });
                    
                    return 0f;
                }
                return real;
            }
            set
            {
                randomKey = Random.Range(int.MinValue, int.MaxValue);
                int intValue = System.BitConverter.SingleToInt32Bits(value);
                hiddenValue = intValue ^ randomKey;
                checksum = value;
            }
        }

        public static implicit operator float(SecureFloat s) => s.Value;
        public static implicit operator SecureFloat(float v) => new SecureFloat(v);

        public override string ToString() => Value.ToString();
    }


    [System.Serializable]
    public struct SecureString
    {
        private string hiddenValue;
        private char randomKey;
        private string checksum;

        public SecureString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                randomKey = (char)Random.Range(1, 65535);
                hiddenValue = "";
                checksum = "";
            }
            else
            {
                randomKey = (char)Random.Range(1, 65535);
                hiddenValue = Encrypt(value, randomKey);
                checksum = value;
            }
        }

        public string Value
        {
            get
            {
                string real = Decrypt(hiddenValue, randomKey);
                if (real != checksum)
                {
                    ActOnModule((CodeFortManager _ref) =>
                    {
                        _ref.EmitSurfaceAlert();
                    });

                    return string.Empty;
                }
                return real;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    hiddenValue = "";
                    checksum = "";
                    randomKey = (char)Random.Range(1, 65535);
                }
                else
                {
                    randomKey = (char)Random.Range(1, 65535);
                    hiddenValue = Encrypt(value, randomKey);
                    checksum = value;
                }
            }
        }

        private static string Encrypt(string input, char key)
        {
            char[] buffer = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                buffer[i] = (char)(input[i] ^ key);
            }
            return new string(buffer);
        }

        private static string Decrypt(string input, char key) => Encrypt(input, key);

        public static implicit operator string(SecureString s) => s.Value;
        public static implicit operator SecureString(string v) => new SecureString(v);

        public override string ToString() => Value;
    }

}