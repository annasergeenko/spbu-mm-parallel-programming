#pragma once

#include <atomic>
#include <thread>
#include <vector>

#include "queue_on_list.h"

template <typename T>
class Producer {

public:
    using Data_vec = std::vector<T>;

    Producer(Data_vec data, std::chrono::seconds pause)
        : data_to_write{std::move(data)}, written_data{}, pause_period{pause}
    {};

    // should_stop is managed in an another thread, but not in this
    void writing_to_queue_until(Queue_on_list<T>& queue, const std::atomic<bool>& should_stop) {
        for (const auto& elem : data_to_write) {
            if (should_stop.load(std::memory_order_relaxed)) {
                return;
            }
            queue.enqueue(elem);
            written_data.push_back(elem);
            std::this_thread::sleep_for(pause_period);
        }
    }

    Data_vec get_written_data() const {
        return written_data;
    }

private:
    const Data_vec data_to_write;
    Data_vec written_data;
    const std::chrono::seconds pause_period;
};
