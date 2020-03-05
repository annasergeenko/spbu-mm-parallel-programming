//
// Created by alexander on 01.03.2020.
//

#ifndef CONSUMER_PRODUCER_INCLUDE_UTILS_H
#define CONSUMER_PRODUCER_INCLUDE_UTILS_H

#include <iostream>
#include <deque>
#include <mutex>
#include <random>
#include <future>
#include <vector>

namespace {
std::mutex CoutMutex;
}


template <typename T = unsigned >
using AsyncReturnType = std::vector<std::future<std::vector<T>>>;


template <typename ...Args>
void SyncPrint(Args&& ...args) {
  std::lock_guard<std::mutex> Lock(CoutMutex);
  ((std::cout << args), ...);
}


/// DataType must be a type supported by "uniform_int_distribution".
/// The effect is undefined if it's not true.
template <typename DataType = int,
          typename Container = std::deque<DataType>>
Container generateRandomIntData(unsigned count) {
  Container Data;
  std::random_device RD;
  std::mt19937 MT(RD());
  std::uniform_int_distribution<DataType> Distribution(count);
  for (auto i = 0; i < count; ++i)
    Data.push_back(Distribution(MT));
  return Data;
}

#endif //CONSUMER_PRODUCER_INCLUDE_UTILS_H
