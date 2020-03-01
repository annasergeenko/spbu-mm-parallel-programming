//
// Created by alexander on 29.02.2020.
//

#ifndef CONSUMER_PRODUCER_INCLUDE_CONSUMER_H
#define CONSUMER_PRODUCER_INCLUDE_CONSUMER_H

#include <mutex>
#include <thread>

#include "Utils.h"
#include "Buffer.h"

namespace {
unsigned ConsumerID = 0;
}


template <typename T>
class Consumer {
  const unsigned ID;
  Buffer<T>& BufferRef;
  const std::chrono::milliseconds CooldownTime;
  const bool& ShouldStop;
  /// Mutex to wait until condition variable is woken up
  std::mutex CVMutex;

public:
  Consumer(Buffer<T>& BufferRef,
           const std::chrono::milliseconds CooldownTime,
           const bool& ShouldStop)
    : ID(ConsumerID++), BufferRef(BufferRef),
      CooldownTime(CooldownTime), ShouldStop(ShouldStop) { }

  void takeTasks() {
    while (! ShouldStop) {
      if (auto Task = BufferRef.pop()) {
        SyncPrint("Consumer #", ID, ": successfully got task #", Task.value(),
                  " . Sleep for '", CooldownTime.count(), "' ms...\n");
        std::this_thread::sleep_for(CooldownTime);
        SyncPrint("Consumer #", ID, ": woken up\n");
      } else {
        SyncPrint("Consumer #", ID, ": did not find available task, waiting...\n");
        std::unique_lock<std::mutex> UniqueLock(CVMutex);
        BufferRef.CV.wait(UniqueLock);
        SyncPrint("Consumer #", ID, ": going to try getting",
                  " produced task one more time\n");
      }
    }
  }
};

#endif //CONSUMER_PRODUCER_INCLUDE_CONSUMER_H
