#pragma once

namespace WindowsRuntimeComponent1
{
    public ref class WAByteBuffer sealed
    {
    private:
        Windows::Storage::Streams::IBuffer^ buffer;
//        Platform::Array<uint8> ^buffer;

    public:
        WAByteBuffer();
        bool Get(Windows::Storage::Streams::IBuffer^* intOutArray);
        bool PutWithCopy(Windows::Storage::Streams::IBuffer^ byteArray);
        bool Put(Windows::Storage::Streams::IBuffer^ byteArray);

        bool ChangeTheBufferConent();

    private:
        byte * GetPointerToData(Windows::Storage::Streams::IBuffer^ pixelBuffer, unsigned int *length);

    };
}
