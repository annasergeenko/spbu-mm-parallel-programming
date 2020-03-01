//
// Created by alexander on 26.02.2020.
//

#ifndef CONSUMER_PRODUCER_INCLUDE_PRODUCER_H
#define CONSUMER_PRODUCER_INCLUDE_PRODUCER_H

#include <deque>
#include <chrono>
#include <thread>

#include "Utils.h"
#include "Buffer.h"

namespace {
unsigned ProducerID = 0;
}


template <typename T, typename Container = std::deque<T>>
class Producer {
  const unsigned ID;
  Container WorkElements;
  Buffer<T>& BufferRef;
  const std::chrono::milliseconds CooldownTime;
  const bool& ShouldStop;

public:
  Producer(
      Container&& WorkElements,
      Buffer<T>& BufferRef,
      const std::chrono::milliseconds CooldownTime,
      const bool& ShouldStop)
    : ID(ProducerID++), WorkElements(std::forward<Container>(WorkElements)),
      BufferRef(BufferRef), CooldownTime(CooldownTime),
      ShouldStop(ShouldStop) { }

  Producer(const Producer&) = delete;
  Producer& operator=(const Producer&) = delete;

  void start() {
    auto WorkElemIter = WorkElements.begin();

    if (WorkElemIter == WorkElements.end()) {
      SyncPrint("Nothing to do for producer ", ID, '\n');
      return;
    }

    /// loop over $WorkElements container until $ShouldStop is false
    while (! ShouldStop) {
      SyncPrint("Producer #", ID, ": push the task '",
                *WorkElemIter, "'.\n");
      BufferRef.push(*WorkElemIter);
      if (++WorkElemIter == WorkElements.end())
        WorkElemIter = WorkElements.begin();
      SyncPrint("Producer #", ID, ": sleep for '",
                CooldownTime.count(), "' ms...\n");
      std::this_thread::sleep_for(CooldownTime);
    }
  }
};

#endif //CONSUMER_PRODUCER_INCLUDE_PRODUCER_H
