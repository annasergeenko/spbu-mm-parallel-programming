cmake_minimum_required(VERSION 3.15)
project(MultiThreading)

set(CMAKE_CXX_STANDARD 17)

if ((CMAKE_CXX_COMPILER_ID STREQUAL "Clang") OR (CMAKE_CXX_COMPILER_ID STREQUAL "GNU"))
    set(CMAKE_CXX_FLAGS  "${CMAKE_CXX_FLAGS} -Wall -Wextra")
elseif (CMAKE_CXX_COMPILER_ID STREQUAL "MSVC")
    set(CMAKE_CXX_FLAGS  "${CMAKE_CXX_FLAGS} /W4")
endif()

find_package(Threads REQUIRED)

set(SRC_CODE queue_on_list.h producer.h consumer.h config_runner.h)

add_executable(MultiThreading main.cpp ${SRC_CODE})
target_link_libraries(MultiThreading Threads::Threads)

add_executable(RunTests tests.cpp ${SRC_CODE})
target_link_libraries(RunTests Threads::Threads)
