#pragma once
#include <string>
#include <vector>
#include <cstdint>

#ifdef _WIN32
#define DRM_API __declspec(dllexport)
#else
#define DRM_API
#endif

class DRM_API CodeFort
{
public:
    CodeFort();
    ~CodeFort();

    // Requests a new session from the server
    const char* RequestSession();

    // Requests a decryption key for a specific file
    bool RequestFileKey(const std::string &fileID, std::vector<uint8_t> &outKey);

    // Decrypt a file using the provided key
    bool DecryptFile(const std::string &filePath, const std::vector<uint8_t> &key);

private:
    std::string sessionID;
    std::string sessionSignature;
};
