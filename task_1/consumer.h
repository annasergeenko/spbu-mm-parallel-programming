#pragma once

#include <atomic>
#include <thread>
#include <vector>

#include "queue_on_list.h"

template <typename T>
class Consumer {

public:
    using Data_vec = std::vector<T>;

    explicit Consumer(std::chrono::seconds pause)
        : read_data{}, pause_period{pause}
    {};

    // should_stop is managed in another thread, but not in this
    void reading_from_queue_until(Queue_on_list<T>& queue, const std::atomic<bool>& should_stop) {
        while (!should_stop.load(std::memory_order_relaxed)) {
            if (auto opt_res = queue.dequeue(); opt_res.has_value()) {
                read_data.push_back(*opt_res);
                std::this_thread::sleep_for(pause_period);
            }
        }
    }

    Data_vec get_read_data() const {
        return read_data;
    }

private:
    Data_vec read_data;
    const std::chrono::seconds pause_period;
};
