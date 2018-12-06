cmake_minimum_required (VERSION 3.0)

set(SHADERC_SKIP_TESTS ON)
set(SHADERC_SKIP_INSTALL ON)
add_subdirectory(ext/shaderc)

set(CMAKE_CXX_STANDARD 17)
include_directories(ext/SPIRV-Cross)
include_directories(ext/SPIRV-Cross/include)
include_directories(ext/shaderc/libshaderc/include/shaderc)

file(GLOB_RECURSE LIBVELDRID_SPIRV_SOURCES src/libveldrid-spirv/*.cpp src/libveldrid-spirv/*.hpp)
set(SPIRV_CROSS_SOURCES 
    ext/SPIRV-Cross/spirv_cpp.cpp
    ext/SPIRV-Cross/spirv_cpp.hpp
    ext/SPIRV-Cross/spirv_cross_util.cpp
    ext/SPIRV-Cross/spirv_cross_util.hpp
    ext/SPIRV-Cross/spirv_hlsl.cpp
    ext/SPIRV-Cross/spirv_hlsl.hpp
    ext/SPIRV-Cross/spirv_cfg.cpp
    ext/SPIRV-Cross/spirv_cfg.hpp
    ext/SPIRV-Cross/spirv_cross.cpp
    ext/SPIRV-Cross/spirv_cross.hpp
    ext/SPIRV-Cross/spirv_glsl.cpp
    ext/SPIRV-Cross/spirv_glsl.hpp
    ext/SPIRV-Cross/spirv_msl.cpp
    ext/SPIRV-Cross/spirv_msl.hpp
)

set(LIBSHADERC_SOURCES
    ext/shaderc/libshaderc/include/shaderc/shaderc.h)

if (IOS_PLATFORM STREQUAL "")
    set(LIBRARY_TYPE SHARED)
else()
    set(LIBRARY_TYPE STATIC)
endif()

project (veldrid-spirv)
add_library(veldrid-spirv ${LIBRARY_TYPE} ${LIBVELDRID_SPIRV_SOURCES} ${SPIRV_CROSS_SOURCES})
target_link_libraries(veldrid-spirv shaderc)
set_target_properties(veldrid-spirv PROPERTIES PREFIX "lib")

set_target_properties(veldrid-spirv PROPERTIES XCODE_ATTRIBUTE_ENABLE_BITCODE "NO")
