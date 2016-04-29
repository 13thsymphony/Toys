// UVCDisableAutofocus.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

int wmain(int argc, wchar_t* argv[])
{
    HRESULT hr = S_OK;

    IFRL(CoInitialize(nullptr));

    CComPtr<ICreateDevEnum> sysDeviceEnum;
    IFRL(CoCreateInstance(CLSID_SystemDeviceEnum, nullptr, CLSCTX_INPROC_SERVER,
        IID_ICreateDevEnum, (void **)&sysDeviceEnum));

    CComPtr<IEnumMoniker> enumFilters;
    hr = sysDeviceEnum->CreateClassEnumerator(CLSID_VideoInputDeviceCategory, &enumFilters, 0);
    if (hr != S_OK)
    {
        IFRL(E_FAIL); // CreateClassEnumerator returns S_FALSE on failure.
    }

    CComPtr<IMoniker> moniker;
    ULONG numFetched;
    while (enumFilters->Next(1, &moniker, &numFetched) == S_OK)
    {
        CComPtr<IPropertyBag> propBag;
        IFRL(moniker->BindToStorage(nullptr, nullptr, IID_IPropertyBag, (void **)&propBag));

        // Extract and display the friendly name of the filter.
        VARIANT variant;
        VariantInit(&variant);
        hr = propBag->Read(L"FriendlyName", &variant, nullptr);

        if (FAILED(hr))
        {
            VariantClear(&variant);
            IFRL(hr);
        }

        wprintf(L"Friendly name: %s\n", variant.bstrVal);


        if (0 == wcscmp(variant.bstrVal, L"Microsoft LifeCam HD-5000"))
        {
            CComPtr<IBaseFilter> filter;
            hr = moniker->BindToObject(nullptr, nullptr, IID_IBaseFilter, (void**)&filter);

            CComPtr<IAMCameraControl> cameraControl;
            HRESULT hr;
            IFRL(filter->QueryInterface(IID_IAMCameraControl, (void **)&cameraControl));

            long defaultFocus, minFocus, maxFocus, step, capFlags;
            IFRL(cameraControl->GetRange(
                CameraControl_Focus,
                &minFocus, // min
                &maxFocus, // max
                &step, // minstep
                &defaultFocus, // default
                &capFlags)); // capflags

            wprintf(L"Min focus: %d\n", minFocus);
            wprintf(L"Max focus: %d\n", maxFocus);
            wprintf(L"Step: %d\n", step);
            wprintf(L"Default focus: %d\n", defaultFocus);
            wprintf(L"Cap flags: %d\n", capFlags);

            IFRL(cameraControl->Set(
                CameraControl_Focus, // property
                defaultFocus, // value
                CameraControl_Flags_Manual));
        }

        VariantClear(&variant);
        moniker.Release();
    }

    // Why do I need to release in a certain order??
    enumFilters.Release();
    sysDeviceEnum.Release();

    CoUninitialize();
    return 0;
}

