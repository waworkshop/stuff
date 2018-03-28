#pragma once

namespace WindowsRuntimeComponent1
{
    public ref class WAByteBuffer sealed
    {
    private:
        Windows::Storage::Streams::IBuffer^ buffer;

    public:
        WAByteBuffer();
        bool Get(Windows::Storage::Streams::IBuffer^* intOutArray);
        bool PutWithCopy(Windows::Storage::Streams::IBuffer^ byteArray);
        bool Put(Windows::Storage::Streams::IBuffer^ byteArray);
        
        bool MakeNativeBuffer();
        bool ChangeTheBufferConent();
        bool ChangeTheNativeBufferConent();

    private:
        byte * GetPointerToData(Windows::Storage::Streams::IBuffer^ pixelBuffer, unsigned int *length);
        byte * localbuffer;
    };

    //public ref class WABuffer sealed : public Windows::Storage::Streams::IBuffer, public Windows::Storage::Streams::IBufferByteAccess
    //{
    //public:
    //    WAByteBuffer();

    //};
}
