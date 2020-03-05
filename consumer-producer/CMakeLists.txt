cmake_minimum_required(VERSION 3.10)
project(consumer-producer)

set(CMAKE_CXX_STANDARD 17)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -pthread")

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin")

include_directories(include)

add_executable(consumer_producer src/main.cpp include/Buffer.h
        include/Producer.h include/Consumer.h include/Utils.h)

add_executable(test_consumer_producer src/test.cpp include/Buffer.h
        include/Producer.h include/Consumer.h include/Utils.h)