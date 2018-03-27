#include <windows.h>

#include "buffer.h"
#include "com.h"
#include "misc.h"
#include "ischeduler.h"
#include "variant.h"

typedef struct _BUFFER
{
   COMBASE Base;

   PVOID Payload;
   SIZE_T Length;
   SIZE_T Capacity;

   VOID (*Dtor)(PVOID, PVOID);
   PVOID DtorContext;
} BUFFER, *PBUFFER;

static HRESULT CALLBACK
Buffer_Reset(
   PVOID Buffer
);

static HRESULT CALLBACK
Buffer_Get(
   PVOID Thisp,
   PVOID *Output
)
{
   HRESULT hr = S_OK;
   PBUFFER This = Thisp;
   IPropertyValueStatics *Statics = NULL;

   HR_ASSIGN(hr, GetStatics(&Statics));
   if (SUCCEEDED(hr))
   {
      HR_ASSIGN(
         hr,
         (*Statics)->CreateUInt8Array(
            (PVOID)Statics,
            This->Length,
            This->Payload,
            (PVOID)Output
         )
      );
   }

   return hr;
}

static VOID
WinRtDtor(
   PVOID Context,
   PVOID Buffer
   )
{
   CoTaskMemFree(Buffer);
}

static HRESULT CALLBACK
Buffer_Put(
   PVOID Thisp,
   PVOID InputUnkp
)
{
   HRESULT hr = S_OK;
   UINT32 Length = 0;
   PBUFFER This = Thisp;
   IPropertyValue *Input = NULL;
   PBYTE RawBuffer = NULL;

   HR_ASSIGN(
      hr,
      ComQueryInterface(
         InputUnkp,
         &IID_IPropertyValue,
         &Input
      )
   );

   if (SUCCEEDED(hr))
   {
      HR_ASSIGN(hr, (*Input)->GetUInt8Array(Input, &Length, &RawBuffer));
   }

   if (SUCCEEDED(hr))
   {
      Buffer_Reset(This);

      This->Payload = RawBuffer;
      This->Length = Length;
      This->Dtor = WinRtDtor;
      This->DtorContext = NULL;
   }

   ComRelease(Input);

   return hr;
}

static VOID
ActionDtor(
   PVOID Actionp, PVOID Buffer
)
{
   PACTION_VTABLE *Action = Actionp;
   (*Action)->Perform(Action);
   ComRelease(Action);
}

static HRESULT CALLBACK
Buffer_PutZeroCopy(
   PVOID Thisp,
   PVOID Buffer,
   DWORD Length,
   PVOID DtorAction
)
{
   HRESULT hr = S_OK;
   PBUFFER This = Thisp;

   Buffer_Reset(This);
   This->Payload = Buffer;
   This->Length = Length;
   This->Dtor = ActionDtor;
   This->DtorContext = DtorAction;

   ComAddRef(DtorAction);

   return hr;
}

extern BUFFER_VTABLE
Buffer_VTable;

static HRESULT CALLBACK
Buffer_Reset(
   PVOID Bufferp
)
{
   PBUFFER Buffer = Bufferp;

   if (Buffer->Payload && Buffer->Dtor)
   {
      Buffer->Dtor(Buffer->DtorContext, Buffer->Payload);
   }

   Buffer->Payload = NULL;
   Buffer->Length = Buffer->Capacity = 0;
   Buffer->Dtor = NULL;
   Buffer->DtorContext = NULL;

   return S_OK;
}

static HRESULT CALLBACK
Buffer_GetPointer(PVOID Thisp, PVOID *Arg)
{
   HRESULT hr = S_OK;
   PBUFFER This = Thisp;
   *Arg = This->Payload;
   return hr;
}

static HRESULT CALLBACK
Buffer_GetLength(PVOID Thisp, PDWORD Length)
{
   HRESULT hr = S_OK;
   PBUFFER This = Thisp;

   *Length = This->Length;

   return hr;
}

DECLARE_CTOR(
   Buffer_New,
   BUFFER,
   L"ByteBuffer",
   MACRO_ARGS(&IID_IByteBuffer, NULL),
   MACRO_ARGS(0),
   MACRO_ARGS(&Buffer_VTable),
   MACRO_ARGS(TRUE),
   NULL,
   Buffer_Reset
)

VOID
Buffer_GetDtor(
   PVOID Bufferp,
   VOID (**Dtor)(PVOID, PVOID),
   PVOID *DtorContext
)
{
   PBUFFER Buffer = Bufferp;
   *Dtor = Buffer->Dtor;
   *DtorContext = Buffer->DtorContext;
}

