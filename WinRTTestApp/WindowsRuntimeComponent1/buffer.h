#ifndef buffer_h
#define buffer_h

#include "unknown.h"
#include "misc.h"

#if defined(__cplusplus)
extern "C" {
#endif

// 9eb98dc6-afda-460c-9949-d17135c07612
DEFINE_GUID_OBS(
   CLSID_ByteBuffer,
   0x9eb98dc6L,0xafda,0x460c,0x99,0x49,0xd1,0x71,0x35,0xc0,0x76,0x12
);

// 701e8e7d-7df6-4db2-8d00-ee1703c526f9
DEFINE_GUID_OBS(
   IID_IByteBuffer,
   0x701e8e7dL,0x7df6,0x4db2,0x8d,0x00,0xee,0x17,0x03,0xc5,0x26,0xf9
);

typedef struct _BUFFER_VTABLE
{
   BASE_VTABLE Base;
   HRESULT (CALLBACK *Get)(PVOID Thisp, PVOID *Output);
   HRESULT (CALLBACK *Put)(PVOID Thisp, PVOID Input);
   HRESULT (CALLBACK *Reset)(PVOID Thisp);
   HRESULT (CALLBACK *PutZeroCopy)(PVOID Thisp, PVOID Ptr, DWORD Length, PVOID DtorAction);
   HRESULT (CALLBACK *GetLength)(PVOID Thisp, PDWORD Length);
   HRESULT (CALLBACK *CopyFrom)(PVOID Thisp, PVOID OtherBuffer);
   HRESULT (CALLBACK *GetPointer)(PVOID Thisp, PVOID *);
} BUFFER_VTABLE, *PBUFFER_VTABLE;

HRESULT
Buffer_New(
   PVOID *ReturnedObject
);

HRESULT
Buffer_SetContents(
   PVOID BufferObject,
   PVOID Buffer,
   SIZE_T Length,
   VOID (*Dtor)(PVOID, PVOID),
   PVOID DtorContext
);

HRESULT
Buffer_NewWithContents(
   PVOID Buffer,
   SIZE_T Length,
   VOID (*Dtor)(PVOID, PVOID),
   PVOID DtorContext,
   PVOID *ReturnedObject
);

HRESULT
Buffer_GetContents(
   PVOID BufferObject,
   PVOID *ReturnedBuffer,
   SIZE_T *ReturnedLength,
   VOID (**FreeFunction)(PVOID)
);

VOID
Buffer_GetDtor(
   PVOID Bufferp,
   VOID (**Dtor)(PVOID, PVOID),
   PVOID *DtorContext
);

VOID
Buffer_SetDtor(
   PVOID Bufferp,
   VOID (*Dtor)(PVOID, PVOID),
   PVOID DtorContext
);

VOID
Buffer_Default_Dtor(PVOID, PVOID);

VOID
Buffer_IAction_Dtor(PVOID, PVOID);

#if defined(__cplusplus)
}
#endif
#endif
