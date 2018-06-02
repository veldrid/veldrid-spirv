#pragma once

#include "stdint.h"
#include "ShaderSetCompilationInfo.hpp"
#include <vector>

using namespace Veldrid;

ShaderCompilationResult* Compile(const ShaderSetCompilationInfo& info);

void FreeResult(ShaderCompilationResult* result);

std::vector<uint32_t> ReadFile(std::string path);
