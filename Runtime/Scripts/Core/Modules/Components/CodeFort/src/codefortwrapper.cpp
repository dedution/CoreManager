// CodeFortWrapper.cpp
#include "codefort.h"
#include <string>
#include <vector>

extern "C"
{
    static std::string g_lastSessionID; // keep string alive for C#

    // Create a new CodeFort instance and return a pointer
    __declspec(dllexport) CodeFort *CodeFort_Create()
    {
        return new CodeFort();
    }

    // Destroy a CodeFort instance
    __declspec(dllexport) void CodeFort_Destroy(CodeFort *instance)
    {
        delete instance;
    }

    // Request a session
    __declspec(dllexport) const char *CodeFort_RequestSession(CodeFort *instance)
    {
        if (!instance)
            return nullptr;
        if (instance->RequestSession())
        {
            g_lastSessionID = instance->RequestSession(); // store internally
            return g_lastSessionID.c_str();
        }
        return nullptr;
    }

    // Request a file key
    __declspec(dllexport) bool CodeFort_RequestFileKey(CodeFort *instance, const char *fileID, uint8_t *outKey, int keySize)
    {
        std::vector<uint8_t> key;
        bool result = instance->RequestFileKey(fileID, key);
        if (!result)
            return false;

        // Copy key to outKey buffer (up to keySize bytes)
        int copySize = (key.size() < keySize) ? key.size() : keySize;
        for (int i = 0; i < copySize; ++i)
        {
            outKey[i] = key[i];
        }
        return true;
    }

    // Decrypt file
    __declspec(dllexport) bool CodeFort_DecryptFile(CodeFort *instance, const char *filePath, const uint8_t *key, int keySize)
    {
        std::vector<uint8_t> keyVec(key, key + keySize);
        return instance->DecryptFile(filePath, keyVec);
    }
}
