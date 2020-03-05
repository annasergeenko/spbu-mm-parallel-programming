## Overview
This project is a part of a parallel programming course.
It simulates classic consumer-producer problem with unbounded
buffer.

## Build
### Requirements
`CMake` minimum required version is `3.10`.

`C++17` compiler.

### Build (example)
Create and navigate to build directory
>  `mkdir $BUILD_DIR && cd $BUILD_DIR`

Run Cmake
> `cmake -G "Ninja" $PATH_TO_PROJECT`

Build binaries
> `ninja`

Binaries are in `$BUILD_DIR/bin` directory.

## Run
You now may run interactive console application `consumer_producer`
that requires user input, or just run simple unit tests
`test_consumer_producer` 