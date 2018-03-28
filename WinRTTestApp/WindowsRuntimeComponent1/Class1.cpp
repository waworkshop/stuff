#include "pch.h"
#include <wrl.h>  
#include <robuffer.h>  
#include "Class1.h"
#include "MyBuffer.h"
using namespace Windows::Storage::Streams;
using namespace Microsoft::WRL;
using namespace WindowsRuntimeComponent1;
using namespace Platform;

typedef uint8 byte;

WAByteBuffer::WAByteBuffer()
{
    localbuffer = new byte[256] { '\0' };
    localbuffer[0] = 'H';
    localbuffer[1] = 'e';
    localbuffer[2] = 'l';
    localbuffer[3] = 'l';
    localbuffer[4] = 'o';
    localbuffer[5] = ' ';
    localbuffer[6] = 'C';
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

bool WAByteBuffer::MakeNativeBuffer()
{
    //auto var = new ArrayReference<byte>(localbuffer, 256);

    //Array<byte>^ array = reinterpret_cast<Array<byte>^>(var);


    //buffer = Windows::Security::Cryptography::CryptographicBuffer::CreateFromByteArray(array);


    ComPtr<MyBuffer> nativeBuffer;
    Microsoft::WRL::Details::MakeAndInitialize<MyBuffer>(&nativeBuffer, localbuffer, 256);
    auto iinspectable = reinterpret_cast<IInspectable *>(nativeBuffer.Get());
    buffer = reinterpret_cast<Windows::Storage::Streams::IBuffer ^>(iinspectable);

    return true;

    return true;
}

bool WAByteBuffer::ChangeTheNativeBufferConent()
{
    localbuffer[0] = 'B';
    localbuffer[1] = 'y';
    localbuffer[2] = 'e';
    localbuffer[3] = '!';
    localbuffer[4] = '\0';
    localbuffer[5] = '\0';
    localbuffer[6] = '\0';

    return true;
}
