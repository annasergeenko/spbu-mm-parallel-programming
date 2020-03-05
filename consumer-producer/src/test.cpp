//
// Created by alexander on 05.03.2020.
//

#include <iostream>
#include <cassert>
#include <optional>
#include <deque>
#include <chrono>
#include <future>
#include <thread>

#include "Utils.h"
#include "Buffer.h"
#include "Producer.h"
#include "Consumer.h"


void testBuffer() {
  Buffer<int> Buf;

  // Test that we can safely pop from an empty buffer
  assert(Buf.pop() == std::nullopt && "Empty buffer test failed");

  // Test push & pop behaviour
  std::vector<int> PushData{1, 2, 3, 4, 5};
  for (const auto& Elem : PushData)
    Buf.push(Elem);
  for (const auto& Elem : PushData)
    assert(Elem == Buf.pop().value() && "Pushed and popped values are not equal");

  // Test that there are no elements in the buffer after all pop operations
  assert(Buf.pop() == std::nullopt && "Buffer is not empty after all pop operations");

  std::cout << "\n\nBuffer tests passed\n\n";
}


void testProducer() {
  Buffer<int> Buf;
  bool ShouldStop(false);

  std::vector<int> Reference{1, 2, 3, 4, 5};

  Producer<int> P(std::deque<int>{1, 2, 3, 4, 5}, Buf, ShouldStop,
      std::chrono::milliseconds(50));

  auto Result = std::async(std::launch::async,
                           &Producer<int>::start,
                           &P);

  // Wait for some time to let the Producer do his job
  std::this_thread::sleep_for(std::chrono::milliseconds(500));
  ShouldStop = true;

  assert(Result.get() == Reference &&
         "Reference data does not match to produced one");

  std::cout << "\n\nProducer test passed\n\n";
}


void testConsumer() {
  Buffer<int> Buf;
  bool ShouldStop(false);

  std::vector<int> Reference{1, 2, 3, 4, 5};
  for (const auto Elem : Reference)
    Buf.push(Elem);

  Consumer<int> C(Buf, ShouldStop, std::chrono::milliseconds(50));

  auto Result = std::async(std::launch::async,
                           &Consumer<int>::takeTasks,
                           &C);

  // Wait for some time to let the Producer do his job
  std::this_thread::sleep_for(std::chrono::milliseconds(500));
  ShouldStop = true;

  assert(Result.get() == Reference &&
         "Reference data does not match to produced one");

  std::cout << "\n\nConsumer test passed\n\n";
}


int main() {
  testBuffer();
  testProducer();
  testConsumer();

  return 0;
}