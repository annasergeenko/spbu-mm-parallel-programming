#pragma once

#include <list>
#include <mutex>
#include <optional>

template <typename T>
class Queue_on_list {

public:
    explicit Queue_on_list(std::list<T> d = std::list<T>{})
        : data{std::move(d)}
    {};

    std::optional<T> dequeue() {
        std::lock_guard<std::mutex> lock(mutex);
        if (data.empty()) {
            return std::nullopt;
        }
        auto res = std::optional<T>{data.front()};
        data.pop_front();
        return res;
    }

    void enqueue(const T& elem) {
        std::lock_guard<std::mutex> lock(mutex);
        data.push_back(elem);
    }

    std::list<T> get_data() const {
        return data;
    }

private:
    std::list<T> data;
    std::mutex mutex;
};
