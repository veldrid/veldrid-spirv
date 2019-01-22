cmake_minimum_required (VERSION 3.0)

set(CMAKE_POSITION_INDEPENDENT_CODE ON)

add_subdirectory(ext/SPIRV-Cross)

set(SHADERC_SKIP_TESTS ON)
set(SHADERC_SKIP_INSTALL ON)
add_subdirectory(ext/shaderc)

set(CMAKE_CXX_STANDARD 17)
include_directories(ext/SPIRV-Cross)
include_directories(ext/SPIRV-Cross/include)
include_directories(ext/shaderc/libshaderc/include/shaderc)

file(GLOB_RECURSE LIBVELDRID_SPIRV_SOURCES src/libveldrid-spirv/*.cpp src/libveldrid-spirv/*.hpp)

if (DEFINED IOS_PLATFORM)
    set(LIBRARY_TYPE STATIC)
    message( "Configured to build a static library." )
else()
    set(LIBRARY_TYPE SHARED)
    message( "Configured to build a shared library." )
endif()


project (veldrid-spirv)
add_library(veldrid-spirv ${LIBRARY_TYPE} ${LIBVELDRID_SPIRV_SOURCES})
target_link_libraries(veldrid-spirv
    spirv-cross-core
    spirv-cross-glsl
    spirv-cross-reflect
    spirv-cross-msl
    spirv-cross-hlsl
    shaderc
)
set_target_properties(veldrid-spirv PROPERTIES PREFIX "lib")
set_target_properties(veldrid-spirv PROPERTIES DEBUG_POSTFIX "")

set_target_properties(veldrid-spirv PROPERTIES XCODE_ATTRIBUTE_ENABLE_BITCODE "NO")
