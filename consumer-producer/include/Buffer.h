//
// Created by alexander on 26.02.2020.
//

#ifndef CONSUMER_PRODUCER_BUFFER_H
#define CONSUMER_PRODUCER_BUFFER_H

#include <mutex>
#include <deque>
#include <condition_variable>
#include <optional>
#include <type_traits>


template <typename T>
class Buffer {
  /// Mutex for synchronizing access to the Tasks queue.
  std::mutex QueueLock;
  std::deque<T> Tasks;

public:
  std::condition_variable CV;

  template <typename UR = T,
            typename TypeMustBeT =
              std::enable_if<std::is_same<std::remove_reference<UR>, T>::value>>
  void push(UR&& Task) noexcept(
      std::is_nothrow_copy_constructible<T>::value &&\
      std::is_nothrow_move_constructible<T>::value
      ) {
    std::lock_guard<std::mutex> Lock(QueueLock);
    Tasks.push_back(std::forward<UR>(Task));
    /// If there is only one element in the queue after inserting,
    /// somebody waiting for tasks may want to be notified.
    if (Tasks.size() == 1) CV.notify_all();
  }

  std::optional<T> pop() {
    if (Tasks.empty())
      return std::nullopt;

    std::lock_guard<std::mutex> Lock(QueueLock);

    auto tmp = std::make_optional<T>(std::move(Tasks.front()));
    Tasks.pop_front();

    return tmp;
  }
};

#endif //CONSUMER_PRODUCER_BUFFER_H