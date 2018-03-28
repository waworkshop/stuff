#pragma once

#include <wrl.h>
#include <wrl/implements.h>
#include <windows.storage.streams.h>
#include <robuffer.h>
#include <vector>

namespace WindowsRuntimeComponent1
{
    class MyBuffer :
        public Microsoft::WRL::RuntimeClass<Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::RuntimeClassType::WinRtClassicComMix>,
        ABI::Windows::Storage::Streams::IBuffer,
        Windows::Storage::Streams::IBufferByteAccess>
        
    {
        InspectableClass(L"MyBuffer", TrustLevel::BaseTrust);

    public:
        virtual ~MyBuffer()
        {
        }

        STDMETHODIMP RuntimeClassInitialize(byte *buffer, UINT totalSize)
        {
            m_length = totalSize;
            m_buffer = buffer;

            return S_OK;
        }

        STDMETHODIMP Buffer(byte **value)
        {
            *value = m_buffer;

            return S_OK;
        }

        STDMETHODIMP get_Capacity(UINT32 *value)
        {
            *value = m_length;

            return S_OK;
        }

        STDMETHODIMP get_Length(UINT32 *value)
        {
            *value = m_length;

            return S_OK;
        }

        STDMETHODIMP put_Length(UINT32 value)
        {
            m_length = value;

            return S_OK;
        }

    private:
        UINT32 m_length;
        byte *m_buffer;
    };
}

