cmake_minimum_required(VERSION 3.15)
project(CodeFort)

set(CMAKE_CXX_STANDARD 17)

# Include folder
include_directories(include)

# Add both the class and the wrapper
add_library(CodeFort SHARED 
    src/codefort.cpp
    src/CodeFortWrapper.cpp
)
