#pragma once

#ifdef _WIN32
#define VD_EXPORT extern "C" __declspec(dllexport)
#else
#define VD_EXPORT extern "C"
#endif
