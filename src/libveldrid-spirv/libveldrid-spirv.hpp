#pragma once

#include "stdint.h"
#include "ShaderSetCompilationInfo.hpp"
#include <vector>

#ifdef _WIN32
#define VD_EXPORT extern "C" __declspec(dllexport)
#else
#define VD_EXPORT extern "C"
#endif

using namespace Veldrid;

ShaderCompilationResult* Compile(const ShaderSetCompilationInfo& info);

std::vector<uint32_t> ReadFile(std::string path);
