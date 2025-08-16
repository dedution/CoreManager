#pragma once
#include "codefort.h"
#include <cstdint>

#ifdef __cplusplus
extern "C"
{
#endif

    __declspec(dllexport) CodeFort *CodeFort_Create();
    __declspec(dllexport) void CodeFort_Destroy(CodeFort *instance);
    __declspec(dllexport) const char* CodeFort_RequestSession(CodeFort *instance);
    __declspec(dllexport) bool CodeFort_RequestFileKey(CodeFort *instance, const char *fileID, uint8_t *outKey, int keySize);
    __declspec(dllexport) bool CodeFort_DecryptFile(CodeFort *instance, const char *filePath, const uint8_t *key, int keySize);

#ifdef __cplusplus
}
#endif
