//
// Created by alexander on 01.03.2020.
//

#ifndef CONSUMER_PRODUCER_INCLUDE_UTILS_H
#define CONSUMER_PRODUCER_INCLUDE_UTILS_H

#include <iostream>
#include <mutex>

namespace {
std::mutex CoutMutex;
}

template <typename ...Args>
void SyncPrint(Args&& ...args) {
  std::lock_guard<std::mutex> Lock(CoutMutex);
  ((std::cout << args), ...);
}

#endif //CONSUMER_PRODUCER_INCLUDE_UTILS_H
