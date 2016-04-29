// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#include <stdio.h>
#include <tchar.h>
#include <Windows.h>
#include <dshow.h>
#include <atlbase.h> // For CComPtr

// If failed, return hr and log to stderr. Set a breakpoint here to debug failures.
#define IFRL(x) if(FAILED(hr = x)) \
    { \
        fwprintf(stderr, L"\nError HRESULT: 0x%X\nFile: %s\nLine: %d", hr, __FILEW__, __LINE__); \
        return hr; \
    }