VOID
Buffer_SetDtor(
   PVOID Bufferp,
   VOID (*Dtor)(PVOID, PVOID),
   PVOID DtorContext
)
{
   PBUFFER Buffer = Bufferp;
   Buffer->Dtor = Dtor;
   Buffer->DtorContext = DtorContext;
}

HRESULT
Buffer_SetContents(
   PVOID Bufferp,
   PVOID Payload,
   SIZE_T Length,
   VOID (*Dtor)(PVOID, PVOID),
   PVOID DtorContext
)
{
   HRESULT hr = S_OK;
   PBUFFER Buffer = Bufferp;

   Buffer_Reset(Buffer);

   Buffer->Payload = Payload;
   Buffer->Length = Buffer->Capacity = Length;
   Buffer->Dtor = Dtor;
   Buffer->DtorContext = DtorContext;

   return hr;
}

HRESULT
Buffer_GetContents(
   PVOID Bufferp,
   PVOID *ReturnedBuffer,
   SIZE_T *ReturnedLength,
   VOID (**FreeFunction)(PVOID)
)
{
   HRESULT hr = S_OK;
   PBUFFER_VTABLE *VTable = Bufferp;

   if (!Bufferp || !ReturnedBuffer || !ReturnedLength || !FreeFunction)
   {
      HR_RETURN(E_POINTER);
   }

   // Is this our own implementation of the COM interface?
   //
   if ((*VTable)->Get == Buffer_Get && (*VTable)->Put == Buffer_Put)
   {
      PBUFFER Buffer = Bufferp;
      
      // Buffer object owns object lifetime.
      //
      *FreeFunction = NULL;

      *ReturnedBuffer = Buffer->Payload;
      *ReturnedLength = Buffer->Length;
   }
   else
   {
      // Caller has implemented IBuffer on its own.
      // TODO - consider making a copy.
      //
      HR_ASSIGN(hr, E_INVALIDARG);

      // In the future this would be a heap allocation
      //
      *ReturnedBuffer = NULL;
      *ReturnedLength = 0;
      *FreeFunction = NULL;
   }

   return hr;
}

static HRESULT CALLBACK
Buffer_CopyFrom(
   PVOID Thisp,
   PVOID OtherBuffer
)
{
   HRESULT hr = S_OK;
   PBUFFER This = Thisp;
   PVOID Buffer = NULL;
   SIZE_T Length = 0;
   VOID (*FreeFunc)(PVOID) = NULL;

   if (!This->Payload)
      HR_RETURN(E_INVALIDARG);

   HR_ASSIGN(
      hr,
      Buffer_GetContents(
         OtherBuffer,
         &Buffer,
         &Length,
         &FreeFunc
      )
   );

   if (SUCCEEDED(hr))
   {
      if (Length != This->Length)
         HR_ASSIGN(hr, E_INVALIDARG);
      else
      {
         memcpy(This->Payload, Buffer, Length);
      }
   }

   if (FreeFunc)
      FreeFunc(Buffer);
   return hr;
}

VOID
Buffer_Default_Dtor(
   PVOID Context,
   PVOID OnHeap
)
{
   free(OnHeap);
}

VOID
Buffer_IAction_Dtor(
   PVOID Context,
   PVOID OnHeap
)
{
   if (OnHeap)
   {
      PACTION_VTABLE *Action = Context;
      (*Action)->Perform(Action);
      ComRelease(Action);
   }
}

BUFFER_VTABLE Buffer_VTable =
{
   COMBASE_INIT,
   Buffer_Get,
   Buffer_Put,
   Buffer_Reset,
   Buffer_PutZeroCopy,
   Buffer_GetLength,
   Buffer_CopyFrom,
   Buffer_GetPointer,
};

HRESULT
Buffer_NewWithContents(
   PVOID Buffer,
   SIZE_T Length,
   VOID (*Dtor)(PVOID, PVOID),
   PVOID DtorContext,
   PVOID *ReturnedObject
)
{
   PVOID Object = NULL;
   HRESULT hr = S_OK;

   HR_ASSIGN(hr, Buffer_New(&Object));
   if (SUCCEEDED(hr))
   {
      HR_ASSIGN(
         hr,
         Buffer_SetContents(Object, Buffer, Length, Dtor, DtorContext)
      );
   }

   if (SUCCEEDED(hr))
   {
      *ReturnedObject = Object;
      Object = NULL;
   }

   ComRelease(Object);
   return hr;
}
