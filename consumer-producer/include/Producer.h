//
// Created by alexander on 26.02.2020.
//

#ifndef CONSUMER_PRODUCER_INCLUDE_PRODUCER_H
#define CONSUMER_PRODUCER_INCLUDE_PRODUCER_H

#include <deque>
#include <vector>
#include <chrono>
#include <thread>

#include "Utils.h"
#include "Buffer.h"

namespace {
unsigned ProducerID = 0;
constexpr auto DefaultProducerCooldown = std::chrono::milliseconds(100);
}


template <typename T, typename Container = std::deque<T>>
class Producer {
  const unsigned ID;
  Container WorkElements;
  Buffer<T>& BufferRef;
  std::vector<T> ProducedTasks;
  const std::chrono::milliseconds CooldownTime;
  volatile bool& ShouldStop;

public:
  Producer(
      Container&& WorkElements,
      Buffer<T>& BufferRef,
      volatile bool& ShouldStop,
      const std::chrono::milliseconds CooldownTime = DefaultProducerCooldown)
    : ID(ProducerID++), WorkElements(std::move(WorkElements)),
      BufferRef(BufferRef), ProducedTasks(),
      CooldownTime(CooldownTime), ShouldStop(ShouldStop) { }


  std::vector<T> start() {
    if (WorkElements.empty()) {
      SyncPrint("Nothing to do for producer ", ID, '\n');
      return {};
    }

    auto WorkElemIter = WorkElements.begin();

    while (! ShouldStop) {
      if (WorkElemIter == WorkElements.end()) {
        SyncPrint("Producer #", ID, ": produced all tasks, ",
                  "waiting for termination...\n");
        std::this_thread::sleep_for(DefaultProducerCooldown);
      } else {
        SyncPrint("Producer #", ID, ": push the task ", *WorkElemIter,
                  ". Now sleep for ", CooldownTime.count(), " ms...\n");
        BufferRef.push(*WorkElemIter);
        ProducedTasks.push_back(std::move(*WorkElemIter++));
        std::this_thread::sleep_for(CooldownTime);
      }
    }
    SyncPrint("Producer #", ID, ": terminated\n");
    return ProducedTasks;
  }
};

#endif //CONSUMER_PRODUCER_INCLUDE_PRODUCER_H
