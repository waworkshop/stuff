#include "pch.h"
#include <wrl.h>  
#include <robuffer.h>  
#include "Class1.h"
using namespace Windows::Storage::Streams;
using namespace Microsoft::WRL;
using namespace WindowsRuntimeComponent1;
using namespace Platform;

typedef uint8 byte;

WAByteBuffer::WAByteBuffer()
{
}

bool WAByteBuffer::Get(Windows::Storage::Streams::IBuffer^* intOutArray)
{
    *intOutArray = buffer;
    return true;
}

bool WAByteBuffer::PutWithCopy(Windows::Storage::Streams::IBuffer^ byteArray)
{
    auto array = ref new Platform::Array<byte>(byteArray->Length);
    unsigned int length = 0;

    auto bytes = GetPointerToData(byteArray, &length);


    // Copy the contents.
    for (unsigned int i = 0; i < byteArray->Length; i++)
    {
        array[i] = bytes[i];
    }
    
    buffer = Windows::Security::Cryptography::CryptographicBuffer::CreateFromByteArray(array);

    return true;
}


bool WAByteBuffer::Put(IBuffer^ byteArray)
{
    buffer = byteArray;
    return true;
}

bool WAByteBuffer::ChangeTheBufferConent()
{
    bool returnValue = false;

    if (buffer != nullptr)
    {
        unsigned int length = 0;

        auto bytes = GetPointerToData(buffer, &length);

        length -= 1;

        // Copy the contents.
        for (unsigned int i = 0; i < length / 2; i++)
        {
            byte store = bytes[i];
            bytes[i] = bytes[length - i];
            bytes[length - i] = store;
        }

        returnValue = true;
    }

    return returnValue;
}

// Retrieves the raw data from the provided IBuffer object.  
// Warning: The lifetime of the returned buffer is controlled by  
// the lifetime of the buffer object that's passed to this method.  
// When the buffer has been released, the pointer becomes invalid  
// and must not be used.  
byte* WAByteBuffer::GetPointerToData(IBuffer^ pixelBuffer, unsigned int *length)
{
    if (length != nullptr)
    {
        *length = pixelBuffer->Length;
    }
    // Query the IBufferByteAccess interface.  
    ComPtr<IBufferByteAccess> bufferByteAccess;
    reinterpret_cast<IInspectable*>(pixelBuffer)->QueryInterface(IID_PPV_ARGS(&bufferByteAccess));

    // Retrieve the buffer data.  
    byte* data = nullptr;
    bufferByteAccess->Buffer(&data);
    return data;
}

